#!/usr/bin/env bash
set -e

# ==============================================================================
# OPS-12: Disaster Recovery Database Restore Script
# Restores MySQL and MongoDB snapshots from plaintext, gzip, or AES-256-CBC
# encrypted archives into target containers.
#
# USAGE:
#   ./scripts/restore-database.sh <MYSQL_BACKUP> <MONGO_BACKUP> [MYSQL_CONTAINER] [MONGO_CONTAINER] [DATABASE_NAME]
# EXAMPLE:
#   BACKUP_ENCRYPTION_KEY="secret" ./scripts/restore-database.sh ./backups/kim-sm/store_kim-sm_20261001.sql.gz.enc ./backups/kim-sm/mongodb_20261001.archive.gz.enc kim-sm-mysql kim-sm-mongodb store_kim-sm
# ==============================================================================

MYSQL_BACKUP="${1:-}"
MONGO_BACKUP="${2:-}"
MYSQL_CONTAINER="${3:-store-mysql}"
MONGO_CONTAINER="${4:-store-mongodb}"
DATABASE_NAME="${5:-store_db_v2}"

verify_checksum() {
    local file="$1"
    local checksum_file="${file}.sha256"
    if [ -f "$checksum_file" ]; then
        echo "Verifying SHA-256 checksum for $(basename "$file")..."
        (cd "$(dirname "$file")" && sha256sum -c "$(basename "$checksum_file")")
        echo "Checksum verified successfully."
    else
        echo "Note: No .sha256 checksum file found for $(basename "$file"), skipping hash check."
    fi
}

# 1. Restore MySQL
if [ -n "$MYSQL_BACKUP" ] && [ -f "$MYSQL_BACKUP" ]; then
    verify_checksum "$MYSQL_BACKUP"
    echo "Restoring MySQL database '${DATABASE_NAME}' from '${MYSQL_BACKUP}' into container '${MYSQL_CONTAINER}'..."
    
    if [[ "$MYSQL_BACKUP" == *.enc ]]; then
        if [ -z "$BACKUP_ENCRYPTION_KEY" ]; then
            echo "Error: ${MYSQL_BACKUP} is encrypted, but BACKUP_ENCRYPTION_KEY environment variable is not set." >&2
            exit 1
        fi
        echo "Decrypting AES-256-CBC stream and restoring to MySQL..."
        openssl enc -d -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -in "$MYSQL_BACKUP" \
            | gunzip -c \
            | docker exec -i "$MYSQL_CONTAINER" mysql -u root -prootpassword "$DATABASE_NAME"
    elif [[ "$MYSQL_BACKUP" == *.gz ]]; then
        gunzip -c "$MYSQL_BACKUP" | docker exec -i "$MYSQL_CONTAINER" mysql -u root -prootpassword "$DATABASE_NAME"
    else
        docker exec -i "$MYSQL_CONTAINER" mysql -u root -prootpassword "$DATABASE_NAME" < "$MYSQL_BACKUP"
    fi
    echo "MySQL restore complete."
fi

# 2. Restore MongoDB
if [ -n "$MONGO_BACKUP" ] && [ -f "$MONGO_BACKUP" ]; then
    verify_checksum "$MONGO_BACKUP"
    echo "Restoring MongoDB from '${MONGO_BACKUP}' into container '${MONGO_CONTAINER}'..."
    
    if [[ "$MONGO_BACKUP" == *.enc ]]; then
        if [ -z "$BACKUP_ENCRYPTION_KEY" ]; then
            echo "Error: ${MONGO_BACKUP} is encrypted, but BACKUP_ENCRYPTION_KEY environment variable is not set." >&2
            exit 1
        fi
        echo "Decrypting AES-256-CBC stream and restoring to MongoDB..."
        openssl enc -d -aes-256-cbc -salt -pbkdf2 -pass env:BACKUP_ENCRYPTION_KEY -in "$MONGO_BACKUP" \
            | docker exec -i "$MONGO_CONTAINER" mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive
    else
        docker exec -i "$MONGO_CONTAINER" mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive < "$MONGO_BACKUP"
    fi
    echo "MongoDB restore complete."
fi

echo "Disaster recovery restore process finished successfully."
