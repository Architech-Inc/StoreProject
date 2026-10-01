#!/usr/bin/env sh
set -e

# ==============================================================================
# OPS-12: Tenant-Scoped MongoDB Backup & Encrypted Archiving Script
# Supports isolated per-tenant backups, OpenSSL AES-256-CBC PBKDF2 encryption,
# SHA-256 integrity verification, S3/MinIO replication, and retention rotation.
# ==============================================================================

BACKUP_DIR="${BACKUP_DIR:-/backups}"
TENANT_SLUG="${TENANT_SLUG:-${TENANT_ID:-default}}"
MONGO_HOST="${MONGO_HOST:-store-mongodb}"
MONGO_PORT="${MONGO_PORT:-27017}"
MONGO_USER="${MONGO_USER:-admin}"
MONGO_PASSWORD="${MONGO_PASSWORD:-adminpassword}"
MONGO_DATABASE="${MONGO_DATABASE:-store_${TENANT_SLUG}}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
RETENTION_COUNT="${RETENTION_COUNT:-14}"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
TENANT_MONGO_DIR="${BACKUP_DIR}/${TENANT_SLUG}/mongodb"
mkdir -p "$TENANT_MONGO_DIR"

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Starting MongoDB backup for database '${MONGO_DATABASE}'..."

# Configure DB scope argument if specified
DB_ARG=""
if [ -n "$MONGO_DATABASE" ] && [ "$MONGO_DATABASE" != "all" ]; then
  DB_ARG="--db=${MONGO_DATABASE}"
fi

# Determine encryption mode
if [ -n "$BACKUP_ENCRYPTION_KEY" ]; then
  FILENAME="${TENANT_SLUG}_mongodb_${TIMESTAMP}.archive.gz.enc"
  FILEPATH="${TENANT_MONGO_DIR}/${FILENAME}"
  
  # Dump -> Gzip Archive -> OpenSSL AES-256-CBC with PBKDF2
  mongodump \
    --host="$MONGO_HOST" \
    --port="$MONGO_PORT" \
    --username="$MONGO_USER" \
    --password="$MONGO_PASSWORD" \
    --authenticationDatabase="admin" \
    ${DB_ARG} \
    --gzip \
    --archive | openssl enc -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -out "$FILEPATH"
    
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Encrypted MongoDB backup created: ${FILENAME}"
else
  FILENAME="${TENANT_SLUG}_mongodb_${TIMESTAMP}.archive.gz"
  FILEPATH="${TENANT_MONGO_DIR}/${FILENAME}"
  
  # Dump -> Gzip Archive (Plaintext)
  mongodump \
    --host="$MONGO_HOST" \
    --port="$MONGO_PORT" \
    --username="$MONGO_USER" \
    --password="$MONGO_PASSWORD" \
    --authenticationDatabase="admin" \
    ${DB_ARG} \
    --gzip \
    --archive="$FILEPATH"
    
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Warning: BACKUP_ENCRYPTION_KEY not set. Unencrypted backup created: ${FILENAME}"
fi

# Generate SHA-256 integrity checksum
(cd "$TENANT_MONGO_DIR" && sha256sum "$FILENAME" > "${FILENAME}.sha256")
BACKUP_SIZE=$(du -h "$FILEPATH" | cut -f1)
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] MongoDB snapshot completed: ${FILENAME} (${BACKUP_SIZE})"

# Optional S3 / MinIO tenant-isolated offsite synchronization
if [ -n "$S3_BUCKET" ] && [ -n "$AWS_ACCESS_KEY_ID" ] && [ -n "$AWS_SECRET_ACCESS_KEY" ]; then
  S3_ENDPOINT_ARG=""
  if [ -n "$S3_ENDPOINT" ]; then
    S3_ENDPOINT_ARG="--endpoint-url ${S3_ENDPOINT}"
  fi
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Uploading snapshot to S3 bucket ${S3_BUCKET}/tenants/${TENANT_SLUG}/mongodb/..."
  aws ${S3_ENDPOINT_ARG} s3 cp "$FILEPATH" "s3://${S3_BUCKET}/tenants/${TENANT_SLUG}/mongodb/${FILENAME}"
  aws ${S3_ENDPOINT_ARG} s3 cp "${FILEPATH}.sha256" "s3://${S3_BUCKET}/tenants/${TENANT_SLUG}/mongodb/${FILENAME}.sha256"
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] S3 replication complete."
fi

# Isolated Tenant Retention Rotation:
# 1. Prune backups older than RETENTION_DAYS
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Pruning backups older than ${RETENTION_DAYS} days in ${TENANT_MONGO_DIR}..."
find "$TENANT_MONGO_DIR" -maxdepth 1 -type f \( -name "*.archive.gz*" -o -name "*.sha256" \) -mtime +"$RETENTION_DAYS" -exec rm -f {} \;

# 2. Enforce RETENTION_COUNT (keep the N most recent backups)
TOTAL_BACKUPS=$(find "$TENANT_MONGO_DIR" -maxdepth 1 -type f \( -name "*.archive.gz" -o -name "*.archive.gz.enc" \) | wc -l)
if [ "$TOTAL_BACKUPS" -gt "$RETENTION_COUNT" ]; then
  EXCESS=$((TOTAL_BACKUPS - RETENTION_COUNT))
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Total snapshots (${TOTAL_BACKUPS}) exceeds retention count (${RETENTION_COUNT}). Pruning oldest ${EXCESS} snapshots..."
  find "$TENANT_MONGO_DIR" -maxdepth 1 -type f \( -name "*.archive.gz" -o -name "*.archive.gz.enc" \) | sort | head -n "$EXCESS" | while read -r old_backup; do
    echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Removing expired snapshot: $(basename "$old_backup")"
    rm -f "$old_backup" "${old_backup}.sha256"
  done
fi

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] MongoDB backup maintenance cycle finished successfully."
