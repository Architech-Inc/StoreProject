#!/usr/bin/env sh
set -e

# ==============================================================================
# OPS-12: Unified Per-Tenant Database Backup Engine
# Runs tenant-scoped MySQL + MongoDB snapshot dumps with:
#  - OpenSSL AES-256-CBC PBKDF2 encryption
#  - Cryptographic SHA-256 manifest generation
#  - Isolated tenant rotation (RETENTION_DAYS & RETENTION_COUNT)
#  - Remote S3 / MinIO offsite synchronization
# ==============================================================================

BACKUP_DIR="${BACKUP_DIR:-/backups}"
TENANT_SLUG="${TENANT_SLUG:-${TENANT_ID:-default}}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
RETENTION_COUNT="${RETENTION_COUNT:-14}"

TENANT_BASE_DIR="${BACKUP_DIR}/${TENANT_SLUG}"
mkdir -p "${TENANT_BASE_DIR}/mysql"
mkdir -p "${TENANT_BASE_DIR}/mongodb"
mkdir -p "${TENANT_BASE_DIR}/manifests"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
UTC_ISO=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

echo "[${UTC_ISO}] ============================================================"
echo "[${UTC_ISO}] Starting unified backup cycle for tenant '${TENANT_SLUG}'"
echo "[${UTC_ISO}] ============================================================"

# Execute MySQL dump
/app/mysql-backup.sh

# Execute MongoDB dump
/app/mongo-backup.sh

# Find newest generated dump files
MYSQL_LATEST=$(find "${TENANT_BASE_DIR}/mysql" -maxdepth 1 -type f \( -name "*.sql.gz" -o -name "*.sql.gz.enc" \) | sort | tail -n 1)
MONGO_LATEST=$(find "${TENANT_BASE_DIR}/mongodb" -maxdepth 1 -type f \( -name "*.archive.gz" -o -name "*.archive.gz.enc" \) | sort | tail -n 1)

MYSQL_NAME=$(basename "${MYSQL_LATEST:-none}")
MONGO_NAME=$(basename "${MONGO_LATEST:-none}")
MYSQL_SIZE=0
MONGO_SIZE=0
MYSQL_SHA="none"
MONGO_SHA="none"

if [ -f "$MYSQL_LATEST" ]; then
  MYSQL_SIZE=$(stat -c%s "$MYSQL_LATEST" 2>/dev/null || wc -c < "$MYSQL_LATEST")
  if [ -f "${MYSQL_LATEST}.sha256" ]; then
    MYSQL_SHA=$(awk '{print $1}' "${MYSQL_LATEST}.sha256")
  fi
fi

if [ -f "$MONGO_LATEST" ]; then
  MONGO_SIZE=$(stat -c%s "$MONGO_LATEST" 2>/dev/null || wc -c < "$MONGO_LATEST")
  if [ -f "${MONGO_LATEST}.sha256" ]; then
    MONGO_SHA=$(awk '{print $1}' "${MONGO_LATEST}.sha256")
  fi
fi

IS_ENCRYPTED="false"
if [ -n "$BACKUP_ENCRYPTION_KEY" ]; then
  IS_ENCRYPTED="true"
fi

MANIFEST_FILE="${TENANT_BASE_DIR}/manifests/${TENANT_SLUG}_manifest_${TIMESTAMP}.json"

cat <<EOF > "$MANIFEST_FILE"
{
  "tenantSlug": "${TENANT_SLUG}",
  "timestamp": "${UTC_ISO}",
  "isEncrypted": ${IS_ENCRYPTED},
  "encryptionAlgorithm": "AES-256-CBC-PBKDF2",
  "mysql": {
    "filename": "${MYSQL_NAME}",
    "sizeBytes": ${MYSQL_SIZE},
    "sha256": "${MYSQL_SHA}"
  },
  "mongodb": {
    "filename": "${MONGO_NAME}",
    "sizeBytes": ${MONGO_SIZE},
    "sha256": "${MONGO_SHA}"
  },
  "retentionPolicy": {
    "days": ${RETENTION_DAYS},
    "count": ${RETENTION_COUNT}
  },
  "status": "Completed"
}
EOF

# Optional S3 manifest sync
if [ -n "$S3_BUCKET" ] && [ -n "$AWS_ACCESS_KEY_ID" ] && [ -n "$AWS_SECRET_ACCESS_KEY" ]; then
  S3_ENDPOINT_ARG=""
  if [ -n "$S3_ENDPOINT" ]; then
    S3_ENDPOINT_ARG="--endpoint-url ${S3_ENDPOINT}"
  fi
  aws ${S3_ENDPOINT_ARG} s3 cp "$MANIFEST_FILE" "s3://${S3_BUCKET}/tenants/${TENANT_SLUG}/manifests/${TENANT_SLUG}_manifest_${TIMESTAMP}.json" || true
fi

# Prune old manifests
find "${TENANT_BASE_DIR}/manifests" -maxdepth 1 -type f -name "*.json" -mtime +"$RETENTION_DAYS" -exec rm -f {} \;
TOTAL_MANIFESTS=$(find "${TENANT_BASE_DIR}/manifests" -maxdepth 1 -type f -name "*.json" | wc -l)
if [ "$TOTAL_MANIFESTS" -gt "$RETENTION_COUNT" ]; then
  EXCESS=$((TOTAL_MANIFESTS - RETENTION_COUNT))
  find "${TENANT_BASE_DIR}/manifests" -maxdepth 1 -type f -name "*.json" | sort | head -n "$EXCESS" | xargs -r rm -f
fi

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Manifest recorded at ${MANIFEST_FILE}"
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Unified backup completed successfully."
