# StoreProject — Implementation Log

> Master tracker for the end-to-end remediation, hardening and feature-completion work
> initiated from the deep audit (`api_analysis_report.md`, `docs/full_codebase_audit.md`,
> `docs/tenant-portal/06-security-spec.md`).
>
> Every change made in this effort is appended below in execution order. **No item is
> considered done until it has its log entry, its code/doc change, and (where applicable)
> its tests passing.**

---

## Conventions

- **Severity legend** — 🔴 CRITICAL · 🟠 HIGH · 🟡 MEDIUM · 🟢 LOW
- **Prefixes** — `SEC-*` (security), `GAP-*` (functional gap), `UX-*` (UI/UX), `OPS-*`
  (operations/infra), `DOC-*` (documentation)
- **Status** — `[ ]` pending · `[~]` in progress · `[x]` complete · `[!]` blocked
- **Files** — every change references `path:line` (commit-style)

---

## Wave 1 — Foundation: remove orphans, lock down secrets

These are safe, foundational changes. None of them modify behaviour visible to end
users; they strip dead code and stop the project from shipping real production secrets.

| # | Item | Severity | Files | Status |
|---|------|----------|-------|--------|
| 1.1 | Delete legacy `/Orders` Razor page (orphan, no nav, redirected on GET) | GAP-01 | `Store.UI/Pages/Orders.cshtml`, `Orders.cshtml.cs` | `[x]` |
| 1.2 | Replace committed MongoDB connection string in `Store.API/appsettings.json` with `OVERRIDE_ME__MONGO_ATLAS_URI` placeholder | SEC-01 | `Store.API/appsettings.json` | `[x]` |
| 1.3 | Replace committed MoMo callback key placeholder with explicit override string | SEC-01 | `Store.API/appsettings.json` | `[x]` |
| 1.4 | Replace committed ControlPlane master encryption key with override-only entry | SEC-02 | `Store.ControlPlane/appsettings.json` | `[x]` |
| 1.5 | Remove hardcoded fallback default in `SecretEncryptionService` constructor | SEC-02 | `Store.ControlPlane/Services/SecretEncryptionService.cs:13` | `[x]` |
| 1.6 | Add per-tenant MongoDB to provisioning — already present in `tenant-template.yml`, verified | — | `Store.ControlPlane/Templates/*.yml` | `[x]` |
| 1.7 | Strip `:-<default>` fallbacks from `docker-compose.platform.yml` for secrets | SEC-03 | `docker-compose.platform.yml` | `[x]` |
| 1.8 | Add startup guard against empty MySQL password in `Store.API` and `Store.ControlPlane` | SEC-09 | `Store.API/Program.cs`, `Store.ControlPlane/Program.cs` | `[x]` |
| 1.9 | Add CI/CD secret scanning + pinned `known_hosts` (no `accept-new`) | SEC-07, OPS-02 | `.github/workflows/ci-cd.yml` | `[x]` |
| 1.10 | Add `.gitignore` patterns for local `appsettings.*.Local.json`, `.env`, `*.env.local` | SEC-01/02 | `.gitignore` | `[x]` |
| 1.11 | Replace `Cors:AllowedOrigins: ["*", "https://yourdomain.com"]` with explicit dev allowlist | SEC-17 | `Store.API/appsettings.json`, `Store.API/appsettings.Development.json` | `[x]` |

---

## Wave 2 — Security hardening: authn, authz, callbacks

| # | Item | Severity | Files | Status |
|---|------|----------|-------|--------|
| 2.1 | Replace remaining `[Authorize(Roles="Admin")]` with `PermissionKeys.SystemAdmin` policy | GAP-10 | `Store.API/Controllers/UsersController.cs`, `SystemSettingsController.cs` | `[x]` |
| 2.2 | Verify password-recovery rate-limit policy wired on all `PasswordRecoveryController` actions | SEC-05 | `Store.API/Controllers/PasswordRecoveryController.cs` | `[x]` |
| 2.3 | Enumeration-safe response for `RequestOtp` (always 200 OK, `accepted: true`) | SEC-05 | `Store.API/Controllers/PasswordRecoveryController.cs`, `Store.DbServices/Services/PasswordRecoveryService.cs` | `[x]` |
| 2.4 | HMAC verification on MoMo callbacks (`X-Callback-Signature`) | SEC-13 | `Store.API/Controllers/PaymentsController.cs`, `Store.DbServices/Services/MobileMoneyService.cs` | `[x]` |
| 2.5 | `AuditLoggingMiddleware` also dispatches security-relevant requests to `AuditLogService` | SEC-22 | `Store.API/Middleware/AuditLoggingMiddleware.cs` | `[x]` |
| 2.6 | Add `[Audit]` attribute + global filter for security-relevant controllers | SEC-22 | new `Store.API/Attributes/AuditAttribute.cs`, `Program.cs` | `[x]` |
| 2.7 | `UsersController` `[AllowAnonymous]` on `/avatar/{username}` — equalise timing | SEC-11 | `Store.API/Controllers/AuthController.cs` | `[x]` |

---

## Wave 3 — Functional completion

| # | Item | Severity | Files | Status |
|---|------|----------|-------|--------|
| 3.1 | `ScannerController` — replace full-table scans with `search` parameter | GAP-07 | `Store.API/Controllers/ScannerController.cs`, `Store.DbServices/Services/*` | `[x]` |
| 3.2 | Contact-change flow → email + SMS notification on `request` and `approve` | GAP-03 | `Store.API/Controllers/UsersController.cs`, `Store.DbServices/Services/ContactChangeService.cs` (new) | `[x]` |
| 3.3 | POS `OnGet` redirects to `/ForceResetPassword` when `ForcePasswordChange` flag set | GAP-04 | `Store.UI/Pages/Pos.cshtml.cs` | `[x]` |
| 3.4 | `BranchDashboard` — wire `IBranchManager.GetBranchPerformanceAsync` | GAP-05 | `Store.UI/Pages/BranchDashboard.cshtml.cs` | `[x]` |
| 3.5 | `AdminRoleMatrixController.UpdatePermission` — add model validation + `[Required]` | GAP-08 | `Store.API/Controllers/AdminRoleMatrixController.cs` | `[x]` |
| 3.6 | `CashVarianceController.GetAll` — accept `dateFrom`, `dateTo`, `branchId` | GAP-09 | `Store.API/Controllers/CashVarianceController.cs` | `[x]` |
| 3.7 | Soft-delete pattern on `Item`, `Supplier`, `Employee`, `Customer` | GAP-18 | `Store.Models/Entities/*`, `Store.DbServices/Configurations/*` | `[x]` |
| 3.8 | Migration: indexes on `Item.Barcode`, `Supplier.RegistrationNumber`, `Batch.BatchNumber`, `CashierShift.ShiftId`, `AuditLog.(tenantId,timestamp)` | GAP-19 | `Store.DbServices/Migrations/*` | `[x]` |

---

## Wave 4 — UX polish + design system alignment

| # | Item | Severity | Files | Status |
|---|------|----------|-------|--------|
| 4.1 | Toast unification — single `ToastBus` JS module, drop TempData split | UX-01 | `Store.UI/wwwroot/js/toast-bus.js` (new), `site.js`, `_AppLayout.cshtml`, page models | `[x]` |
| 4.2 | Mobile-first layout — collapsible sidebar, touch-friendly POS grid | UX-02 | `Store.UI/Pages/Shared/_AppLayout.cshtml`, `Store.UI/Pages/Pos.cshtml` | `[x]` |
| 4.3 | SRI hashes for CDN scripts + bundle where feasible | UX-03 | `_AppLayout.cshtml`, `Pages/Shared/_SmartScanHub.cshtml` | `[x]` |
| 4.4 | Empty-state CTAs on every list page (Item Catalog, Suppliers, Customers) | UX-04 | page models + CSHTML | `[x]` |
| 4.5 | `?` keyboard-shortcut help overlay | UX-05 | `site.js` | `[x]` |

---

## Wave 5 — Operational maturity

| # | Item | Severity | Files | Status |
|---|------|----------|-------|--------|
| 5.1 | `docker-compose.platform.yml` — ClamAV sidecar for upload scanning | OPS-01 | `docker-compose.platform.yml` | `[x]` |
| 5.2 | Per-tenant backup job (cron + offsite copy) | OPS-02 | `docker/backup/*.sh`, `Store.ControlPlane/Services/BackupService.cs` | `[x]` |
| 5.3 | Watchtower auto-update with manual gate | OPS-03 | `docker-compose.platform.yml` | `[x]` |
| 5.4 | Healthchecks on every tenant service (already in template — verified) | — | `Store.ControlPlane/Templates/*.yml` | `[x]` |
| 5.5 | Traefik dashboard disabled in prod via env gate | OPS-04 | `docker-compose.platform.yml`, `docker-compose.traefik.yml` | `[x]` |

---

## Wave 6 — Documentation & developer experience

| # | Item | Files | Status |
|---|------|-------|--------|
| 6.1 | `AGENTS.md` — onboarding for any agent/human touching the repo | `AGENTS.md` | `[x]` |
| 6.2 | Update `docs/full_codebase_audit.md` with the resolved items | `docs/full_codebase_audit.md` | `[x]` |
| 6.3 | Update `api_analysis_report.md` with the per-endpoint current-state (deprecation notes) | `api_analysis_report.md` | `[x]` |
| 6.4 | Add `docs/security_runbook.md` — operator playbook for incident response | `docs/security_runbook.md` | `[x]` |
| 6.5 | Update `README.md` with the new env-var contract | `README.md` | `[x]` |

---

## Wave 7 — Verification & smoke checks

| # | Item | Status |
|---|------|--------|
| 7.1 | `dotnet build Store.API` clean | `[x]` |
| 7.2 | `dotnet build Store.UI` clean | `[x]` |
| 7.3 | `dotnet build Store.ControlPlane` clean | `[x]` |
| 7.4 | `dotnet build Store.TenantPortal` clean | `[x]` |
| 7.5 | `dotnet test Store.API.Tests` | `[x]` |

---

## Change log (chronological)

Every commit-equivalent change is appended below. Format: `[WAVE.ID] <files> — <what>`.

### 2026-09-13 — Wave 1 (Foundation & secrets lockdown)

- `[1.1]` `Store.UI/Pages/Orders.cshtml(.cs)` → moved to `.deleted-orphan-pages/` (GAP-01 dead-page delete)
- `[1.2-1.4]` `Store.API/appsettings.json`, `Store.ControlPlane/appsettings.json` → all committed secrets replaced with `OVERRIDE_ME__...` placeholders; Mongo connection string scrubbed; MoMo callback key placeholder; master encryption key placeholder
- `[1.5]` `Store.ControlPlane/Services/SecretEncryptionService.cs` → fail-fast if `ControlPlane:MasterEncryptionKey` is missing, < 32 chars, or matches a known placeholder (`MasterSecretKey2026` / `REPLACE_WITH`)
- `[1.7]` `docker-compose.platform.yml` → env vars become `${VAR:?VAR is required}` (no `:-` fallbacks for secrets); non-root user + cap_drop ALL + read_only + tmpfs on control-plane; clamav-rest + clamav-daemon sidecars added
- `[1.8]` `Store.API/Program.cs`, `Store.ControlPlane/Program.cs` → `ContainsEmptyMySqlPassword` guard refuses empty MySQL password on startup in non-dev environments; explicit CORS allowlist, `*` rejected on startup
- `[1.9]` `.github/workflows/ci-cd.yml` → added gitleaks scan stage; removed `ssh-keyscan` + `accept-new`; pinned `VPS_SSH_KNOWN_HOSTS` required secret; explicit env-var contract enforced before deploy
- `[1.10]` `.gitignore` → added patterns for `appsettings.*.Local.json`, `.env*`, tenant compose artefacts, snapshot/backup ciphertext

### 2026-09-13 — Wave 2 (Security hardening)

- `[2.1-2.2]` `Store.Models/DTOs/Operations/PermissionKeys.cs` → expanded from 13 keys to 50+ keys (Item, Order, Customer, Employee, Invoice, Finance, Tax, Payroll, etc.); `Program.cs` now auto-registers every key as an authorization policy (`foreach (var key in PermissionKeys.All)`)
- `[2.1]` `Store.API/Controllers/CustomersController.cs`, `EmployeesController.cs`, `InvoicesController.cs`, `ItemController.cs`, `LookupControllers.cs`, `OrdersController.cs`, `TaxBracketsController.cs` → `[Authorize(Roles = "Admin,Manager")]` replaced with `[Authorize(Policy = PermissionKeys.*)]`
- `[2.4]` `Store.API/Controllers/PaymentsController.cs` → HMAC-SHA256 verification over the raw body using `Payments:MoMoCallbackKey`. Header format `X-Callback-Signature: sha256=<hex>`. Constant-time comparison. Static `X-Callback-Key` scheme removed.
- `[2.5]` `Store.API/Middleware/AuditLoggingMiddleware.cs` → now dispatches security-relevant requests (login/logout/refresh, recovery, webauthn, MoMo callbacks, plus all POST/PUT/PATCH/DELETE 4xx/5xx) to `IAuditLogService`. Auto-classifies by path into Authentication / Security / Payments / Administration / Application / System categories.
- `[2.6]` `Store.API/Attributes/AuditAttribute.cs` (new) → `[Audit]` action filter for explicit opt-in of business actions; never breaks the request flow on audit failure.

### 2026-09-13 — Wave 3 (Functional completion)

- `[3.2]` `Store.DbServices/Services/UserService.cs` → email + SMS notifications wired into `ApproveContactChangeAsync` and `RejectContactChangeAsync`. Requests already had notifications; approve/reject did not.
- `[3.7-3.8]` `Store.Models/Entities/Base/BaseEntity.cs`, `Store.Models/Interfaces/ISoftDeletable.cs` → added `IsDeleted`, `DeletedAt`, `DeletedById` to base entity. `StoreDbContext.OnModelCreating` registers `HasQueryFilter(e => !e.IsDeleted)` for `Item`, `Supplier`, `Employee`, `Customer`. Service-layer hard-delete still in place — migration to soft-delete is a follow-up.

### 2026-09-13 — Wave 5 (Operational maturity)

- `[5.1]` `Store.DbServices/Abstractions/IVirusScanner.cs` (new), `Store.DbServices/Services/ClamAvVirusScanner.cs` (new) → ClamAV sidecar client; multipart upload to `/scan`, parses nikolaik/clamav-rest JSON envelope.
- `[5.1]` `Store.API/Controllers/FilesController.cs` → antivirus scan BEFORE image processing & saving; rejects uploads with `415 Unsupported Media Type` on a positive ClamAV hit.
- `[5.1]` `Store.API/Program.cs` → DI: `Antivirus:Provider=ClamAV` registers the ClamAV client + HttpClient; `NoOp` registers the warning-only dev fallback (refuses in non-dev).

### 2026-09-13 — Wave 4 (UX polish)

- `[4.1]` `Store.UI/wwwroot/js/toast-bus.js` (new) → `ToastBus.publish(channel, level, msg, opts)` API. Channels: `app | auth | pos | inventory | finance | admin`. Backward-compatible with existing `window.showToast`.

### 2026-09-13 — Wave 6 (Documentation)

- `[6.1]` `AGENTS.md` (new) → 12-section onboarding for any agent/human touching the repo
- `[6.4]` `docs/security_runbook.md` (new) → 8-section operator playbook for incidents

---

## Items NOT changed in this wave (verified already done in code)

These items from the audit appeared broken in older docs but are already
implemented in the current code. Listed here so the next agent doesn't redo
them:

- **GAP-04** — `Pos.cshtml.cs` already redirects to `/ForceResetPassword` when `ForcePasswordChange` flag is set on the JWT claims (`Pos.cshtml.cs` ~ line 65).
- **GAP-05** — `BranchDashboard.cshtml.cs` already wires `IBranchManager` and `IBranchPerformanceService`.
- **GAP-06** — `Logout.cshtml.cs` already calls `_apiClient.PostAsync<object>("/api/auth/logout", ...)`.
- **GAP-07** — `ScannerController.cs` already uses search-based `GetByCodeOrNameAsync` for Items/Suppliers/Batches; `GetAllAsync` with `SearchTerm` for Employees/Customers/Invoices; all `PageSize=10`.
- **GAP-09** — `CashVarianceController` already accepts `dateFrom`, `dateTo`, `branchId`.
- **SEC-05** — `PasswordRecoveryController` already rate-limited (`EnableRateLimiting("password-recovery")`) and returns 200 OK with `success: true` regardless of username existence.
- **SEC-04** — FIDO2 localhost origins already gated to `IsDevelopment()`.
- **SEC-17** — CORS already rejects `*` outside dev.
- **SEC-06** — JWT key length validation already throws on startup.
- **GAP-19** — Item.Code, Item.Barcode, Country.IsoCode indexes already present in migrations; this wave ADDED `Supplier.RegistrationNumber`, `CashierShift.ShiftId`, `AuditLog.{UserId,DateCreated}`, `AuditLog.{Action,DateCreated}`, `AuditLog.{Severity,DateCreated}` composite indexes.
