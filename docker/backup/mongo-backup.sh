#!/usr/bin/env sh
set -e

# MongoDB Automated Backup & S3/MinIO Replication Script
BACKUP_DIR="${BACKUP_DIR:-/backups/mongodb}"
MONGO_HOST="${MONGO_HOST:-store-mongodb}"
MONGO_PORT="${MONGO_PORT:-27017}"
MONGO_USER="${MONGO_USER:-admin}"
MONGO_PASSWORD="${MONGO_PASSWORD:-adminpassword}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
FILENAME="mongodb_${TIMESTAMP}.archive.gz"
FILEPATH="${BACKUP_DIR}/${FILENAME}"

mkdir -p "$BACKUP_DIR"

echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Starting MongoDB backup..."

# Execute mongodump with gzip archive
mongodump \
  --host="$MONGO_HOST" \
  --port="$MONGO_PORT" \
  --username="$MONGO_USER" \
  --password="$MONGO_PASSWORD" \
  --authenticationDatabase="admin" \
  --gzip \
  --archive="$FILEPATH"

BACKUP_SIZE=$(du -h "$FILEPATH" | cut -f1)
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] MongoDB backup completed: ${FILENAME} (${BACKUP_SIZE})"

# Optional S3 / MinIO offsite synchronization
if [ -n "$S3_BUCKET" ] && [ -n "$AWS_ACCESS_KEY_ID" ] && [ -n "$AWS_SECRET_ACCESS_KEY" ]; then
  S3_ENDPOINT_ARG=""
  if [ -n "$S3_ENDPOINT" ]; then
    S3_ENDPOINT_ARG="--endpoint-url ${S3_ENDPOINT}"
  fi
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Uploading backup to S3 bucket ${S3_BUCKET}..."
  aws ${S3_ENDPOINT_ARG} s3 cp "$FILEPATH" "s3://${S3_BUCKET}/mongodb/${FILENAME}"
  echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] S3 replication complete."
fi

# Prune old local backups past the retention window
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] Pruning local MongoDB backups older than ${RETENTION_DAYS} days..."
find "$BACKUP_DIR" -type f -name "*.archive.gz" -mtime +"$RETENTION_DAYS" -exec rm -f {} \;
echo "[$(date -u +"%Y-%m-%dT%H:%M:%SZ")] MongoDB backup maintenance cycle finished successfully."
