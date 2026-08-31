#!/usr/bin/env bash
set -e

# SYNOPSIS: Restores a MySQL or MongoDB snapshot into running containers.
# EXAMPLE: ./scripts/restore-database.sh ./backups/store_db_v2_20260831.sql.gz ./backups/mongodb_20260831.archive.gz

MYSQL_BACKUP="${1:-}"
MONGO_BACKUP="${2:-}"
MYSQL_CONTAINER="${3:-store-mysql}"
MONGO_CONTAINER="${4:-store-mongodb}"
DATABASE_NAME="${5:-store_db_v2}"

if [ -n "$MYSQL_BACKUP" ] && [ -f "$MYSQL_BACKUP" ]; then
    echo "Restoring MySQL database '${DATABASE_NAME}' from '${MYSQL_BACKUP}'..."
    if [[ "$MYSQL_BACKUP" == *.gz ]]; then
        gunzip -c "$MYSQL_BACKUP" | docker exec -i "$MYSQL_CONTAINER" mysql -u root -prootpassword "$DATABASE_NAME"
    else
        docker exec -i "$MYSQL_CONTAINER" mysql -u root -prootpassword "$DATABASE_NAME" < "$MYSQL_BACKUP"
    fi
    echo "MySQL restore complete."
fi

if [ -n "$MONGO_BACKUP" ] && [ -f "$MONGO_BACKUP" ]; then
    echo "Restoring MongoDB from '${MONGO_BACKUP}'..."
    docker exec -i "$MONGO_CONTAINER" mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive < "$MONGO_BACKUP"
    echo "MongoDB restore complete."
fi

echo "Disaster recovery restore process finished."
