#!/usr/bin/env pwsh
# =============================================================================
# ClexAn Foods (StoreProject) — Production VPS Provisioning Script (OPS-05, OPS-06, OPS-09)
# =============================================================================
# This script provisions a fresh Linux VPS (Ubuntu/Debian) for ClexAn platform hosting.
# It configures:
#   1. Secure deploy user and SSH key authentication
#   2. Docker Engine & Docker Compose v2
#   3. Global proxy network (`proxy-network`)
#   4. Edge Proxy (Traefik v3) with automated Let's Encrypt TLS (ACME) (OPS-05)
#   5. Watchtower automated container image updates with cleanup flags (OPS-06)
#   6. Non-root / sudo privilege validation (OPS-09)
#
# Usage:
#   .\scripts\provision-docker-vps.ps1 -HostIp "157.173.112.19" -AcmeEmail "admin@store.clexan.com"
# =============================================================================

param(
    [Parameter(Mandatory=$true, HelpMessage="The public IP address or hostname of the target VPS")]
    [string]$HostIp,

    [string]$AdminUser = "root",

    [string]$DeployUser = "deploy",
    
    [string]$PublicKeyPath = "$env:USERPROFILE\.ssh\id_ed25519.pub",
    
    [int]$Port = 22,

    [string]$AcmeEmail = "admin@store.clexan.com",

    [bool]$EnableWatchtower = $true
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " ClexAn Foods — Production VPS Provisioner               " -ForegroundColor Cyan
Write-Host " Automated Let's Encrypt TLS (ACME) & Watchtower Stack   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

if (-not (Test-Path $PublicKeyPath)) {
    Write-Host "[ERROR] SSH public key not found at: $PublicKeyPath" -ForegroundColor Red
    Write-Host "Generate an SSH key pair using 'ssh-keygen -t ed25519' or supply a valid path via -PublicKeyPath." -ForegroundColor Yellow
    exit 1
}

$PublicKey = (Get-Content $PublicKeyPath -Raw).Trim()

$BashScript = @"
#!/bin/bash
set -e

# OPS-09: Validate root / sudo privilege execution
if [ "`$(id -u)" -ne 0 ]; then
    echo "[ERROR] Setup script must be executed with root or sudo privileges." >&2
    exit 1
fi

NEW_USER="$DeployUser"
PUB_KEY="$PublicKey"
ACME_EMAIL="$AcmeEmail"
ENABLE_WATCHTOWER="$($EnableWatchtower.ToString().ToLower())"

echo "==> [1/6] Updating system packages and installing prerequisites..."
export DEBIAN_FRONTEND=noninteractive
apt-get update -y -q || true
apt-get upgrade -y -q || true
apt-get install -y -q curl wget ufw fail2ban sudo ca-certificates gnupg || true

echo "==> [2/6] Configuring deployment user (`$NEW_USER)..."
if ! id -u "`$NEW_USER" >/dev/null 2>&1; then
    adduser --disabled-password --gecos "" "`$NEW_USER"
    usermod -aG sudo "`$NEW_USER"
    echo "`$NEW_USER ALL=(ALL) NOPASSWD:ALL" > /etc/sudoers.d/`$NEW_USER
    chmod 440 /etc/sudoers.d/`$NEW_USER
fi

echo "==> [3/6] Setting up SSH authorized keys for `$NEW_USER..."
mkdir -p /home/`$NEW_USER/.ssh
echo "`$PUB_KEY" > /home/`$NEW_USER/.ssh/authorized_keys
chmod 700 /home/`$NEW_USER/.ssh
chmod 600 /home/`$NEW_USER/.ssh/authorized_keys
chown -R `$NEW_USER:`$NEW_USER /home/`$NEW_USER/.ssh

echo "==> [4/6] Installing Docker Engine & Docker Compose Plugin..."
if ! command -v docker >/dev/null 2>&1; then
    curl -fsSL https://get.docker.com -o /tmp/get-docker.sh
    sh /tmp/get-docker.sh
    rm -f /tmp/get-docker.sh
fi
usermod -aG docker "`$NEW_USER"

echo "==> [5/6] Creating Docker proxy network and directory structures..."
docker network create proxy-network 2>/dev/null || true
mkdir -p /opt/projects/proxy/letsencrypt /opt/projects/proxy/dynamic /opt/projects/platform/tenants
touch /opt/projects/proxy/letsencrypt/acme.json
chmod 600 /opt/projects/proxy/letsencrypt/acme.json

echo "==> [6/6] Writing Traefik Edge Proxy & Watchtower compose configuration (OPS-05, OPS-06)..."
cat << 'EOF' > /opt/projects/proxy/docker-compose.yml
version: '3.8'

services:
  traefik:
    image: traefik:v3.0
    container_name: traefik
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - "/var/run/docker.sock:/var/run/docker.sock:ro"
      - "./letsencrypt:/letsencrypt"
      - "./dynamic:/etc/traefik/dynamic:ro"
    command:
      - "--api.insecure=false"
      - "--api.dashboard=false"
      - "--providers.docker=true"
      - "--providers.docker.exposedbydefault=false"
      - "--providers.file.directory=/etc/traefik/dynamic"
      - "--providers.file.watch=true"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
      # Automated HTTP -> HTTPS Redirection
      - "--entrypoints.web.http.redirections.entrypoint.to=websecure"
      - "--entrypoints.web.http.redirections.entrypoint.scheme=https"
      - "--entrypoints.web.http.redirections.entrypoint.permanent=true"
      # Automated Let's Encrypt TLS ACME Resolver (OPS-05)
      - "--certificatesresolvers.letsencrypt.acme.email=${ACME_EMAIL}"
      - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
      - "--certificatesresolvers.letsencrypt.acme.httpchallenge=true"
      - "--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web"
      - "--certificatesresolvers.letsencrypt.acme.tlschallenge=true"
      - "--entrypoints.websecure.http.tls.certresolver=letsencrypt"
    networks:
      - proxy-network
    labels:
      - "com.centurylinklabs.watchtower.enable=false"

  # Watchtower: Scheduled container updates with image cleanup (OPS-06)
  watchtower:
    image: containrrr/watchtower:latest
    container_name: watchtower
    restart: unless-stopped
    command: --schedule "0 0 4 * * *" --cleanup --include-stopped --label-enable
    environment:
      - WATCHTOWER_SCHEDULE=0 0 4 * * *
      - WATCHTOWER_CLEANUP=true
      - WATCHTOWER_INCLUDE_STOPPED=true
      - WATCHTOWER_LABEL_ENABLE=true
      - WATCHTOWER_ROLLING_RESTART=true
      - WATCHTOWER_TIMEOUT=60s
    volumes:
      - "/var/run/docker.sock:/var/run/docker.sock:ro"
    networks:
      - proxy-network
    labels:
      - "com.centurylinklabs.watchtower.enable=false"

networks:
  proxy-network:
    external: true
EOF

chown -R `$NEW_USER:`$NEW_USER /opt/projects
chmod 600 /opt/projects/proxy/letsencrypt/acme.json

cd /opt/projects/proxy
docker compose up -d

echo "==> Generating GitHub Actions deployment SSH key for CI/CD..."
if [ ! -f /home/`$NEW_USER/.ssh/github_deploy ]; then
    su - "`$NEW_USER" -c "ssh-keygen -t ed25519 -C 'github-actions-deploy' -f ~/.ssh/github_deploy -N ''"
    cat /home/`$NEW_USER/.ssh/github_deploy.pub >> /home/`$NEW_USER/.ssh/authorized_keys
fi

echo ""
echo "=========================================================="
echo " VPS BOOTSTRAP COMPLETE!                                  "
echo " Traefik ACME TLS & Watchtower services are active.        "
echo "=========================================================="
echo " GitHub Actions Private Deployment Key (store as VPS_SSH_KEY):"
echo ""
cat /home/`$NEW_USER/.ssh/github_deploy
echo ""
echo "=========================================================="
"@

Write-Host "Connecting to $AdminUser@$HostIp on port $Port..." -ForegroundColor Green
Write-Host "You may be prompted for the password if SSH key authentication is not yet configured." -ForegroundColor Yellow

$SudoCmd = if ($AdminUser -eq "root") { "bash" } else { "sudo bash" }
$Base64Script = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($BashScript))
$RemoteCmd = "echo '$Base64Script' | base64 -d > /tmp/setup.sh && $SudoCmd /tmp/setup.sh"

ssh -t -p $Port "$AdminUser@$HostIp" $RemoteCmd

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n[SUCCESS] VPS provisioned with Traefik ACME TLS and Watchtower." -ForegroundColor Green
} else {
    Write-Host "`n[ERROR] VPS provisioning failed. Exit code: $LASTEXITCODE" -ForegroundColor Red
}
