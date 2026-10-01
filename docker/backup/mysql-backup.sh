#!/usr/bin/env sh
set -e

# ==============================================================================
# OPS-12: Tenant-Scoped MySQL Backup & Encrypted Archiving Script
# Supports isolated per-tenant backups, OpenSSL AES-256-CBC PBKDF2 encryption,
# SHA-256 integrity verification, S3/MinIO replication, and retention rotation.
# ==============================================================================

BACKUP_DIR="${BACKUP_DIR:-/backups}"
TENANT_SLUG="${TENANT_SLUG:-${TENANT_ID:-default}}"
MYSQL_HOST="${MYSQL_HOST:-store-mysql}"
MYSQL_PORT="${MYSQL_PORT:-3306}"
MYSQL_USER="${MYSQL_USER:-root}"
MYSQL_PASSWORD="${MYSQL_PASSWORD:-rootpassword}"
MYSQL_DATABASE="${MYSQL_DATABASE:-store_${TENANT_SLUG}}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
RETENTION_COUNT="${RETENTION_COUNT:-14}"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
TENANT_MYSQL_DIR="${BACKUP_DIR}/${TENANT_SLUG}/mysql"
mkdir -p "$TENANT_MYSQL_DIR"

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Starting MySQL backup for database '${MYSQL_DATABASE}'..."

# Determine encryption mode
if [ -n "$BACKUP_ENCRYPTION_KEY" ]; then
  FILENAME="${TENANT_SLUG}_mysql_${TIMESTAMP}.sql.gz.enc"
  FILEPATH="${TENANT_MYSQL_DIR}/${FILENAME}"
  
  # Dump -> Gzip -> OpenSSL AES-256-CBC with PBKDF2
  mysqldump \
    --host="$MYSQL_HOST" \
    --port="$MYSQL_PORT" \
    --user="$MYSQL_USER" \
    --password="$MYSQL_PASSWORD" \
    --single-transaction \
    --quick \
    --routines \
    --triggers \
    "$MYSQL_DATABASE" | gzip -9 | openssl enc -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -out "$FILEPATH"
  
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Encrypted MySQL backup created: ${FILENAME}"
else
  FILENAME="${TENANT_SLUG}_mysql_${TIMESTAMP}.sql.gz"
  FILEPATH="${TENANT_MYSQL_DIR}/${FILENAME}"
  
  # Dump -> Gzip (Plaintext)
  mysqldump \
    --host="$MYSQL_HOST" \
    --port="$MYSQL_PORT" \
    --user="$MYSQL_USER" \
    --password="$MYSQL_PASSWORD" \
    --single-transaction \
    --quick \
    --routines \
    --triggers \
    "$MYSQL_DATABASE" | gzip -9 > "$FILEPATH"
  
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Warning: BACKUP_ENCRYPTION_KEY not set. Unencrypted backup created: ${FILENAME}"
fi

# Generate SHA-256 integrity checksum
(cd "$TENANT_MYSQL_DIR" && sha256sum "$FILENAME" > "${FILENAME}.sha256")
BACKUP_SIZE=$(du -h "$FILEPATH" | cut -f1)
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] MySQL snapshot completed: ${FILENAME} (${BACKUP_SIZE})"

# Optional S3 / MinIO tenant-isolated offsite synchronization
if [ -n "$S3_BUCKET" ] && [ -n "$AWS_ACCESS_KEY_ID" ] && [ -n "$AWS_SECRET_ACCESS_KEY" ]; then
  S3_ENDPOINT_ARG=""
  if [ -n "$S3_ENDPOINT" ]; then
    S3_ENDPOINT_ARG="--endpoint-url ${S3_ENDPOINT}"
  fi
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Uploading snapshot to S3 bucket ${S3_BUCKET}/tenants/${TENANT_SLUG}/mysql/..."
  aws ${S3_ENDPOINT_ARG} s3 cp "$FILEPATH" "s3://${S3_BUCKET}/tenants/${TENANT_SLUG}/mysql/${FILENAME}"
  aws ${S3_ENDPOINT_ARG} s3 cp "${FILEPATH}.sha256" "s3://${S3_BUCKET}/tenants/${TENANT_SLUG}/mysql/${FILENAME}.sha256"
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] S3 replication complete."
fi

# Isolated Tenant Retention Rotation:
# 1. Prune backups older than RETENTION_DAYS
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Pruning backups older than ${RETENTION_DAYS} days in ${TENANT_MYSQL_DIR}..."
find "$TENANT_MYSQL_DIR" -maxdepth 1 -type f \( -name "*.sql.gz*" -o -name "*.sha256" \) -mtime +"$RETENTION_DAYS" -exec rm -f {} \;

# 2. Enforce RETENTION_COUNT (keep the N most recent backups)
TOTAL_BACKUPS=$(find "$TENANT_MYSQL_DIR" -maxdepth 1 -type f \( -name "*.sql.gz" -o -name "*.sql.gz.enc" \) | wc -l)
if [ "$TOTAL_BACKUPS" -gt "$RETENTION_COUNT" ]; then
  EXCESS=$((TOTAL_BACKUPS - RETENTION_COUNT))
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Total snapshots (${TOTAL_BACKUPS}) exceeds retention count (${RETENTION_COUNT}). Pruning oldest ${EXCESS} snapshots..."
  find "$TENANT_MYSQL_DIR" -maxdepth 1 -type f \( -name "*.sql.gz" -o -name "*.sql.gz.enc" \) | sort | head -n "$EXCESS" | while read -r old_backup; do
    echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] Removing expired snapshot: $(basename "$old_backup")"
    rm -f "$old_backup" "${old_backup}.sha256"
  done
fi

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] [Tenant: ${TENANT_SLUG}] MySQL backup maintenance cycle finished successfully."
