#!/usr/bin/env bash
set -e

# SYNOPSIS: Instantly creates a timestamped MySQL and MongoDB snapshot from running containers.
# EXAMPLE: ./scripts/backup-now.sh

OUTPUT_DIR="${1:-./backups}"
MYSQL_CONTAINER="${2:-store-mysql}"
MONGO_CONTAINER="${3:-store-mongodb}"
DATABASE_NAME="${4:-store_db_v2}"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
mkdir -p "$OUTPUT_DIR"

echo "Creating instant backup snapshot (${TIMESTAMP})..."

# 1. MySQL Dump
MYSQL_FILE="${OUTPUT_DIR}/${DATABASE_NAME}_${TIMESTAMP}.sql.gz"
echo "Dumping MySQL database '${DATABASE_NAME}' from '${MYSQL_CONTAINER}'..."
docker exec "$MYSQL_CONTAINER" mysqldump -u root -prootpassword --single-transaction --quick "$DATABASE_NAME" | gzip -9 > "$MYSQL_FILE"
echo "MySQL snapshot saved: ${MYSQL_FILE}"

# 2. MongoDB Dump
MONGO_FILE="${OUTPUT_DIR}/mongodb_${TIMESTAMP}.archive.gz"
echo "Dumping MongoDB from '${MONGO_CONTAINER}'..."
docker exec "$MONGO_CONTAINER" mongodump -u admin -padminpassword --authenticationDatabase=admin --gzip --archive > "$MONGO_FILE"
echo "MongoDB snapshot saved: ${MONGO_FILE}"

echo "Instant backup complete! All snapshots saved in ${OUTPUT_DIR}"
