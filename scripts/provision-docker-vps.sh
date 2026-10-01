#!/usr/bin/env bash
# =============================================================================
# ClexAn Foods (StoreProject) — Production VPS Provisioning Script (OPS-05, OPS-06, OPS-09)
# =============================================================================
# Usage:
#   ./scripts/provision-docker-vps.sh [HOST_IP] [ADMIN_USER] [DEPLOY_USER] [ACME_EMAIL]
# =============================================================================

set -e

HOST_IP="${1:?Target VPS Host IP is required}"
ADMIN_USER="${2:-root}"
DEPLOY_USER="${3:-deploy}"
ACME_EMAIL="${4:-admin@store.clexan.com}"
SSH_PORT="${PORT:-22}"
SSH_PUB_KEY="${SSH_PUB_KEY:-$HOME/.ssh/id_ed25519.pub}"

if [ ! -f "$SSH_PUB_KEY" ]; then
    echo "[ERROR] SSH public key not found at $SSH_PUB_KEY" >&2
    exit 1
fi

PUB_KEY_CONTENT=$(cat "$SSH_PUB_KEY")

echo "==> Provisioning VPS at $HOST_IP with Traefik ACME TLS and Watchtower..."

ssh -t -p "$SSH_PORT" "$ADMIN_USER@$HOST_IP" bash -c "'
set -e
if [ \"\$(id -u)\" -ne 0 ]; then
    echo \"[ERROR] Script must be run as root or with sudo.\" >&2
    exit 1
fi

export DEBIAN_FRONTEND=noninteractive
apt-get update -y -q || true
apt-get install -y -q curl wget ufw fail2ban sudo ca-certificates gnupg || true

if ! id -u \"$DEPLOY_USER\" >/dev/null 2>&1; then
    adduser --disabled-password --gecos \"\" \"$DEPLOY_USER\"
    usermod -aG sudo \"$DEPLOY_USER\"
    echo \"$DEPLOY_USER ALL=(ALL) NOPASSWD:ALL\" > /etc/sudoers.d/$DEPLOY_USER
    chmod 440 /etc/sudoers.d/$DEPLOY_USER
fi

mkdir -p /home/$DEPLOY_USER/.ssh
echo \"$PUB_KEY_CONTENT\" > /home/$DEPLOY_USER/.ssh/authorized_keys
chmod 700 /home/$DEPLOY_USER/.ssh
chmod 600 /home/$DEPLOY_USER/.ssh/authorized_keys
chown -R $DEPLOY_USER:$DEPLOY_USER /home/$DEPLOY_USER/.ssh

if ! command -v docker >/dev/null 2>&1; then
    curl -fsSL https://get.docker.com -o /tmp/get-docker.sh
    sh /tmp/get-docker.sh
    rm -f /tmp/get-docker.sh
fi
usermod -aG docker \"$DEPLOY_USER\"

docker network create proxy-network 2>/dev/null || true
mkdir -p /opt/projects/proxy/letsencrypt /opt/projects/proxy/dynamic /opt/projects/platform/tenants /opt/projects/backups
chmod 700 /opt/projects/backups
touch /opt/projects/proxy/letsencrypt/acme.json
chmod 600 /opt/projects/proxy/letsencrypt/acme.json

cat << 'EOF' > /opt/projects/proxy/docker-compose.yml
version: '3.8'

services:
  traefik:
    image: traefik:v3.0
    container_name: traefik
    restart: unless-stopped
    ports:
      - \"80:80\"
      - \"443:443\"
    volumes:
      - \"/var/run/docker.sock:/var/run/docker.sock:ro\"
      - \"./letsencrypt:/letsencrypt\"
      - \"./dynamic:/etc/traefik/dynamic:ro\"
    command:
      - \"--api.insecure=false\"
      - \"--api.dashboard=false\"
      - \"--providers.docker=true\"
      - \"--providers.docker.exposedbydefault=false\"
      - \"--providers.file.directory=/etc/traefik/dynamic\"
      - \"--providers.file.watch=true\"
      - \"--entrypoints.web.address=:80\"
      - \"--entrypoints.websecure.address=:443\"
      - \"--entrypoints.web.http.redirections.entrypoint.to=websecure\"
      - \"--entrypoints.web.http.redirections.entrypoint.scheme=https\"
      - \"--entrypoints.web.http.redirections.entrypoint.permanent=true\"
      - \"--certificatesresolvers.letsencrypt.acme.email=$ACME_EMAIL\"
      - \"--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json\"
      - \"--certificatesresolvers.letsencrypt.acme.httpchallenge=true\"
      - \"--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web\"
      - \"--certificatesresolvers.letsencrypt.acme.tlschallenge=true\"
      - \"--entrypoints.websecure.http.tls.certresolver=letsencrypt\"
    networks:
      - proxy-network
    labels:
      - \"com.centurylinklabs.watchtower.enable=false\"

  watchtower:
    image: containrrr/watchtower:latest
    container_name: watchtower
    restart: unless-stopped
    command: --schedule \"0 0 4 * * *\" --cleanup --include-stopped --label-enable
    environment:
      - WATCHTOWER_SCHEDULE=0 0 4 * * *
      - WATCHTOWER_CLEANUP=true
      - WATCHTOWER_INCLUDE_STOPPED=true
      - WATCHTOWER_LABEL_ENABLE=true
      - WATCHTOWER_ROLLING_RESTART=true
      - WATCHTOWER_TIMEOUT=60s
    volumes:
      - \"/var/run/docker.sock:/var/run/docker.sock:ro\"
    networks:
      - proxy-network
    labels:
      - \"com.centurylinklabs.watchtower.enable=false\"

  # Seq: Centralized Log Shipping & Observability (OPS-07)
  store-seq:
    image: datalust/seq:latest
    container_name: store-seq
    restart: unless-stopped
    environment:
      - ACCEPT_EULA=Y
    volumes:
      - /opt/projects/proxy/seq-data:/data
    networks:
      - proxy-network
    labels:
      - \"com.centurylinklabs.watchtower.enable=false\"

networks:
  proxy-network:
    external: true
EOF

mkdir -p /opt/projects/proxy/seq-data
chown -R $DEPLOY_USER:$DEPLOY_USER /opt/projects
chmod 600 /opt/projects/proxy/letsencrypt/acme.json

cd /opt/projects/proxy
docker compose up -d

echo \"[SUCCESS] VPS setup complete with Traefik ACME TLS, Watchtower, and Seq Logging.\"
'"
