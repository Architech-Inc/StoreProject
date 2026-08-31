#!/usr/bin/env bash
set -e

# SYNOPSIS: Provisions a new isolated tenant store container stack.
# EXAMPLE: ./scripts/provision-tenant.sh "Bastos Fresh Market" "bastos-market" "admin@bastos.cm" "SuperSecret123!"

STORE_NAME="${1:-Bastos Market}"
SLUG="${2:-bastos-market}"
ADMIN_EMAIL="${3:-admin@bastos.cm}"
ADMIN_PASSWORD="${4:-SuperSecret123!}"
CURRENCY="${5:-XAF}"
CONTROL_PLANE_URL="${6:-http://localhost:5050}"

JSON_PAYLOAD=$(cat <<EOF
{
  "storeName": "$STORE_NAME",
  "slug": "$SLUG",
  "adminEmail": "$ADMIN_EMAIL",
  "adminUsername": "admin",
  "adminPassword": "$ADMIN_PASSWORD",
  "currency": "$CURRENCY",
  "planTier": 1
}
EOF
)

echo "Provisioning tenant '$SLUG' via Control Plane at $CONTROL_PLANE_URL..."

curl -s -X POST "$CONTROL_PLANE_URL/api/control/tenants/provision" \
  -H "Content-Type: application/json" \
  -d "$JSON_PAYLOAD" | jq .
