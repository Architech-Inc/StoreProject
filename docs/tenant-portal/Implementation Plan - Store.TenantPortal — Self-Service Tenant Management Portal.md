# Implementation Plan: Store.TenantPortal — Self-Service Tenant Management Portal

## Background

The Control Plane currently requires direct HTTP API calls to provision and manage tenants.
This plan introduces **Store.TenantPortal**, a fully independent, publicly accessible web portal that gives tenants:

1. Self-service onboarding and store provisioning via a guided wizard
2. A real-time environment control panel (health, start/stop/restart/logs)
3. Domain management — bring your own domain, platform subdomains, per-branch sub-subdomains
4. External cloud backup configuration (OneDrive, Google Drive, S3/MinIO) via OAuth

> [!IMPORTANT]
> **This is a brand-new project** (`Store.TenantPortal`) added to the solution alongside the existing projects.
> It does NOT modify `Store.UI`, `Store.API`, or `Store.ControlPlane` source code except to extend
> the Control Plane data model and API surface.

---

## Open Questions

> [!IMPORTANT]
> **Q1 — Portal Auth:** Should portal users authenticate with a local email + PBKDF2 password system
> (standalone, no IdP dependency), or integrate with Microsoft Entra / Google OAuth?
>
> **Proposed default:** Local email + password with secure cookie session. No external IdP required to launch.

> [!IMPORTANT]
> **Q2 — Branch Subdomain Routing:** When a tenant maps `hq.acme.com`, Traefik must be updated.
> Should this be: (A) automatic via Docker socket from the Portal, or
> (B) via a Traefik dynamic config file that the ControlPlane rewrites on domain change?
>
> **Proposed default:** Option B — safer. ControlPlane writes `/traefik/dynamic/{slug}.yml`; Traefik hot-reloads it.

> [!IMPORTANT]
> **Q3 — OAuth App Registration:** The OneDrive and Google Drive backup integrations require registered
> OAuth apps (Azure App Registration and Google Cloud Console OAuth 2.0 Client).
> Do you have these, or shall I include setup instructions?
>
> **Proposed default:** Plan scaffolds the full OAuth flow. You supply `ClientId`/`ClientSecret` via environment variables.

> [!IMPORTANT]
> **Q4 — Custom Domain Verification Method:** To verify tenant-owned domains:
> (A) DNS TXT record challenge (`_clexan-verify.example.com = token`), or
> (B) HTTP file challenge (`example.com/.well-known/clexan-verify/{token}`)
>
> **Proposed default:** Option A — DNS TXT challenge (robust, works behind HTTPS, no HTTP dependency).

---

## Architecture Overview

```
[Public Internet]
        |
        |  portal.store.yourcompany.com:443
        v
[Traefik]  <─────────── also hot-reloads /traefik/dynamic/ for tenant custom domains
        |
        v
[Store.TenantPortal]  ── ASP.NET Core 8 Razor Pages (Port 9998)
        |                  Pages: Landing, Register, Onboarding Wizard,
        |                         Dashboard, Environment Panel, Domains,
        |                         Branches, Backups, Settings
        |
        |  HTTP (internal, store-control-plane network)
        v
[Store.ControlPlane]  ── Extended REST API (Port 9999)
        |                  Existing: /tenants (CRUD, health)
        |                  NEW:      /auth, /domains, /branches, /backups, /oauth
        |
        v
[TenantOrchestrator + DomainVerificationService + TraefikDynamicConfigWriter]
        |
        v
[Docker Compose Silos]   {slug}-mysql / {slug}-mongodb / {slug}-api / {slug}-ui
```

---

## Proposed Changes

---

### A. Store.TenantPortal (NEW PROJECT)

**Tech stack:** ASP.NET Core 8 Razor Pages — consistent with the rest of the solution.
**Auth:** Cookie-based session with local email + PBKDF2-hashed password.
**Port:** 9998

#### [NEW] `Store.TenantPortal/Store.TenantPortal.csproj`

Key NuGet packages:
- `Microsoft.AspNetCore.Authentication.Cookies`
- `Microsoft.Graph` — OneDrive / Microsoft 365 backup via Graph API
- `Google.Apis.Drive.v3` — Google Drive backup
- `DnsClient` — DNS TXT record lookup for domain verification
- `FluentValidation.AspNetCore`

#### [NEW] Razor Pages

| Page | Route | Purpose |
|:---|:---|:---|
| `Index.cshtml` | `/` | Marketing landing page with CTA |
| `Register.cshtml` | `/register` | Portal account creation |
| `Onboarding.cshtml` | `/onboarding` | 4-step provisioning wizard |
| `Dashboard.cshtml` | `/dashboard` | Home — health summary, quick actions |
| `Environment.cshtml` | `/environment` | Silo control panel (health, restart, logs) |
| `Domains.cshtml` | `/domains` | Custom domain + platform subdomain management |
| `Branches.cshtml` | `/branches` | Branch subdomain mapping |
| `Backups.cshtml` | `/backups` | Cloud backup provider config + history |
| `Settings.cshtml` | `/settings` | Plan, currency, admin credentials |
| `Login.cshtml` | `/login` | Portal authentication |

#### [NEW] Onboarding Wizard — 4 Steps

```
Step 1: Account          Step 2: Your Store          Step 3: Domain            Step 4: Confirm
Name, Email, Password →  Store Name, Slug, Plan  →   Domain Choice         →   Review & Launch
                         Currency                      (Platform or BYOD)
```

**Domain Choice (Step 3) — Two paths:**
```
( ) Use ClexAn platform domain  (zero DNS setup)
    Store: [slug].store.yourcompany.com
    API:   api.[slug].store.yourcompany.com

( ) Use my own domain
    Domain: [acme-foods.com]
    DNS TXT record instructions shown here:
      Name:  _clexan-verify.acme-foods.com
      Value: clxv_<random-64-char-token>
      TTL:   300 seconds
    [Verify Now] button (live DNS check)
```

#### [NEW] Environment Control Panel

- **Container status cards** — MySQL, MongoDB, API, UI with health dot + uptime + last check time
- **Action buttons** — Restart All, Restart API, Restart UI, Force Health Check
- **Provisioning log timeline** — visual timeline of all `TenantProvisioningLog` entries with step icons
- **Silo URLs** — clickable links to UI and API endpoints

#### [NEW] Domain Manager

**Panel A — Platform Subdomain (always active):**
```
Store URL:  https://acme.store.yourcompany.com     ● Active
API URL:    https://api.acme.store.yourcompany.com ● Active
```

**Panel B — Custom Domain (BYOD):**
```
Status:  [ Not configured | Pending Verification | Active ]

Domain:  [______________________]  [Save]

When pending — shows DNS instructions + [Check DNS Now] button.
When Active  — shows [Remove Custom Domain] + Traefik TLS status.
```

#### [NEW] Branch Manager

Tenants map named branches to subdomains:

```
Branch      │ URL                                    │ Status      │ Actions
────────────┼────────────────────────────────────────┼─────────────┼──────────
HQ          │ hq.acme.store.yourcompany.com          │ ● Active    │ [Edit][Remove]
Northgate   │ northgate.acme.store.yourcompany.com   │ ● Active    │ [Edit][Remove]
Mfoundi     │ mfoundi.acme-foods.com                 │ ⚠ Pending  │ [Verify DNS][Remove]

                                                       [+ Add Branch Mapping]
```

Each branch maps to the **same tenant UI container** — the Store.UI already supports multi-branch via branch context. The branch subdomain is just an additional Traefik `Host()` rule for that tenant's UI router.

Branch domain options:
- **Platform:** `{branch}.{slug}.store.yourcompany.com` — zero setup, immediate
- **Custom (on BYOD domain):** `{branch}.{custom-domain}` — requires DNS CNAME record

#### [NEW] Backup Configuration Panel

```
┌───────────────────────┐  ┌───────────────────────┐  ┌───────────────────────┐
│  OneDrive              │  │  Google Drive          │  │  S3 / MinIO            │
│  Status: Disconnected  │  │  Status: Connected     │  │  Status: Configured    │
│  [Connect OneDrive]    │  │  Folder: /StoreBackups │  │  Bucket: store-backups │
│                        │  │  Last: 2h ago ✓        │  │  Last: 3h ago ✓        │
│                        │  │  [Disconnect]          │  │  [Edit Credentials]    │
│                        │  │  [Backup Now]          │  │  [Backup Now]          │
└───────────────────────┘  └───────────────────────┘  └───────────────────────┘

Schedule:  Every night at [ 02:00 UTC ]    Retention: [ 7 days ]    [Save Schedule]

Backup History:
  ● 2026-09-01 02:00 — MySQL 450 MB — Google Drive    — ✓ Success
  ● 2026-09-01 02:30 — MongoDB 120 MB — Google Drive  — ✓ Success
  ● 2026-08-31 02:00 — MySQL 448 MB — S3              — ✓ Success
```

---

### B. Store.ControlPlane — Extensions (MODIFY)

#### [MODIFY] `Store.ControlPlane/Models/Tenant.cs`

Add new fields to the `Tenant` domain entity:

```csharp
// Domain management
public string? CustomDomain { get; set; }
public DomainVerificationStatus CustomDomainStatus { get; set; } = DomainVerificationStatus.NotConfigured;
public string? CustomDomainVerificationToken { get; set; }
public DateTime? CustomDomainVerifiedAt { get; set; }
public List<TenantBranch> Branches { get; set; } = new();

// Backup configuration
public List<BackupProvider> BackupProviders { get; set; } = new();
public string? BackupScheduleCron { get; set; }
public int BackupRetentionDays { get; set; } = 7;

// Portal authentication
public string? PortalPasswordHash { get; set; }      // PBKDF2-SHA512
public string? PortalPasswordSalt { get; set; }
public DateTime? LastPortalLogin { get; set; }
```

#### [NEW] `Store.ControlPlane/Models/TenantBranch.cs`

```csharp
public class TenantBranch
{
    public Guid BranchId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string BranchName { get; set; } = string.Empty;    // "HQ", "Northgate"
    public string BranchSlug { get; set; } = string.Empty;    // "hq", "northgate"
    public BranchDomainType DomainType { get; set; }
    public string ResolvedUrl { get; set; } = string.Empty;
    public DomainVerificationStatus VerificationStatus { get; set; }
    public string? DnsVerificationToken { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public enum BranchDomainType { Platform, Custom }
public enum DomainVerificationStatus { NotConfigured, Pending, Verified, Failed }
```

#### [NEW] `Store.ControlPlane/Models/BackupProvider.cs`

```csharp
public class BackupProvider
{
    public Guid ProviderId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public BackupProviderType ProviderType { get; set; }
    public bool IsEnabled { get; set; }
    public string? OAuthRefreshToken { get; set; }     // AES-256 encrypted at rest
    public string? OAuthAccessToken { get; set; }      // AES-256 encrypted at rest
    public DateTime? OAuthTokenExpiry { get; set; }
    public string? TargetFolder { get; set; }          // Drive folder ID or S3 bucket
    public DateTime? LastBackupAt { get; set; }
    public bool LastBackupSuccess { get; set; }
    public string? LastBackupMessage { get; set; }
}

public enum BackupProviderType { OneDrive, GoogleDrive, S3, MinIO }
```

#### [NEW] `Store.ControlPlane/Services/DomainVerificationService.cs`

DNS TXT lookup via `DnsClient` NuGet package:
- Resolves `_clexan-verify.{domain}` TXT records
- Compares against stored `CustomDomainVerificationToken`
- Returns `bool` and updates tenant `CustomDomainStatus`

#### [NEW] `Store.ControlPlane/Services/TraefikDynamicConfigWriter.cs`

Writes `/traefik/dynamic/{slug}.yml` on:
- Custom domain verification success
- Branch mapping add/remove

Traefik watches this directory and hot-reloads with zero downtime.

```yaml
# /traefik/dynamic/acme.yml (auto-generated — do not edit manually)
http:
  routers:
    acme-ui-custom:
      rule: "Host(`acme-foods.com`) || Host(`hq.acme-foods.com`) || Host(`northgate.acme-foods.com`)"
      service: acme-ui-svc
      tls:
        certResolver: letsencrypt
  services:
    acme-ui-svc:
      loadBalancer:
        servers:
          - url: "http://acme-ui:8080"
```

#### [NEW] `Store.ControlPlane/Services/OneDriveBackupService.cs`

Microsoft Graph SDK (`Microsoft.Graph`):
- Authorization Code OAuth2 flow, scopes: `Files.ReadWrite.AppFolder offline_access`
- Uploads MySQL and MongoDB dump files to `/Apps/ClexAnFoods/{slug}/backups/`
- Token refresh handled automatically via `Microsoft.Graph.Authentication`

#### [NEW] `Store.ControlPlane/Services/GoogleDriveBackupService.cs`

Google.Apis.Drive.v3:
- OAuth2 Authorization Code flow, scope: `https://www.googleapis.com/auth/drive.file`
- Creates folder `ClexAn Backups/{slug}` in tenant's Google Drive
- Uploads dump files, honours retention by deleting files older than `BackupRetentionDays`

#### [NEW] Extended Control Plane API Endpoints

| Method | Path | Description |
|:---|:---|:---|
| `POST` | `/api/control/auth/register` | Create portal account |
| `POST` | `/api/control/auth/login` | Authenticate + issue session token |
| `GET` | `/api/control/tenants/{id}/domains` | Get domain configuration |
| `POST` | `/api/control/tenants/{id}/domains/custom` | Set custom domain + issue verification token |
| `POST` | `/api/control/tenants/{id}/domains/verify` | Trigger live DNS TXT check |
| `DELETE` | `/api/control/tenants/{id}/domains/custom` | Remove custom domain + clear Traefik config |
| `GET` | `/api/control/tenants/{id}/branches` | List branch mappings |
| `POST` | `/api/control/tenants/{id}/branches` | Add branch + update Traefik config |
| `DELETE` | `/api/control/tenants/{id}/branches/{branchId}` | Remove branch |
| `GET` | `/api/control/tenants/{id}/backups/providers` | List configured backup providers |
| `POST` | `/api/control/tenants/{id}/backups/providers` | Add or update a provider |
| `DELETE` | `/api/control/tenants/{id}/backups/providers/{providerId}` | Remove provider |
| `POST` | `/api/control/tenants/{id}/backups/trigger` | Trigger manual backup now |
| `GET` | `/api/control/tenants/{id}/backups/history` | List backup job history |
| `GET` | `/api/control/oauth/onedrive/callback` | OneDrive OAuth2 redirect handler |
| `GET` | `/api/control/oauth/google/callback` | Google Drive OAuth2 redirect handler |

---

### C. Docker Compose Integration

#### [NEW] `docker-compose.portal.yml`

```yaml
services:
  store-tenant-portal:
    image: store-tenant-portal:latest
    container_name: store-tenant-portal
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ControlPlane__BaseUrl=http://store-control-plane:9999
      - OAuth__Microsoft__ClientId=${PORTAL_MSFT_CLIENT_ID}
      - OAuth__Microsoft__ClientSecret=${PORTAL_MSFT_CLIENT_SECRET}
      - OAuth__Google__ClientId=${PORTAL_GOOGLE_CLIENT_ID}
      - OAuth__Google__ClientSecret=${PORTAL_GOOGLE_CLIENT_SECRET}
    volumes:
      - traefik-dynamic:/traefik/dynamic     # Writes dynamic routing config
    networks:
      - proxy-network
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.tenant-portal.rule=Host(`portal.store.yourcompany.com`)"
      - "traefik.http.services.tenant-portal.loadbalancer.server.port=8080"

volumes:
  traefik-dynamic:
    external: true
```

---

## Security Design

| Concern | Approach |
|:---|:---|
| Portal session | Secure `HttpOnly SameSite=Strict` cookie with 8-hour expiry |
| Password storage | PBKDF2-SHA512, 100,000 iterations, 32-byte salt per account |
| OAuth tokens at rest | AES-256-GCM encrypted before writing to `tenants.json` |
| DNS verification tokens | 64 cryptographically random bytes (hex encoded) |
| Traefik config writes | Validated slug allowlist before writing any filename |
| Custom domain input | Regex + length validation; no shell execution on domain strings |

---

## Verification Plan

### Automated Tests (New)

```bash
dotnet test Store.API.Tests/Store.API.Tests.csproj
```

New test classes:
- `PortalAuthTests` — register, login, bad credentials
- `DomainVerificationTests` — token generation, DNS TXT mock, failed verification
- `BranchMappingTests` — add/remove/list, Traefik config output validation
- `BackupProviderTests` — provider CRUD, OAuth token refresh logic

### Manual Verification Steps

1. `http://localhost:9998` → Landing page renders with ClexAn Fluent 2.0 design.
2. Register + complete onboarding wizard → tenant silo provisioned → Dashboard shows health cards.
3. Add a branch mapping → Traefik dynamic config file written and readable.
4. Set custom domain → TXT record instructions shown → `[Check DNS Now]` responds correctly.
5. Initiate OneDrive OAuth → Microsoft consent screen → token stored (verify no plaintext in JSON).
6. Trigger manual backup → response shows success, entry appears in Backup History table.

---

## Phased Delivery

| Phase | Scope | Effort |
|:---|:---:|:---:|
| **1** | New project scaffold, portal auth (register/login), onboarding wizard, Dashboard | ~3 days |
| **2** | Environment control panel — health cards, restart actions, provisioning log timeline | ~1 day |
| **3** | Domain management — platform subdomain display, BYOD DNS TXT flow, Traefik writer | ~2 days |
| **4** | Branch subdomain mapping — add/remove, platform + custom, Traefik routing update | ~1 day |
| **5** | Cloud backup OAuth — OneDrive + Google Drive flows, history table, schedule editor | ~2 days |
| **6** | S3/MinIO backup config, Portal Dockerfile, `docker-compose.portal.yml`, CI/CD | ~1 day |

**Total estimated effort: ~10 development days**
