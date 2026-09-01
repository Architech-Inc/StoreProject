# ClexAn Foods StoreProject — Multi-Tenant Architecture

**Document Version:** 1.0
**Date:** September 2026
**Scope:** Full technical reference for the Docker-Silo multi-tenancy model, Control Plane internals, tenant lifecycle, networking, security boundaries, and operational runbook.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Isolation Model — Docker Silos](#2-isolation-model--docker-silos)
3. [Store.ControlPlane Service](#3-storecontrolplane-service)
4. [Tenant Lifecycle](#4-tenant-lifecycle)
5. [Networking and Routing](#5-networking-and-routing)
6. [Secret Management](#6-secret-management)
7. [Health Monitoring](#7-health-monitoring)
8. [Tenant Data Model](#8-tenant-data-model)
9. [Control Plane REST API](#9-control-plane-rest-api)
10. [Configuration Reference](#10-configuration-reference)
11. [Security Boundaries](#11-security-boundaries)
12. [Backup and Disaster Recovery](#12-backup-and-disaster-recovery)
13. [Operational Runbook](#13-operational-runbook)
14. [Scaling Considerations](#14-scaling-considerations)
15. [Related Files](#15-related-files)

---

## 1. Architecture Overview

ClexAn Foods StoreProject is a **SaaS multi-tenant ERP platform** built on a **Docker-Silo isolation model**.
Every tenant (store franchise) receives a fully isolated set of containers — its own database engines, application services, and network segment.
There is **zero database or process sharing** between tenants.

```
+----------------------------------------------------------------------+
|                         Host Server / VPS                            |
|                                                                      |
|  +----------------------+    +------------------------------------+  |
|  |  Store.ControlPlane  |    |      Traefik Reverse Proxy         |  |
|  |  (Port 9999)         |    |      (proxy-network)               |  |
|  |  - TenantsController |    +------------------+-----------------+  |
|  |  - TenantOrchestrator|                       |                    |
|  |  - HealthMonitor     |                       | Routes by Host     |
|  +----------+-----------+                       |                    |
|             | Provisions                        |                    |
|             v                                   |                    |
|  +-----------------------------------+          |                    |
|  |  Tenant Silo: "acme"             |<---------+ acme.store.domain  |
|  |                                   |          | api.acme.store...  |
|  |  +----------+  +-------------+   |                               |
|  |  | acme-api |  |   acme-ui   |   |                               |
|  |  +----+-----+  +-------------+   |                               |
|  |       |                          |                               |
|  |  +----v------+  +-----------+    |                               |
|  |  | acme-mysql|  |acme-mongo |    |                               |
|  |  +-----------+  +-----------+    |                               |
|  |  Network: acme_net               |                               |
|  +-----------------------------------+                              |
|                                                                      |
|  +-----------------------------------+                              |
|  |  Tenant Silo: "northgate"        |<-------- northgate.store...   |
|  |  (Independent identical stack)   |                               |
|  +-----------------------------------+                              |
+----------------------------------------------------------------------+
```

### Core Principles

| Principle | Implementation |
|:---|:---|
| **Complete Data Isolation** | Each tenant has its own MySQL database (`store_{slug}`) and MongoDB instance |
| **Process Isolation** | 4 dedicated containers per tenant (MySQL, MongoDB, API, UI) |
| **Network Isolation** | Each tenant has a private Docker bridge network (`{slug}_net`) |
| **Secret Isolation** | Per-tenant cryptographic secrets (JWT, DB passwords, MoMo keys) |
| **Failure Isolation** | One tenant container failure cannot affect another silo |
| **Subdomain Routing** | Traefik routes `{slug}.domain` to UI, `api.{slug}.domain` to API |

---

## 2. Isolation Model — Docker Silos

Each tenant is provisioned as an **independent Docker Compose stack** generated from a parameterized template:

```
Store.ControlPlane/Templates/docker-compose.tenant.template.yml
```

### Per-Tenant Container Stack

| Container | Image | Role |
|:---|:---|:---|
| `{slug}-mysql` | `mysql:8.0` | Relational data store (transactions, RBAC, inventory) |
| `{slug}-mongodb` | `mongo:latest` | Document store (audit logs, notifications, analytics) |
| `{slug}-api` | `store-api:latest` | ASP.NET Core REST API backend |
| `{slug}-ui` | `store-ui:latest` | ASP.NET Core Razor Pages frontend |

### Per-Tenant Volume Set

| Volume | Purpose |
|:---|:---|
| `{slug}_mysql_data` | MySQL data files — persistent across container restarts |
| `{slug}_mongodb_data` | MongoDB data directory |
| `{slug}_dataprotection_keys` | ASP.NET Data Protection key ring (shared by API and UI) |
| `{slug}_uploads` | User-uploaded media (product images, avatars) |

### Network Topology

```
{slug}_net (private bridge)                proxy-network (external shared)
      |                                            |
      +-- {slug}-mysql  (internal only)            |
      +-- {slug}-mongodb (internal only)           |
      +-- {slug}-api  --------------------------> (Traefik exposed)
      +-- {slug}-ui   --------------------------> (Traefik exposed)
```

- Databases are **not reachable** from the external network or from other tenant networks.
- Only the API and UI containers join `proxy-network` and are reachable through Traefik.

### Dependency Chain and Health Gates

```
mysql (healthy) --+
                  +--> api (healthy) --> ui (starts)
mongo (healthy) --+
```

All `depends_on` use `condition: service_healthy` — the API never starts against an unready database, and the UI never starts against an unready API.

---

## 3. Store.ControlPlane Service

`Store.ControlPlane` is a standalone ASP.NET Core Web API — the **orchestration brain** for all tenant lifecycle management. It runs as a separate service alongside the primary stack.

### Project Structure

```
Store.ControlPlane/
+-- Controllers/
|   +-- TenantsController.cs             # REST API surface (8 endpoints)
+-- Services/
|   +-- ITenantOrchestrator.cs           # Orchestrator contract
|   +-- TenantOrchestrator.cs            # Full lifecycle implementation
+-- Workers/
|   +-- TenantHealthMonitorWorker.cs     # Background health polling (60s interval)
+-- Repositories/
|   +-- ITenantRepository.cs             # Storage abstraction
|   +-- JsonFileTenantRepository.cs      # File-backed registry with SemaphoreSlim lock
+-- Models/
|   +-- Tenant.cs                        # Core domain entity
|   +-- TenantEnums.cs                   # TenantStatus, TenantTier enums
|   +-- TenantSecretsAndLogs.cs          # TenantSecrets, TenantProvisioningLog
|   +-- DTOs/
|       +-- TenantDtos.cs                # ProvisionTenantRequest, TenantDto, TenantDetailDto
+-- Templates/
|   +-- docker-compose.tenant.template.yml
+-- App_Data/
|   +-- tenants.json                     # Live tenant registry (JSON flat-file)
+-- Tenants/
    +-- {slug}/
        +-- docker-compose.yml           # Generated per-tenant compose specification
```

### Dependency Injection Registration

```csharp
// Program.cs
builder.Services.AddSingleton<ITenantRepository, JsonFileTenantRepository>();
builder.Services.AddScoped<ITenantOrchestrator, TenantOrchestrator>();
builder.Services.AddHostedService<TenantHealthMonitorWorker>();
```

- `ITenantRepository` is a **Singleton** — one shared JSON file lock guards concurrent access.
- `ITenantOrchestrator` is **Scoped** — fresh instance per HTTP request.
- `TenantHealthMonitorWorker` is a **hosted background service** polling every 60 seconds, using `CreateScope()` to safely resolve scoped services.

---

## 4. Tenant Lifecycle

### State Machine

```
                  POST /provision
                        |
                        v
                  [ Provisioning ]
                        |
            +-----------+-----------+
            |                       |
       Docker OK?           Docker failed?
            |                       |
            v                       v
         [Active]               [Failed]
            |
   +--------+--------+
   |                 |
POST /suspend   DELETE /{id}
   |                 |
   v                 v
[Suspended]      [Terminated]
   |
POST /resume
   |
   v
 [Active]
```

### Status Definitions

| Status | Description |
|:---|:---|
| `Pending` | Tenant record created, provisioning not yet initiated |
| `Provisioning` | Docker Compose stack is being deployed |
| `Active` | All containers running and health checks passing |
| `Suspended` | Stack stopped (containers halted, data volumes preserved) |
| `Failed` | Docker deployment returned a non-zero exit code |
| `Terminated` | Fully deprovisioned; containers and volumes destroyed |

### Provisioning Flow (Step by Step)

```json
POST /api/control/tenants/provision
{
  "storeName": "Acme Foods",
  "slug": "acme",
  "adminEmail": "admin@acme.com",
  "adminUsername": "admin",
  "adminPassword": "SuperSecure123!",
  "currency": "XAF",
  "planTier": 1
}
```

1. **Slug Validation** — Check against `ReservedSlugs` blocklist.
2. **Uniqueness Check** — Query `ITenantRepository.SlugExistsAsync()`.
3. **URL Construction** — Compute `uiUrl` and `apiUrl` from slug and root domain.
4. **Secret Generation** — Generate 5 cryptographic secrets via `RandomNumberGenerator.Fill()`.
5. **Directory Creation** — `Tenants/{slug}/` workspace directory created on the host.
6. **Compose Generation** — Template rendered with tenant values and written to `Tenants/{slug}/docker-compose.yml`.
7. **Docker Deployment** (if `AutoDeployDocker: true`) — `docker compose up -d` executed.
8. **Registry Save** — Tenant persisted to `App_Data/tenants.json`.
9. **Response** — `201 Created` with `TenantDto` payload.

### Reserved Slug Blocklist

```csharp
private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
{
    "admin", "api", "control", "system", "root", "www",
    "mail", "db", "dashboard", "auth", "login", "register",
    "status", "health", "portal", "store", "app"
};
```

---

## 5. Networking and Routing

### URL Scheme

| Component | Pattern | Example |
|:---|:---|:---|
| Tenant UI | `http://{slug}.{rootDomain}:18080` | `http://acme.store.157.173.112.19.nip.io:18080` |
| Tenant API | `http://api.{slug}.{rootDomain}:18080` | `http://api.acme.store.157.173.112.19.nip.io:18080` |
| Control Plane API | `http://{host}:9999` | Internal management endpoint |

### Traefik Label Configuration

Each tenant container auto-registers with Traefik via Docker labels in the generated compose file:

```yaml
# API Container
- "traefik.enable=true"
- "traefik.docker.network=proxy-network"
- "traefik.http.routers.{slug}-api.rule=Host(`api.{slug}.{rootDomain}`)"
- "traefik.http.services.{slug}-api.loadbalancer.server.port=8080"

# UI Container
- "traefik.enable=true"
- "traefik.docker.network=proxy-network"
- "traefik.http.routers.{slug}-ui.rule=Host(`{slug}.{rootDomain}`)"
- "traefik.http.services.{slug}-ui.loadbalancer.server.port=8080"
```

Traefik auto-discovers containers on `proxy-network` — **no Traefik restart required** when a new tenant is provisioned.

### Internal vs External Communication

| Communication Path | Channel |
|:---|:---|
| UI to API (server-side) | `http://{slug}-api:8080` via `{slug}_net` bridge (no Traefik overhead) |
| Browser client to API | `http://api.{slug}.{rootDomain}:18080` via Traefik |
| API to MySQL | `Server={slug}-mysql;Port=3306` via `{slug}_net` |
| API to MongoDB | `mongodb://admin:{pass}@{slug}-mongodb:27017` via `{slug}_net` |
| ControlPlane health check | `{tenant.ApiUrl}/health` via Traefik |

### CORS Isolation

Each tenant API is bootstrapped with its own subdomain in `AllowedOrigins`:

```yaml
- Cors__AllowedOrigins__0=http://{slug}.{rootDomain}:18080
- Cors__AllowedOrigins__1=https://{slug}.{rootDomain}:18443
```

A tenant's browser UI cannot make cross-origin calls to another tenant's API.

---

## 6. Secret Management

All secrets are generated at provisioning time using `System.Security.Cryptography.RandomNumberGenerator.Fill()` — a CSPRNG backed generator. No secret is ever reused between tenants.

```csharp
private static string GenerateSecureSecret(int length)
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
    var bytes = new byte[length];
    RandomNumberGenerator.Fill(bytes);
    var sb = new StringBuilder(length);
    foreach (var b in bytes) sb.Append(chars[b % chars.Length]);
    return sb.ToString();
}
```

### Secret Inventory Per Tenant

| Secret | Length | Usage |
|:---|:---:|:---|
| `MySqlRootPassword` | 24 | MySQL root user (initial setup only) |
| `MySqlUserPassword` | 24 | `store_user` application database user |
| `MongoDbRootPassword` | 24 | MongoDB admin user |
| `JwtSecret` | 48 | JWT token signing key (`Jwt__Key`) |
| `MoMoCallbackKey` | 32 | MTN MoMo payment webhook HMAC signing key |

### Secret Storage

Secrets are stored in `App_Data/tenants.json` on the Control Plane host. The `TenantDto` API responses **never expose secrets** — `TenantSecrets` is intentionally excluded from all DTO mappings.

> **Warning:** `App_Data/tenants.json` contains plaintext database passwords and JWT signing keys. This file must be:
> - Excluded from version control (`.gitignore`)
> - Protected with strict filesystem permissions
> - Backed up with encryption in production

---

## 7. Health Monitoring

### TenantHealthMonitorWorker

A `BackgroundService` that polls every Active tenant's `/health` endpoint every **60 seconds**.

```csharp
// Uses CreateScope() to safely resolve scoped services from singleton background worker
using var scope = _services.CreateScope();
var orchestrator = scope.ServiceProvider.GetRequiredService<ITenantOrchestrator>();

var activeTenants = (await repo.GetAllAsync()).Where(t => t.Status == TenantStatus.Active);
foreach (var tenant in activeTenants)
    await orchestrator.CheckTenantHealthAsync(tenant.TenantId, stoppingToken);
```

### Health Check Logic

```
GET {tenant.ApiUrl}/health   (5-second timeout)
  |
  +-- HTTP 2xx  --> IsHealthy = true  | "Tenant API & Database healthy."
  +-- HTTP 4xx/5xx --> IsHealthy = false | "Health check returned HTTP {code}."
  +-- Timeout / DNS error --> IsHealthy = false | "Unreachable: {message}"
```

Result is persisted to `tenants.json` after every check, making `LastHealthCheck`, `IsHealthy`, and `LastHealthMessage` always current.

### Health Summary

`GET /api/control/tenants/summary` returns aggregate fleet health:

```json
{
  "totalTenants": 12,
  "activeTenants": 11,
  "provisioningTenants": 1,
  "suspendedTenants": 0,
  "failedTenants": 0,
  "healthyCount": 10,
  "unhealthyCount": 1
}
```

---

## 8. Tenant Data Model

### Core Entity

```csharp
public class Tenant
{
    public Guid   TenantId          { get; set; }  // Unique identifier
    public string Name              { get; set; }  // Display name ("Acme Foods")
    public string Slug              { get; set; }  // URL-safe subdomain ("acme")
    public string AdminEmail        { get; set; }  // Owner email
    public string AdminUsername     { get; set; }  // Admin login username
    public string Currency          { get; set; }  // Default currency code (e.g., "XAF")
    public TenantStatus Status      { get; set; }  // Lifecycle state
    public TenantTier   PlanTier    { get; set; }  // Subscription plan
    public string CustomDomain      { get; set; }  // Optional vanity domain
    public string UiUrl             { get; set; }  // Provisioned UI URL
    public string ApiUrl            { get; set; }  // Provisioned API URL
    public TenantSecrets Secrets    { get; set; }  // Cryptographic secrets (never in DTOs)
    public List<TenantProvisioningLog> ProvisioningLogs { get; set; }
    public DateTime  DateCreated    { get; set; }
    public DateTime? LastHealthCheck{ get; set; }
    public bool      IsHealthy      { get; set; }
    public string?   LastHealthMessage { get; set; }
}
```

### Plan Tiers

| Value | Name | Description |
|:---:|:---|:---|
| `0` | `Starter` | Entry-level plan |
| `1` | `Professional` | Default provisioning tier |
| `2` | `Enterprise` | Full feature access |

### Provisioning Audit Trail

Every lifecycle event writes a `TenantProvisioningLog` entry:

| StepName | Description |
|:---|:---|
| `Validation` | Slug and uniqueness validated |
| `SecretGeneration` | Cryptographic secrets generated |
| `ComposeGeneration` | `docker-compose.yml` written to workspace |
| `DockerDeployment` | `docker compose up -d` executed and result recorded |
| `BlueprintReady` | Compose file ready for manual deployment |
| `Suspension` | Stack stopped via `docker compose stop` |
| `Resumption` | Stack started via `docker compose start` |

---

## 9. Control Plane REST API

**Base URL:** `http://{controlPlaneHost}:9999/api/control`
**Swagger:** `http://{controlPlaneHost}:9999/swagger` *(Development only)*

### Endpoint Reference

| Method | Path | Description |
|:---|:---|:---|
| `GET` | `/tenants` | List all tenants |
| `GET` | `/tenants/summary` | Fleet health summary |
| `GET` | `/tenants/{id}` | Tenant detail with full provisioning log |
| `POST` | `/tenants/provision` | Provision a new tenant silo |
| `POST` | `/tenants/{id}/suspend` | Suspend (stop) a tenant stack |
| `POST` | `/tenants/{id}/resume` | Resume a suspended tenant stack |
| `POST` | `/tenants/{id}/health` | Trigger an immediate health check |
| `DELETE` | `/tenants/{id}` | Deprovision and permanently destroy a tenant silo |

### Provision Request Schema

```json
{
  "storeName":      "Acme Foods Ltd",    // Required, 3–100 chars
  "slug":           "acme-foods",        // Required, 3–50 chars, lowercase a-z 0-9 and hyphen
  "adminEmail":     "admin@acme.com",    // Required, valid email
  "adminUsername":  "admin",             // Required, 3–50 chars
  "adminPassword":  "SecurePass123!",    // Required, min 8 chars
  "currency":       "XAF",              // Optional, defaults to "XAF"
  "planTier":       1                    // 0=Starter, 1=Professional, 2=Enterprise
}
```

### Response Envelope (`ApiResponse<T>`)

```json
{
  "success": true,
  "message": "Tenant stack provisioned successfully.",
  "data": {
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Acme Foods Ltd",
    "slug": "acme-foods",
    "status": "Active",
    "planTier": "Professional",
    "uiUrl": "http://acme-foods.store.157.173.112.19.nip.io:18080",
    "apiUrl": "http://api.acme-foods.store.157.173.112.19.nip.io:18080",
    "isHealthy": true,
    "dateCreated": "2026-09-01T09:00:00Z"
  }
}
```

---

## 10. Configuration Reference

### `Store.ControlPlane/appsettings.json`

```json
{
  "ControlPlane": {
    "RootDomain":       "store.157.173.112.19.nip.io",
    "AutoDeployDocker": false,
    "StoreApiImage":    "store-api:latest",
    "StoreUiImage":     "store-ui:latest"
  }
}
```

### Key Configuration Flags

| Key | Default | Description |
|:---|:---|:---|
| `ControlPlane:RootDomain` | `store.157.173.112.19.nip.io` | Wildcard root domain — change to your actual domain in production |
| `ControlPlane:AutoDeployDocker` | `false` | `false` = generate compose only; `true` = auto-run `docker compose up -d` |
| `ControlPlane:StoreApiImage` | `store-api:latest` | API container image — pin to semantic version tags in production |
| `ControlPlane:StoreUiImage` | `store-ui:latest` | UI container image — pin to semantic version tags in production |

### Production Environment Variable Overrides

```bash
ControlPlane__RootDomain=store.yourcompany.com
ControlPlane__AutoDeployDocker=true
ControlPlane__StoreApiImage=ghcr.io/architech-inc/store-api:v2.1.0
ControlPlane__StoreUiImage=ghcr.io/architech-inc/store-ui:v2.1.0
```

---

## 11. Security Boundaries

### Tenant Isolation Guarantees

| Boundary | Mechanism | Enforced By |
|:---|:---|:---|
| Database isolation | Separate MySQL instance per silo | Docker networking + unique credentials |
| Network isolation | Private bridge per silo (`{slug}_net`) | Docker networking |
| JWT isolation | Unique signing key per tenant | Per-tenant `JwtSecret` |
| MoMo callback isolation | Unique HMAC key per tenant | Per-tenant `MoMoCallbackKey` |
| CORS isolation | Only own subdomain in `AllowedOrigins` | Store.API CORS middleware |
| Data protection isolation | Separate key ring volume per silo | `{slug}_dataprotection_keys` Docker volume |

### Reserved Slug Protection

The `ReservedSlugs` blocklist prevents subdomain collisions with system infrastructure (`admin`, `api`, `control`, `system`, `auth`, etc.), preventing routing hijacks.

### Control Plane Security Notice

> **Important:** The Control Plane API has no authentication in the current implementation. In production the Control Plane endpoint **must** be:
> - Firewalled to internal management IPs only
> - Or protected with an API key or mTLS policy
> - **Never** directly exposed on a public internet endpoint

---

## 12. Backup and Disaster Recovery

Each tenant silo in `docker-compose.prod.yml` includes two dedicated backup sidecar containers:

### MySQL Backup Sidecar

| Setting | Default | Description |
|:---|:---|:---|
| Schedule (`MYSQL_BACKUP_CRON`) | `0 2 * * *` | Nightly at 02:00 UTC |
| Retention (`BACKUP_RETENTION_DAYS`) | `7` | 7-day rolling window |
| Destination | `/backups` volume + optional S3/MinIO push | Local and cloud |

### MongoDB Backup Sidecar

| Setting | Default | Description |
|:---|:---|:---|
| Schedule (`MONGO_BACKUP_CRON`) | `30 2 * * *` | Nightly at 02:30 UTC |
| Retention (`BACKUP_RETENTION_DAYS`) | `7` | 7-day rolling window |
| Destination | `/backups` volume + optional S3/MinIO push | Local and cloud |

### Recovery Procedure

```bash
# 1. Suspend the tenant
POST /api/control/tenants/{id}/suspend

# 2. Restore MySQL
docker exec {slug}-mysql mysql -u store_user -p{pass} store_{slug} < backup.sql

# 3. Restore MongoDB
docker exec {slug}-mongodb mongorestore \
  --uri "mongodb://admin:{pass}@localhost" /backups/latest

# 4. Resume the tenant
POST /api/control/tenants/{id}/resume
```

---

## 13. Operational Runbook

### Provision a New Tenant

```http
POST http://controlplane:9999/api/control/tenants/provision
Content-Type: application/json

{
  "storeName": "Northgate Supermarket",
  "slug": "northgate",
  "adminEmail": "ops@northgate.cm",
  "adminUsername": "admin",
  "adminPassword": "Str0ng!Pass#2026",
  "currency": "XAF",
  "planTier": 1
}
```

**Expected:** `201 Created` with tenant URLs.
**Generated:** `Tenants/northgate/docker-compose.yml` with all secrets injected.

### Manual Deployment (AutoDeployDocker = false)

```bash
cd /opt/controlplane/Tenants/northgate
docker compose up -d
```

### Suspend a Tenant (Preserves Data)

```http
POST http://controlplane:9999/api/control/tenants/{tenantId}/suspend
```

Calls `docker compose stop` — containers halt, all data volumes intact.

### Resume a Suspended Tenant

```http
POST http://controlplane:9999/api/control/tenants/{tenantId}/resume
```

### Deprovision a Tenant (Permanent — CAUTION)

```http
DELETE http://controlplane:9999/api/control/tenants/{tenantId}
```

> **Caution:** This calls `docker compose down -v` which **permanently destroys all data volumes**. MySQL data, MongoDB data, uploaded files, and key rings are irrecoverably deleted. Always take a backup first.

### Check Fleet Health

```http
GET http://controlplane:9999/api/control/tenants/summary
```

### Force Immediate Health Check (One Tenant)

```http
POST http://controlplane:9999/api/control/tenants/{tenantId}/health
```

### View Provisioning Audit Log

```http
GET http://controlplane:9999/api/control/tenants/{tenantId}
```

The `provisioningLogs` array shows every lifecycle event with timestamp, step name, success status, and message.

---

## 14. Scaling Considerations

### Current Architecture Limits

| Concern | Current State | Production Recommendation |
|:---|:---|:---|
| Tenant Registry | JSON flat-file with `SemaphoreSlim` lock | Migrate to SQLite or PostgreSQL for > 50 tenants |
| Health Monitor | Serial loop over all tenants | Add `Parallel.ForEachAsync` with bounded concurrency for large fleets |
| Control Plane HA | Single instance | Add standby replica with shared network storage |
| Image versioning | `store-api:latest` / `store-ui:latest` | Pin to semantic version tags; add rolling-update API endpoints |

### Horizontal Scaling Path

```
Single Host (Current)  -->  Docker Swarm (Multi-Host)  -->  Kubernetes (Namespaces per Tenant)
```

The Docker Silo architecture is intentionally Kubernetes-compatible — each silo maps directly to a Kubernetes Namespace with the same service names, making migration mechanical rather than architectural.

### Per-Tenant Resource Quotas (Future)

The `TenantTier` enum (`Starter`, `Professional`, `Enterprise`) is in the data model and ready to be wired to Docker Compose resource limits:

```yaml
# Future: Tier-based resource constraints
services:
  {slug}-api:
    deploy:
      resources:
        limits:
          cpus: "0.5"    # Starter
          memory: 512M   # Starter
```

---

## 15. Related Files

| File | Purpose |
|:---|:---|
| [TenantOrchestrator.cs](Store.ControlPlane/Services/TenantOrchestrator.cs) | Full tenant lifecycle implementation |
| [TenantsController.cs](Store.ControlPlane/Controllers/TenantsController.cs) | REST API surface (8 endpoints) |
| [TenantHealthMonitorWorker.cs](Store.ControlPlane/Workers/TenantHealthMonitorWorker.cs) | Background health polling worker |
| [docker-compose.tenant.template.yml](Store.ControlPlane/Templates/docker-compose.tenant.template.yml) | Per-tenant compose blueprint |
| [docker-compose.prod.yml](docker-compose.prod.yml) | Production reference stack with backup sidecars |
| [JsonFileTenantRepository.cs](Store.ControlPlane/Repositories/JsonFileTenantRepository.cs) | JSON file tenant persistence layer |
| [Tenant.cs](Store.ControlPlane/Models/Tenant.cs) | Core domain entity |
| [TenantDtos.cs](Store.ControlPlane/Models/DTOs/TenantDtos.cs) | API request/response contracts |
