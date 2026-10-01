# Tenant Operations Runbook & Standard Operating Procedures (SOP) — ClexAn Foods

> **Classification: Internal Operations Manual**  
> **Target Audience:** DevOps Engineers, Platform Administrators, Site Reliability Engineers (SRE), and Level 2/3 Support Staff.  
> **Governs:** Complete tenant operational lifecycle across ClexAn Foods multi-tenant retail stacks.

---

## 1. System Architecture & Multi-Tenant Stacking

Each tenant in the ClexAn Foods platform operates as a dedicated, network-isolated container stack orchestrated by `Store.ControlPlane` and fronted by the `Traefik 2.10` edge proxy.

```
Incoming Request: https://{slug}.store.domain.com
                          │
                          ▼
               ┌───────────────────────┐
               │  Traefik Edge Proxy   │ ── TLS Termination & Routing
               └──────────┬────────────┘
                          │ (Docker network: proxy-network)
                          ▼
            ┌─────────────────────────────┐
            │ Tenant Virtual Stack        │
            ├─────────────────────────────┤
            │ • store-{slug}-ui (Port 80) │
            │ • store-{slug}-api (5000)   │
            │ • store-{slug}-mysql (3306) │
            │ • store-{slug}-mongo (27017)│
            │ • clamav-rest (8080)        │
            └─────────────────────────────┘
```

---

## 2. Standard Operating Procedures (SOP)

### SOP-01: Provisioning a New Tenant Store

Tenant provisioning can be initiated through four channels:
1. **Public SaaS Portal**: Self-service onboarding via `Store.TenantPortal`.
2. **Platform Task Runner**: Interactive CLI runner on the host.
3. **Shell Script**: Standalone automation script.
4. **ControlPlane REST API**: Programmatic API invocation.

#### Method A: Using Platform Task Runners (Recommended)
```bash
# On Linux / macOS:
make provision-tenant SLUG=bastos-market STORE="Bastos Fresh Market" EMAIL=admin@bastos.cm PASSWORD="SuperSecret123!"

# On Windows:
.\tasks.ps1 provision -Slug "bastos-market" -StoreName "Bastos Fresh Market" -AdminEmail "admin@bastos.cm" -AdminPassword "SuperSecret123!"
```

#### Method B: Standalone Script
```bash
# Bash:
./scripts/provision-tenant.sh "Bastos Fresh Market" "bastos-market" "admin@bastos.cm" "SuperSecret123!" "XAF" "http://localhost:5050"

# PowerShell:
.\scripts\provision-tenant.ps1 -StoreName "Bastos Fresh Market" -Slug "bastos-market" -AdminEmail "admin@bastos.cm" -AdminPassword "SuperSecret123!"
```

#### Method C: Direct ControlPlane API
```bash
curl -X POST "http://localhost:5050/api/control/tenants/provision" \
  -H "Content-Type: application/json" \
  -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
  -d '{
    "storeName": "Bastos Fresh Market",
    "slug": "bastos-market",
    "adminEmail": "admin@bastos.cm",
    "adminUsername": "admin",
    "adminPassword": "SuperSecret123!",
    "currency": "XAF",
    "planTier": 1
  }'
```

#### Verification Checklist:
- [ ] Control Plane returns `HTTP 200 OK` with `status: "Provisioned"`.
- [ ] Containers are active: `docker ps --filter "name=bastos-market"` returns API, UI, MySQL, and Mongo.
- [ ] Ingress routing is active: `curl -I https://bastos-market.store.127.0.0.1.nip.io` returns `HTTP 200` or `302`.
- [ ] Default administrator can log in to the UI and access `/Dashboard`.

---

### SOP-02: Custom Domain & SSL Certificate Binding

When an enterprise customer requests their own branded domain (e.g. `pos.bastosfresh.cm`):

1. **DNS Configuration**:
   - Instruct the customer to create a DNS `CNAME` or `A` record pointing to the public VPS IP:
     ```
     pos.bastosfresh.cm.   IN  A   51.159.x.x
     ```

2. **Register Domain in ControlPlane**:
   ```bash
   curl -X POST "http://localhost:5050/api/control/tenants/{tenantId}/domains" \
     -H "Content-Type: application/json" \
     -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
     -d '{
       "customDomain": "pos.bastosfresh.cm",
       "isPrimary": true
     }'
   ```

3. **Traefik Route Generation**:
   - ControlPlane automatically appends a Traefik Host router rule:
     ```yaml
     rule: "Host(`bastos-market.store.example.com`, `pos.bastosfresh.cm`)"
     ```
   - Traefik automatically negotiates Let's Encrypt TLS certificates via the configured ACME challenge.

---

### SOP-03: Plan Quota Adjustments & Tier Upgrades

Tenants operate under tiered limits (Starter = 1 branch, Pro = 5 branches, Enterprise = unlimited).

If a tenant receives `HTTP 402 QuotaExceeded` when adding a branch or user:

1. **Check Current Usage & Limits**:
   ```bash
   curl -X GET "http://localhost:5050/api/control/tenants/{tenantId}/quotas" \
     -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY"
   ```

2. **Upgrade Plan Tier**:
   ```bash
   curl -X POST "http://localhost:5050/api/control/tenants/{tenantId}/upgrade-plan" \
     -H "Content-Type: application/json" \
     -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
     -d '{
       "planTier": 2, // 0 = Starter, 1 = Pro, 2 = Enterprise
       "notes": "Upgraded to Enterprise tier per sales order #SO-4819"
     }'
   ```

3. **Verify Quota Gate Resolution**:
   - Attempting the mutation in the tenant console should immediately succeed without restarting containers.

---

### SOP-04: Tenant Suspension & Resumption

Tenants may be suspended due to overdue billing arrears, suspected fraud, or legal compliance freeze.

#### Suspending a Tenant:
```bash
curl -X POST "http://localhost:5050/api/control/tenants/{tenantId}/suspend" \
  -H "Content-Type: application/json" \
  -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
  -d '{
    "reason": "Overdue payment - 30 days past due",
    "effectiveImmediately": true
  }'
```
- **System Behavior**:
  - Tenant status transitions to `Suspended`.
  - Traefik routing dynamically directs HTTP requests to a standardized suspension notice (`/suspended` on the Tenant Portal).
  - Background cron jobs and outbound integrations are paused.
  - Database containers remain running in read-only mode to prevent data corruption.

#### Resuming a Tenant:
```bash
curl -X POST "http://localhost:5050/api/control/tenants/{tenantId}/resume" \
  -H "Content-Type: application/json" \
  -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
  -d '{
    "reason": "Payment confirmed via MoMo transaction #MM-8921"
  }'
```
- **System Behavior**:
  - Tenant status transitions back to `Active`.
  - Normal Traefik routing is immediately restored.

---

### SOP-05: Backup & Offsite Snapshot Procedures

Tenant data must be backed up daily to offsite encrypted storage.

#### Manual On-Demand Tenant Backup:
```bash
# Using Task Runner:
make backup
# or PowerShell:
.\tasks.ps1 backup

# Or executing the container backup directly:
docker exec store-backup-runner /bin/backup-tenant.sh bastos-market
```

- **Output Artifacts**:
  - MySQL dump: `/backups/bastos-market/mysql_{timestamp}.sql.gz`
  - MongoDB dump: `/backups/bastos-market/mongo_{timestamp}.archive.gz`
- **Encryption & Offsite Upload**:
  - Snapshots are encrypted with AES-256 using `CONTROLPLANE_MASTER_KEY` and synced to the S3 bucket defined by `BACKUP_S3_BUCKET`.

---

### SOP-06: Disaster Recovery & Database Restoration

To restore a tenant after catastrophic data corruption or human error:

1. **Locate Target Snapshot**:
   ```bash
   aws s3 ls s3://clexan-backups/bastos-market/
   ```

2. **Execute Restore**:
   ```bash
   # Using PowerShell:
   .\scripts\restore-database.ps1 -Slug "bastos-market" -SnapshotFile "mysql_20261001_020000.sql.gz"

   # Or Bash:
   ./scripts/restore-database.sh "bastos-market" "mysql_20261001_020000.sql.gz"
   ```

3. **Verify Database Integrity & Migrations**:
   ```bash
   # Check container connectivity:
   docker exec -it store-bastos-market-api curl -s http://localhost:5000/health
   ```

---

### SOP-07: Deprovisioning & Offboarding a Tenant

When a contract is terminated:

1. **Data Retention & OHADA Export**:
   - Before teardown, trigger an immutable export of all sales invoices, general ledger entries, and audit logs:
     ```bash
     curl -X GET "http://localhost:5050/api/control/tenants/{tenantId}/export-compliance" \
       -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
       -o "compliance_export_{tenantId}.zip"
     ```
   - Store compliance archive in long-term cold storage (10-year retention per OHADA regulations).

2. **Deprovision Tenant Stack**:
   ```bash
   curl -X DELETE "http://localhost:5050/api/control/tenants/{tenantId}" \
     -H "X-Master-Key: $CONTROLPLANE_MASTER_KEY" \
     -d '{"confirmPermanentDeletion": true}'
   ```

3. **Infrastructure Teardown**:
   - ControlPlane issues `docker compose down -v` on the tenant stack.
   - Volumes are safely purged after backup confirmation.
   - Master cryptographic secrets for the tenant are permanently shredded from the key vault.

---

## 3. Incident Triage & Diagnostic Quick Reference

| Symptom | Diagnostic Command | Common Root Cause & Fix |
|---|---|---|
| **502 Bad Gateway on Tenant UI** | `docker logs store-{slug}-ui` | UI container crashed. Check `ApiBaseUrl` configuration in container env. |
| **Tenant API throws 500 on start** | `docker logs store-{slug}-api` | MySQL database not ready or placeholder secret detected. Verify DB healthcheck. |
| **Traefik 404 Not Found** | `curl -H "Host: traefik.local" http://localhost:8080/api/http/routers` | Container missing `traefik.enable=true` label or network disconnected from `proxy-network`. |
| **ClamAV Connection Refused** | `docker ps --filter "name=clamav"` | ClamAV sidecar container is stopped or still downloading virus definitions. Check RAM availability (≥1GB). |
| **Database Connection Pool Exhausted** | `docker exec store-{slug}-mysql mysqladmin -u root -p status` | High concurrency or connection leak. Verify `.AsNoTracking()` on queries and restart API container. |
