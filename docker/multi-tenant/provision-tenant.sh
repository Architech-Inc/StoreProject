#!/bin/bash
set -e

if [ "$#" -lt 2 ]; then
    echo "Usage: $0 <tenant_id> <domain>"
    echo "Example: $0 vendor1 vendor1.store.com"
    exit 1
fi

TENANT_ID=$1
DOMAIN=$2

# Create tenant directory
TENANT_DIR="tenants/${TENANT_ID}"
mkdir -p "$TENANT_DIR"

# Generate strong secrets
DB_PASSWORD=$(openssl rand -hex 16)
MONGO_PASSWORD=$(openssl rand -hex 16)
JWT_SECRET=$(openssl rand -base64 32)
OTP_PEPPER=$(openssl rand -hex 32)
MOMO_KEY=$(openssl rand -hex 16)
ROOT_PASSWORD=$(openssl rand -hex 16)

# Copy template
cp tenant-template.yml "$TENANT_DIR/docker-compose.yml"

# Create .env file for docker-compose interpolation
cat <<EOF > "$TENANT_DIR/.env"
TENANT_ID=${TENANT_ID}
STORE_DOMAIN=${DOMAIN}
MYSQL_ROOT_PASSWORD=${ROOT_PASSWORD}
MYSQL_DATABASE=store_db
MYSQL_USER=store_user
MYSQL_PASSWORD=${DB_PASSWORD}
MONGO_INITDB_ROOT_USERNAME=admin
MONGO_INITDB_ROOT_PASSWORD=${MONGO_PASSWORD}
STORE_CONNECTION_STRING=server=${TENANT_ID}-mysql;port=3306;database=store_db;user=store_user;password=${DB_PASSWORD};
STORE_MONGO_CONNECTION_STRING=mongodb://admin:${MONGO_PASSWORD}@${TENANT_ID}-mongodb:27017/?authSource=admin
STORE_JWT_SECRET=${JWT_SECRET}
STORE_OTP_PEPPER=${OTP_PEPPER}
STORE_MOMO_CALLBACK_KEY=${MOMO_KEY}
EOF

echo "Tenant $TENANT_ID provisioned in $TENANT_DIR"
echo "To start the tenant, run:"
echo "  cd $TENANT_DIR && docker compose up -d"
