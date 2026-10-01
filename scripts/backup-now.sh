#!/usr/bin/env bash
set -e

# ==============================================================================
# OPS-12: Instant Snapshot Backup Script for MySQL & MongoDB
# Supports tenant isolation, OpenSSL AES-256-CBC PBKDF2 encryption, SHA-256
# integrity checksums, and retention rotation.
#
# USAGE:
#   ./scripts/backup-now.sh [OUTPUT_DIR] [MYSQL_CONTAINER] [MONGO_CONTAINER] [DATABASE_NAME]
# Or with flags / environment:
#   TENANT_SLUG="acme" BACKUP_ENCRYPTION_KEY="secret" ./scripts/backup-now.sh
# ==============================================================================

OUTPUT_DIR="${1:-./backups}"
TENANT_SLUG="${TENANT_SLUG:-}"
MYSQL_CONTAINER="${2:-}"
MONGO_CONTAINER="${3:-}"
DATABASE_NAME="${4:-}"
RETENTION_COUNT="${RETENTION_COUNT:-14}"

if [ -n "$TENANT_SLUG" ]; then
  MYSQL_CONTAINER="${MYSQL_CONTAINER:-${TENANT_SLUG}-mysql}"
  MONGO_CONTAINER="${MONGO_CONTAINER:-${TENANT_SLUG}-mongodb}"
  DATABASE_NAME="${DATABASE_NAME:-store_${TENANT_SLUG}}"
  TARGET_DIR="${OUTPUT_DIR}/${TENANT_SLUG}"
else
  MYSQL_CONTAINER="${MYSQL_CONTAINER:-store-mysql}"
  MONGO_CONTAINER="${MONGO_CONTAINER:-store-mongodb}"
  DATABASE_NAME="${DATABASE_NAME:-store_db_v2}"
  TARGET_DIR="${OUTPUT_DIR}/standalone"
fi

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
mkdir -p "$TARGET_DIR"

echo "============================================================"
echo "Creating instant backup snapshot (${TIMESTAMP})"
echo "Tenant:       ${TENANT_SLUG:-Standalone}"
echo "MySQL:        ${MYSQL_CONTAINER} (DB: ${DATABASE_NAME})"
echo "MongoDB:      ${MONGO_CONTAINER}"
echo "Target Dir:   ${TARGET_DIR}"
echo "Encryption:   $([ -n "$BACKUP_ENCRYPTION_KEY" ] && echo "AES-256-CBC Enabled" || echo "Disabled")"
echo "============================================================"

# 1. MySQL Dump
if [ -n "$BACKUP_ENCRYPTION_KEY" ]; then
  MYSQL_FILE="${TARGET_DIR}/${DATABASE_NAME}_${TIMESTAMP}.sql.gz.enc"
  echo "Dumping and encrypting MySQL database '${DATABASE_NAME}' from '${MYSQL_CONTAINER}'..."
  docker exec "$MYSQL_CONTAINER" mysqldump -u root -prootpassword --single-transaction --quick "$DATABASE_NAME" \
    | gzip -9 \
    | openssl enc -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -out "$MYSQL_FILE"
else
  MYSQL_FILE="${TARGET_DIR}/${DATABASE_NAME}_${TIMESTAMP}.sql.gz"
  echo "Dumping MySQL database '${DATABASE_NAME}' from '${MYSQL_CONTAINER}'..."
  docker exec "$MYSQL_CONTAINER" mysqldump -u root -prootpassword --single-transaction --quick "$DATABASE_NAME" \
    | gzip -9 > "$MYSQL_FILE"
fi

(cd "$TARGET_DIR" && sha256sum "$(basename "$MYSQL_FILE")" > "$(basename "$MYSQL_FILE").sha256")
echo "MySQL snapshot saved: ${MYSQL_FILE}"

# 2. MongoDB Dump
if [ -n "$BACKUP_ENCRYPTION_KEY" ]; then
  MONGO_FILE="${TARGET_DIR}/mongodb_${TIMESTAMP}.archive.gz.enc"
  echo "Dumping and encrypting MongoDB from '${MONGO_CONTAINER}'..."
  docker exec "$MONGO_CONTAINER" mongodump -u admin -padminpassword --authenticationDatabase=admin --gzip --archive \
    | openssl enc -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -out "$MONGO_FILE"
else
  MONGO_FILE="${TARGET_DIR}/mongodb_${TIMESTAMP}.archive.gz"
  echo "Dumping MongoDB from '${MONGO_CONTAINER}'..."
  docker exec "$MONGO_CONTAINER" mongodump -u admin -padminpassword --authenticationDatabase=admin --gzip --archive > "$MONGO_FILE"
fi

(cd "$TARGET_DIR" && sha256sum "$(basename "$MONGO_FILE")" > "$(basename "$MONGO_FILE").sha256")
echo "MongoDB snapshot saved: ${MONGO_FILE}"

# 3. Retention Rotation (prune oldest files exceeding RETENTION_COUNT)
TOTAL_MYSQL=$(find "$TARGET_DIR" -maxdepth 1 -type f \( -name "*.sql.gz" -o -name "*.sql.gz.enc" \) | wc -l)
if [ "$TOTAL_MYSQL" -gt "$RETENTION_COUNT" ]; then
  EXCESS=$((TOTAL_MYSQL - RETENTION_COUNT))
  echo "Pruning ${EXCESS} old MySQL snapshot(s) beyond retention limit (${RETENTION_COUNT})..."
  find "$TARGET_DIR" -maxdepth 1 -type f \( -name "*.sql.gz" -o -name "*.sql.gz.enc" \) | sort | head -n "$EXCESS" | while read -r old_file; do
    rm -f "$old_file" "${old_file}.sha256"
  done
fi

TOTAL_MONGO=$(find "$TARGET_DIR" -maxdepth 1 -type f \( -name "*.archive.gz" -o -name "*.archive.gz.enc" \) | wc -l)
if [ "$TOTAL_MONGO" -gt "$RETENTION_COUNT" ]; then
  EXCESS=$((TOTAL_MONGO - RETENTION_COUNT))
  echo "Pruning ${EXCESS} old MongoDB snapshot(s) beyond retention limit (${RETENTION_COUNT})..."
  find "$TARGET_DIR" -maxdepth 1 -type f \( -name "*.archive.gz" -o -name "*.archive.gz.enc" \) | sort | head -n "$EXCESS" | while read -r old_file; do
    rm -f "$old_file" "${old_file}.sha256"
  done
fi

echo "Instant backup complete! All snapshots and checksums saved in ${TARGET_DIR}"
