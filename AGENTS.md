# AGENTS.md — StoreProject

> **Read this first.** This file orients any agent (human or AI) before touching the
> codebase. It captures the project shape, the build/run workflow, the design system,
> the security model, and the conventions you must follow so you don't break things.

---

## 1. What this is

**ClexAn Foods** is a multi-tenant retail-ERP / POS platform, built to run as
isolated per-tenant container stacks. Each tenant has its own MySQL, MongoDB,
API, and UI behind a single Traefik. A `Store.ControlPlane` service orchestrates
tenant lifecycle (provision, suspend, resume, restore, deprovision) and a
`Store.TenantPortal` is the public SaaS signup/onboarding surface.

The codebase has been iterated heavily by Antigravity IDE — see the three
`*_conversation_history.md` files in the repo root for the long-form history of
decisions, refactors, and known half-built features.

## 2. Repository layout

```
StoreProject/
├── Store.API/                # Backend REST API (controllers + middleware)
├── Store.UI/                 # Admin/Cashier/Manager Razor Pages front-end
├── Store.TenantPortal/       # Public SaaS signup / onboarding
├── Store.ControlPlane/       # Tenant provisioning / orchestration
├── Store.DbServices/         # Repositories, services, EF DbContext, migrations
├── Store.Models/             # Entities, DTOs, permission keys
├── Store.API.Tests/          # xUnit test suite
├── docker-compose.platform.yml     # ControlPlane + TenantPortal on the host
├── docker-compose.prod.yml         # Per-tenant production stack
├── docker-compose.traefik.yml      # Edge proxy
├── docker/                   # Backup/restore container Dockerfiles + scripts
├── scripts/                  # provision-tenant / restore-database / api_analyzer
├── docs/                     # Specs, audits, plans
├── .github/workflows/        # CI/CD pipelines
└── IMPLEMENTATION_LOG.md     # Live log of in-progress remediation work
```

## 3. Build & run

Prereqs: .NET SDK 8, Node 20 (only if you want to run `api_analyzer.py`),
MySQL 8 (or Docker), MongoDB 6+ (or Atlas URI), ClamAV sidecar (recommended
in any non-local environment).

```powershell
# Restore + build the whole solution
dotnet restore StoreProject.sln
dotnet build StoreProject.sln --configuration Release

# Run individual services (each has its own launchSettings)
dotnet run --project Store.API
dotnet run --project Store.UI
dotnet run --project Store.ControlPlane
dotnet run --project Store.TenantPortal

# Run tests
dotnet test Store.API.Tests

# Regenerate EF migration (after schema changes)
dotnet ef migrations add <Name> --project Store.DbServices --startup-project Store.API
```

## 4. Secrets — the one thing that can ruin your day

**There are no real secrets in this repo.** Every committed `appsettings.json`
uses `OVERRIDE_ME__...` placeholders. Every Docker compose file uses
`${VAR:?VAR is required}` so compose refuses to start without an explicit env
var. Startup guards in `Store.API/Program.cs` and
`Store.ControlPlane/Services/SecretEncryptionService.cs` throw on:

- empty MySQL password (non-dev only)
- placeholder JWT key (non-dev only)
- placeholder master encryption key
- placeholder MoMo callback key on HMAC verification

If you need a real value while debugging locally, set it in
`appsettings.Development.json` or via `dotnet user-secrets set`. **Do not commit
it.**

Required env vars in production:

| Env var | Used by | Notes |
|---|---|---|
| `ConnectionStrings__Default` | Store.API | `Server=...;User Id=...;Password=...` |
| `ConnectionStrings__ControlPlane` | Store.ControlPlane | same shape |
| `Jwt__Key` | Store.API | ≥32 chars, no placeholder |
| `Payments__MoMoCallbackKey` | Store.API | HMAC secret, ≥32 chars |
| `MongoDB__ConnectionString` | Store.API | per-tenant Atlas URI |
| `ControlPlane__MasterEncryptionKey` | Store.ControlPlane | ≥32 chars, no placeholder |
| `ControlPlane__WebhookSigningSecret` | Store.ControlPlane | ≥32 chars |
| `ControlPlane__RootDomain` | Store.ControlPlane | e.g. `store.example.com` |
| `Antivirus:Provider` | Store.API | `ClamAV` (prod) / `NoOp` (dev only) |
| `Antivirus:ClamAV:BaseUrl` | Store.API | `http://clamav-rest:8080` |

## 5. Security model

The runtime security model is layered:

1. **JWT bearer auth** — `JwtBearer` middleware, security-stamp validated on
   every authenticated request. Refresh-token rotation handled in
   `AuthenticationService.RefreshTokenAsync`. Claims include `uid` and one
   `perm` per granted permission key.
2. **Permission policies** — `Program.cs` registers every constant in
   `PermissionKeys.All` as an authorization policy via
   `AddPolicy(key, p => p.RequireClaim("perm", key))`. **Add new permissions
   to `PermissionKeys.All` — they get a policy automatically.**
3. **Rate limiting** — per-IP fixed-window policies in `Program.cs`:
   - `password-recovery` (3 / 15 min)
   - `financial` (heavy endpoints)
   - `strict-anonymous` (login + recovery)
   - `default` (everything else)
   Add new policies by adding to the `AddRateLimiter` block.
4. **Antivirus** — `ClamAvVirusScanner` is invoked by `FilesController`
   before any persisted image. Without ClamAV configured, the API refuses
   `Antivirus:Provider=NoOp` in production.
5. **HMAC MoMo callbacks** — `PaymentsController` validates
   `X-Callback-Signature: sha256=<hex>` over the raw body using
   `Payments:MoMoCallbackKey` (constant-time compare). The older static
   `X-Callback-Key` scheme is gone.
6. **Security headers** — `SecurityHeadersMiddleware` sets CSP,
   `X-Frame-Options: DENY`, HSTS on HTTPS, etc.
7. **CORS** — explicit allowlist only. `*` is rejected on startup outside dev.
8. **Audit log** — `AuditLoggingMiddleware` writes to BOTH `ILogger` (cheap
   structured info) AND `IAuditLogService` (security-relevant events). Use
   `[Audit]` attribute to mark a non-default action as audit-worthy.

## 6. Multi-tenancy

Every tenant gets its own:

- MySQL 8 container (or shared DB with row-level tenant_id — both supported).
- MongoDB container (or shared Atlas DB filtered by `TenantId`).
- API container.
- UI container.
- Compose file generated from `Store.ControlPlane/Templates/*.yml`.

`Store.TenantPortal` is the public signup. It talks to
`Store.ControlPlane` over a hardened internal-only endpoint that requires
the master encryption key. **Never expose the ControlPlane directly to the
public internet.**

For local multi-tenant testing, see `scripts/provision-tenant.ps1`.

## 7. Design system

See `docs/design_system_specification.md` — the canonical source. Tokens,
components, micro-interactions, and accessibility standards live there. The
UI consumes them through:

- `wwwroot/css/tokens.css` — design tokens (colors, spacing, radii, motion)
- `wwwroot/css/components.css` — primitives
- `wwwroot/css/site.css` — layout
- `wwwroot/css/modules/*.css` — feature styles (sales, inventory, etc.)
- `wwwroot/js/site.js` + `wwwroot/js/toast-bus.js` — interaction utilities

**All new UI work must read the spec first.**

## 8. Coding conventions

- Clean Architecture: `Controller → Application/Handlers → IUnitOfWork →
  Repository<T>`. The `Store.API/Application/` folder is the application
  layer — Commands, Queries, Validators, Handlers, Ports.
- DTOs never expose entity types. Always map at the boundary.
- Don't put business logic in controllers. Controllers dispatch to the
  `IRequestDispatcher` and translate the result.
- `AsNoTracking()` on every read query in `IUnitOfWork`.
- One `IEntityTypeConfiguration<T>` per aggregate root.
- Async everywhere. Cancellation token last.
- Never log secrets or PII. Use `ILogger` with structured properties.

## 9. Testing

- Unit tests live in `Store.API.Tests` (xUnit).
- Add tests for any new permission policy, request handler, or service.
- Use `IClassFixture<T>` for shared DB seeding.
- The CI pipeline runs `dotnet test` and `gitleaks` on every push.

## 10. Before you commit

```powershell
# 1. Build clean
dotnet build StoreProject.sln --configuration Release

# 2. Tests pass
dotnet test Store.API.Tests

# 3. Secrets scan (CI runs this too, but check locally)
# gitleaks detect --source . --no-banner

# 4. Update IMPLEMENTATION_LOG.md with the wave + items you touched
```

## 11. Where to look first

| You need to… | Read first |
|---|---|
| Add an endpoint | `Store.API/Application/<Domain>/Handlers/<Domain>Handlers.cs` + `Controllers/<Domain>Controller.cs` |
| Add a permission | `Store.Models/DTOs/Operations/PermissionKeys.cs` (just append to `All`) |
| Add a rate-limit policy | `Store.API/Program.cs` AddRateLimiter block |
| Change the schema | `Store.DbServices/Migrations/` + add `IEntityTypeConfiguration<T>` |
| Add a tenant-facing UI page | `docs/design_system_specification.md` + `_AppLayout.cshtml` |
| Add a tenant-portal page | `Store.TenantPortal/Pages/` + `_LandingLayout.cshtml` |
| Add a ControlPlane API | `Store.ControlPlane/Controllers/` + `Services/` |
| Investigate a security incident | `docs/security_runbook.md` |
| Update audit log | `[Audit]` attribute on the controller action, or extend `AuditLoggingMiddleware` |

## 12. Known half-built features / follow-ups

These are tracked in `IMPLEMENTATION_LOG.md`. Do not start one without updating
the log so the next agent has continuity. Items still on the runway:

- Soft-delete service-layer conversion (columns + global filter added; delete
  methods in `IItemService`, `ISupplierService`, `IEmployeeService`,
  `ICustomerService` still hard-delete).
- API versioning (`/api/v1/...` route prefix, ETag middleware).
- SignalR hub for live notifications (low stock, approval, contact requests).
- Stripe / PayDunya billing integration for plan-tier enforcement.

---

If you change any of the above, update this file in the same commit. Future
agents (human or AI) will thank you.
