#!/usr/bin/env sh
set -e

# MySQL Automated Backup & S3/MinIO Replication Script
BACKUP_DIR="${BACKUP_DIR:-/backups/mysql}"
MYSQL_HOST="${MYSQL_HOST:-store-mysql}"
MYSQL_PORT="${MYSQL_PORT:-3306}"
MYSQL_USER="${MYSQL_USER:-root}"
MYSQL_PASSWORD="${MYSQL_PASSWORD:-rootpassword}"
MYSQL_DATABASE="${MYSQL_DATABASE:-store_db_v2}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
FILENAME="${MYSQL_DATABASE}_${TIMESTAMP}.sql.gz"
FILEPATH="${BACKUP_DIR}/${FILENAME}"

mkdir -p "$BACKUP_DIR"

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Starting MySQL backup of database '${MYSQL_DATABASE}'..."

# Execute mysqldump with single-transaction lock and gzip compression
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

BACKUP_SIZE=$(du -h "$FILEPATH" | cut -f1)
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] MySQL backup completed: ${FILENAME} (${BACKUP_SIZE})"

# Optional S3 / MinIO offsite synchronization
if [ -n "$S3_BUCKET" ] && [ -n "$AWS_ACCESS_KEY_ID" ] && [ -n "$AWS_SECRET_ACCESS_KEY" ]; then
  S3_ENDPOINT_ARG=""
  if [ -n "$S3_ENDPOINT" ]; then
    S3_ENDPOINT_ARG="--endpoint-url ${S3_ENDPOINT}"
  fi
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Uploading backup to S3 bucket ${S3_BUCKET}..."
  aws ${S3_ENDPOINT_ARG} s3 cp "$FILEPATH" "s3://${S3_BUCKET}/mysql/${FILENAME}"
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] S3 replication complete."
fi

# Prune old local backups past the retention window
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Pruning local backups older than ${RETENTION_DAYS} days..."
find "$BACKUP_DIR" -type f -name "*.sql.gz" -mtime +"$RETENTION_DAYS" -exec rm -f {} \;
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Backup maintenance cycle finished successfully."
