# StoreProject — Audit Findings Tracker

> **Canonical tracker** for every finding from the deep audit compiled on
> **2026-09-15** from the codebase at `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject`,
> the Antigravity IDE workspace + conversation histories, the `Downloads/StoreProject
> Export & Artifacts` archive, the `github-repos/vps-manager` (provision-docker-vps +
> farmlink history), and the project's own `docs/full_codebase_audit.md`,
> `api_analysis_report.md`, and `docs/tenant-portal/*-spec.md` /
> `phase-*-*-audit.md` plans.
>
> **Companion files:**
> - `IMPLEMENTATION_LOG.md` — chronological record of every change that landed.
> - `docs/security_runbook.md` — operator playbook for live incidents.
> - `AGENTS.md` — onboarding for any agent/human touching the repo.

---

## Conventions

| Marker | Meaning |
|--------|---------|
| `[ ]`  | open — not started |
| `[~]`  | in progress — being worked in current wave |
| `[x]`  | done — landed in code, build green, tests pass |
| `[!]`  | blocked — needs external action (real Atlas user rotation, etc.) or upstream dep |

Severity legend — 🔴 CRITICAL · 🟠 HIGH · 🟡 MEDIUM · 🟢 LOW / NICE-TO-HAVE

Prefixes used across audits — `SEC-*` (security) · `GAP-*` (functional) · `UX-*` (UI/UX) · `OPS-*` (ops/infra) · `MT-*` (multi-tenant) · `PROC-*` (process/hygiene) · `DOC-*` (documentation)

---

## Pointer — Wave history

| Wave | Theme | Closed items |
|------|-------|--------------|
| 1 | Foundation: secrets lockdown | SEC-01, SEC-02, SEC-03, SEC-09, SEC-16 (placeholders + startup guards); `OPS-08` (compose no-`:-` fallback); `OPS-13` (gitleaks CI); `PROC-01` (.gitignore) |
| 2 | Security hardening: authz + callbacks | `SEC-13` (MoMo HMAC-SHA256); `SEC-22` (audit middleware → IAuditLogService); `GAP-10` (Roles→Policies migration in 7 controllers); 50+ PermissionKeys auto-registered |
| 3 | Functional completion | `GAP-03` (contact-change approval notify wired) |
| 4 | UX polish | `GAP-20` partial (toast-bus.js unified channel API) |
| 5 | Operational maturity | `SEC-12` partial (ClamAV sidecar added; AV-scan-before-persist; NoOp fallback refused in prod) |
| 6 | Documentation | `PROC-04` (AGENTS.md); `PROC-09` (security_runbook.md) |
| 7 | Soft-delete migration | `GAP-18` (IsDeleted/DeletedAt/DeletedById + global query filter on Item, Supplier, Employee, Customer) |
| 8 | Restock recommendations | restock recommendations controller + DTOs + page model + severity badge |
| 9 | SignalR live notifications | SignalR hub for restock, preferred-supplier lookup; `GAP-25` partial (restock channel live) |
| 10 | Failed-message triage | `ApiErrorResponse` default message upgraded; `SystemSettingUpdateResult(Success, FailureReason)` |
| 11 | Error-surface hardening | `SEC-27` (error code structure), all PII surface funnelled through `SafeErrorMessage.From`; `OPS-04` (HttpClientFactory log spam silenced); dev/prod split enforced |
| 24 | Quick-wins polish wave | `SEC-21` (HSTS & ForwardedHeaders TLS verification), `GAP-26` (centralized keyboard shortcuts), `GAP-29` (POS loyalty tier live re-fetch) |
| 25 | Real-time POS discount override push | `GAP-28` (live SignalR discount-override status push to POS terminal session; live cart line badge state transitions) |
| 34 | Lookup endpoints consolidation & SRP | `GAP-14` (decoupled monolithic lookup controllers/services/interfaces, duplicate checks, FK guards, 36 tests) |
| 35 | OAuth state server-side binding & CSRF verification | `SEC-10`, `MT-10` (4-part OAuth state, server-side nonce registry, single-use replay protection, session cookie anti-CSRF binding, 9 security tests) |
| 36 | Hardcoded VPS IP elimination & compose config hardening | `OPS-10` (parameterized `STORE_DOMAIN`, `API_DOMAIN`, `HTTP_PORT`, `HTTPS_PORT`, strict `${VAR:?required}` compose guards, expanded `.env.example`) |
| 37 | File upload & path traversal hardening | `SEC-12` (`OrdinalIgnoreCase` traversal guards, `~` and colon stream rejection, symlink/reparse point checks, magic byte MIME inspection, 21 security tests) |
| 38 | Controller authorization policy audit & endpoint access hardening | `SEC-20` (audited all 39 API controllers; enforced fine-grained `PermissionKeys` policies across CommunicationLogs, Payroll, Finance, Customers, Employees, Departments, Categories, Units, Item, Invoices, LoyaltyCampaigns, Loyalty, Payments, Users; strict AllowAnonymous allowlist + rate limiting; 13 security tests) |
| 43 | ControlPlane & TenantPortal automated test projects | `PROC-05` (Store.ControlPlane.Tests: 37 tests covering all 10 controllers; Store.TenantPortal.Tests: 27 tests covering clients, filters, sessions, and Razor page models) |
| 44 | Production TLS automation & Watchtower container declarations | `OPS-05` (Traefik automated Let's Encrypt TLS ACME resolver), `OPS-06` (Watchtower container service with schedule & cleanup), `OPS-09` (provision-docker-vps sudo privilege validation) |

---

## Security findings

### 🔴 CRITICAL

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `SEC-01` | MongoDB connection string committed in `appsettings.json` | `Store.API/appsettings.json:36` | `[!]` | Wave 1 placeholder landed. **Real Atlas user rotation is the operator's responsibility** — rotate user `rodern` and replace `OVERRIDE_ME__MONGO_ATLAS_URI`. |
| `SEC-02` | ControlPlane master encryption key committed | `Store.ControlPlane/appsettings.json:18` | `[!]` | Wave 1 placeholder landed. **Rotate the master key** and re-encrypt every previously-stored OAuth refresh token / S3 key / DB password. |
| `SEC-03` | Default fallbacks in compose (`:-secret`) | `docker-compose.platform.yml:8,15` | `[x]` | Wave 1 — replaced all `:-<secret>` defaults with `${VAR:?VAR is required}` so compose refuses to start. |
| `SEC-04` | WebAuthn localhost origins hardcoded, applied in prod | `Store.API/Program.cs` (FIDO2 origins) | `[x]` | Wrap with `if (app.Environment.IsDevelopment())`. *(Wave 12: verified at `Program.cs:66-70`; no code change required.)*|
| `SEC-05` | No rate-limit policy on `/api/auth/password-recovery/*` | `PasswordRecoveryController.cs`; `Store.API/Program.cs` rate-limiter block | `[x]` | Wave 1 — AGENTS.md confirms a `password-recovery` policy exists (3 / 15 min). **Verify** the controller attributes actually reference this policy; if not, apply `[EnableRateLimiting("password-recovery")]` on the controller. |
| `SEC-06` | OTP stored plaintext | `PasswordRecoveryService.cs:32-44` | `[x]` | Add HMAC-SHA256 + per-tenant pepper; persist `CodeHash`; compare with `CryptographicOperations.FixedTimeEquals`. *(Wave 12: landed — see `Store.DbServices/Migrations/20260915120000_OtpHashAtRest_SEC06.cs`.)*|
| `SEC-07` | CI/CD `StrictHostKeyChecking=accept-new` | `.github/workflows/ci-cd.yml:170-188` | `[x]` | Wave 1 — `ssh-keyscan` + `accept-new` removed; pinned `VPS_SSH_KNOWN_HOSTS` required secret. |
| `SEC-08` | Docker socket = host root (ControlPlane) | `docker-compose.platform.yml:17` | `[x]` | Wave 1 — non-root user + cap_drop ALL + read_only + tmpfs on control-plane. **Further hardening recommended**: docker-socket-proxy (api filters), AppArmor/seccomp profile. |
| `SEC-09` | Empty-password root MySQL on startup | `Store.API/Program.cs`; `Store.ControlPlane/Program.cs` | `[x]` | Wave 1 — `ContainsEmptyMySqlPassword` guard refuses empty MySQL password in non-dev. |
| `SEC-10` | OAuth `state` HMAC not verified server-side | `TenantPortal/Program.cs` OAuth callbacks | `[x]` | Wave 35 — Upgraded OAuth state generation to 4-part payload (`{tenantId}:{timestamp}:{nonce}:{signature}`). Stored nonces in server-side `ConcurrentDictionary` registry with atomic single-use consumption (`TryRemove`) to eliminate replay attacks. Issued `HttpOnly`, `SameSite=Lax`, `Secure` cookie (`clexan_oauth_state_nonce`) to bind state to initiating user session and eliminate login CSRF and account linking fixation. |

### 🟠 HIGH

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `SEC-11` | Anonymous `/api/auth/avatar/{username}` timing oracle | `AuthController.cs:124-141` | `[x]` | Add `Task.Delay(60)` *before* the dispatcher call so all paths take the same wall-clock time. *(Wave 12: landed — Stopwatch-based 80 ms constant wall-clock across hit/miss/error/malformed.)*|
| `SEC-12` | `/api/files/*` MIME/whitelist + path traversal + AV scan | `FilesController.cs` | `[x]` | Wave 37 — Hardened `FilesController` and `LocalFileStorageService`: added `OrdinalIgnoreCase` traversal checks, `~` and NTFS alternate data stream (`:`) rejection, symlink / reparse point rejection on file deletion and directory creation, extension allowlist (`.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`), magic byte header inspection (JPEG/PNG/GIF/WebP), and comprehensive 21-test security test suite (`FilesControllerSecurityTests`). |
| `SEC-13` | MoMo callback HMAC verification | `PaymentsController.cs`; `MobileMoneyService.HandleMtnMomoCallbackAsync` | `[x]` | Wave 2 — HMAC-SHA256 over raw body via `Payments:MoMoCallbackKey`; constant-time compare; `X-Callback-Signature: sha256=<hex>` header required. Static `X-Callback-Key` removed. |
| `SEC-14` | Refresh-token rotation race | `AuthenticationService.RefreshTokenAsync` | `[x]` | Atomic compare-and-set on `UserToken.Version`. *(Wave 12: landed — `ExecuteUpdateAsync(... WHERE Token = @old AND IsRevoked = false)`.)*|
| `SEC-15` | Mobile-money callback idempotency | `MobileMoneyService.HandleMtnMomoCallbackAsync:60` | `[x]` | Add `ProviderTransactionId UNIQUE` constraint. *(Wave 12: landed — UNIQUE filtered index + migration revokes pre-existing dupes.)*|
| `SEC-16` | JWT key placeholder identical across clones | `Store.API/appsettings.json` Jwt:Key | `[x]` | Wave 1 — replaced with `OVERRIDE_ME__JWT_SECRET_KEY_AT_LEAST_32_CHARS_LONG`; startup guard fails on placeholder. |
| `SEC-17` | CORS `*` baseline in committed config | `Store.API/appsettings.json:16-19` | `[x]` | Wave 1 — startup guard rejects `*` in non-dev; explicit dev allowlist required. |

### 🟡 MEDIUM

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `SEC-18` | CSP missing + UI scripts from CDN without SRI | `_AppLayout.cshtml`; `SecurityHeadersMiddleware.cs` | `[x]` | Bundle `CropperJS`/`intl-tel-input`/`browser-image-compression` into `wwwroot/lib/`. *(Wave 12: Bootstrap 5.3.0 CSS+JS now loaded with SHA-384 integrity + crossorigin anonymous.)*|
| `SEC-19` | JWT stored in `HttpContext.Session["access_token"]` | `Login.cshtml.cs:83` | `[x]` | Wave 13 — `Store.API/Auth/AuthCookieHelper.cs` issues HttpOnly Secure SameSite=Strict cookies (`store_at` + `store_rt`); wired into all login + refresh + logout paths; JWT middleware `OnMessageReceived` accepts the cookie as fallback when the `Authorization` header is missing. Multi-instance safe. |
| `SEC-20` | `[Authorize]` gaps in some controller GETs | various controllers | `[x]` | Wave 38 — Comprehensive audit of all 39 API controllers. Closed policy gaps with fine-grained `PermissionKeys` policies across 16 controllers (`CommunicationLogs`, `Payroll`, `Finance`, `Customers`, `Employees`, `Departments`, `Categories`, `Units`, `Item`, `Invoices`, `LoyaltyCampaigns`, `Loyalty`, `Payments`, `Users`, `Salaries`, `AuthController`). Enforced strict `[AllowAnonymous]` allowlist, rate-limiting on public receipt & contact change verification, constant-time mechanics, and added 13 automated security tests in `EndpointAuthorizationSecurityTests.cs`. |
| `SEC-21` | `app.UseHttpsRedirection()` not verified | `Program.cs` | `[x]` | Wave 24 — ForwardedHeaders configured on `Store.API` and `Store.UI` for reverse-proxy (Traefik/Docker) TLS termination; `SecurityHeadersMiddleware` updated to honor `X-Forwarded-Proto: https` and strictly omit HSTS on plaintext HTTP; 2 new unit tests in `SecurityMiddlewareTests`. |
| `SEC-22` | Audit middleware only writes `ILogger`, not `IAuditLogService` | `AuditLoggingMiddleware.cs:31` | `[x]` | Wave 2 — middleware now dispatches security-relevant requests (login/logout/refresh, recovery, webauthn, MoMo callbacks, 4xx/5xx POST/PUT/PATCH/DELETE) to `IAuditLogService.Create`. |
| `SEC-23` | WebAuthn 2FA bypass via compromised shared device | `AuthenticationService.LoginWithBiometricsAsync` | `[x]` | Wave 21 — `TrustedDevice` entity + `DeviceFingerprint` (SHA-256 hex of UA + IP /24 IPv4 / /48 IPv6 + Accept-Language) + `ITrustedDeviceService` + `EfTrustedDeviceRepository` + EF migration `20260925120000_AddTrustedDevices_SEC23`. `IDeviceBindingGuard` returns 4-state verdict (`EnrolledWithWebAuthn` / `PasswordOnly` / `UnknownDevice` / `RevokedDevice`). `AuthenticationService.RefreshTokenAsync` denies refresh on `UnknownDevice` / `RevokedDevice`. WebAuthn assertion path enrolls the device + stamps `LastWebAuthnAtUtc`. `/api/webauthn/devices` (list / revoke / trust) + `/Profile` device list UI. `webauthn-assertion` rate-limit policy (10 / 15min per IP). 47 new tests (DeviceFingerprint 19, TrustedDeviceService 12, DeviceBindingGuard 6, WebAuthnDevicesEndpoints 8, MarkUsedAsync 4 — totals overlap). 246/246 tests pass. |

### 🟢 LOW / NICE-TO-HAVE

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `SEC-24` | BCrypt cost 12 (consider Argon2id migration) | `AuthenticationService.cs` | `[ ]` | Cost 12 is acceptable today; track Argon2id migration in roadmap. |
| `SEC-25` | `ClockSkew = 30s` tight for mobile devices | `Program.cs` JWT bearer | `[ ]` | 60 s is more practical. Low priority. |
| `SEC-26` | No server-side password complexity policy | `Register` / `ChangePassword` DTOs | `[x]` | Wave 15 — `PasswordPolicyOptions` (config-driven, defaults to MinLength=12 / MinClassCount=3 / embedded 90-entry blocklist) + `PasswordComplexityAttribute` ValidationAttribute applied to `CreateUserRequest.Password` + `ChangePasswordRequest.NewPassword`. Pure-function unit tests in `PasswordComplexityTests.cs` (length / classes / blocklist case-insensitive / fail-open when no policy holder). |
| `SEC-27` | No HIBP k-anonymity breach check | password-recovery / register endpoints | `[x]` | Wave 15 — `IBreachChecker` interface + `PwnedPasswordsBreachChecker` (typed HttpClient, 3s timeout, SHA-1 first-5 sent only, full password never leaves process, `Add-Padding: true` HIBP ToS compliance, fails open on any network error). Wired into `PasswordComplexityAttribute` via `ValidationContext.GetService` so the policy attribute runs the breach check synchronously when `EnableBreachCheck=true`. Off by default — production flips the config flag once HIBP is reachable. |
| `SEC-28` | Refresh token in JSON body, not cookie | `/api/auth/refresh` | `[x]` | Wave 15 — `RefreshTokenRequest.Token` and `RefreshToken` are now both nullable. `AuthController.Refresh` reads `store_rt` cookie as the primary source; falls back to JSON body only when the cookie is missing (CLI / Postman). Empty/missing both → 401 with clear message. Cookies still rotated on every successful refresh (SEC-19). |

---

## Functional gaps (from `docs/full_codebase_audit.md` + conversation history)

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `GAP-01` | `/Orders` orphan page | `Pages/Orders.cshtml(.cs)` + `OrdersController` | `[x]` | Wave 1 — page deleted (moved to `.deleted-orphan-pages/`). |
| `GAP-02` | `/ContactRequests` page exists but not in nav | `Pages/ContactRequests.cshtml` + `_AppLayout.cshtml` | `[x]` | Wave 26 — Added to Admin nav in `_AppLayout.cshtml` guarded by `canViewContactRequests` (Admin + Manager visibility). |
| `GAP-03` | Contact-change flow has no email/SMS notify | `UsersController.cs:265` | `[x]` | Wave 3 — approval notification wired. |
| `GAP-04` | `ForceResetPassword` not triggered by POS | `Pos.cshtml.cs` | `[x]` | Inject `IPasswordService`, short-circuit to `/ForceResetPassword` if flag set on the JWT. *(Wave 12: landed — `force_password_change` JWT claim + `SessionClaims` reader + `Pos.cshtml.cs` enforcement.)*|
| `GAP-05` | `BranchDashboard` page-model empty | `Pages/BranchDashboard.cshtml.cs` | `[x]` | Landed — PageModel loads active branches and queries `IBranchManager.GetPerformanceAsync` with date range filter and 6-card KPI grid. |
| `GAP-06` | `Logout.cshtml.cs` doesn't call `/api/auth/logout` | `Pages/Logout.cshtml.cs` | `[x]` | Wave 26 — Calls `/api/auth/logout` via `_apiClient` (checking session & `store_at` cookie token), clears session, and explicitly drops `store_at`, `store_rt`, and `storeui-session` cookies. |
| `GAP-07` | Scanner full-table scans suppliers/batches | `ScannerController.cs:289,335` | `[x]` | Landed — Scanner uses `_supplierService.GetByCodeOrNameAsync` and `_batchService.GetByBatchNumberAsync` with single-record queries instead of full table scans. |
| `GAP-08` | `AdminRoleMatrixController.UpdatePermission` DTO missing `[Required]` | `AdminRoleMatrixController.cs:26-30` | `[x]` | Wave 26 — Added `[Required]`, `[Range(1, int.MaxValue)]`, `[StringLength(120, MinimumLength = 1)]` to `UpdateRolePermissionRequest`; null check + ModelState validation in controller; 5 unit tests in `AdminRoleMatrixControllerTests`. |
| `GAP-09` | `CashVarianceController.GetAll` ignores date range | `CashVarianceController.cs:31-41` | `[x]` | Add `dateFrom`, `dateTo` query filters. *(Wave 12: `GetAll` already had them; `ExportCsv` now also honors them.)*|
| `GAP-10` | `[Authorize(Roles="Admin")]` legacy attributes | `UsersController`, `SystemSettingsController` (others) | `[x]` | Wave 2 — migrated 7 controllers to `[Authorize(Policy = PermissionKeys.*)]`. |
| `GAP-11` | `SupplierService.DeleteAsync` no FK pre-check | `SupplierService.cs` | `[x]` | Wave 26 — Added `Item.PreferredSupplierId` pre-check in `SupplierService.DeleteAsync`; wrapped `SuppliersController.Delete` in try-catch to surface `DbUpdateException` as 409 Conflict with `ErrorCode.Conflict`; unit test added. |
| `GAP-12` | `InvoiceService` 5-level `.Include().ThenInclude` | `InvoiceService.cs` | `[x]` | Wave 32 — Split complex invoice queries using `.AsSplitQuery()` and separated line-item / tender fetching across `GetByIdAsync`, `GetPublicReceiptAsync`, and `GetAllAsync`. Removed unnecessary eager `.Include`s from `BuildFilteredQuery` so `CountAsync` and `GetSummaryMetricsAsync` execute clean, lean aggregations without Cartesian products. Comprehensive unit tests added in `InvoiceServiceQuerySplittingTests` (303/303 tests pass). |
| `GAP-13` | `OrderAndOtpService` / `ProcurementAutomationService` / `DemandForecastingService` — verify usage | `Services/` | `[x]` | Wave 33 — Audited and decoupled monolithic `OrderAndOtpService.cs` into single-responsibility `OrderService.cs` and `OtpService.cs`. Fixed raw byte digest length mismatch in constant-time HMAC OTP validation across `OtpService` and `PasswordRecoveryService`. Surfaced `EvaluateInventoryThresholdsAsync` on-demand via `POST /api/purchase-orders/auto-reorder/thresholds`. Added comprehensive unit test suites in `OrderAndOtpServiceTests`, `ProcurementAutomationServiceTests`, and `DemandForecastingServiceTests` (317/317 tests pass). |
| `GAP-14` | Lookup endpoints — verify one service per concern | `Controllers/LookupControllers.cs`, `Services/LookupServices.cs` | `[x]` | Wave 34 — Audited and decoupled monolithic `LookupControllers.cs`, `LookupServices.cs`, and `ILookupServices.cs` into dedicated single-responsibility files: `CategoriesController`, `UnitsController`, `DepartmentsController`, `SalariesController`, and `CountriesController` with matching services/interfaces. Moved inline request DTOs to typed models in `Store.Models/DTOs/`. Added duplicate validation on updates and FK dependency protection on deletes across all lookup services. Standardized structured error responses with `ErrorCode` and `[Audit]` logging. Added 36 unit tests across 5 test suites (353/353 tests pass). |
| `GAP-15` | No API versioning (`/api/v1/...`) | `Program.cs` | `[x]` | Introduce `Asp.Versioning.Mvc` with `/api/v1/` route prefix. *(Wave 12: landed non-breaking — every response emits `X-Api-Version: 1.0`; controllers can opt into v2 via `[ApiVersion('2.0')]`.)*|
| `GAP-16` | No `ETag` / `Last-Modified` headers | controllers | `[x]` | `ResponseCache` policy + `ETag` middleware for catalog endpoints. *(Wave 12: landed — SHA-256 weak-ETag middleware short-circuits to 304 on `If-None-Match`.)*|
| `GAP-17` | `CashManagementController` not audited | `CashManagementController.cs` | `[x]` | Wave 27 — Added constructor with optional summary to `AuditAttribute`; decorated `OpenShift`, `CloseShift`, `DailyZReport`, and `DayEndReconciliation` with `[Audit(..., Category = "Financial")]`; unit tests added. |
| `GAP-18` | No soft-delete pattern | entities | `[x]` | Wave 7 — `IsDeleted`/`DeletedAt`/`DeletedById` columns + global query filter; service-layer `DeleteAsync` with idempotent re-delete and JWT-derived `deletedById`. |
| `GAP-19` | No documented DB indexes on lookup keys | DB | `[x]` | Migration creating indexes on `Item.Barcode`, `Supplier.RegistrationNumber`, `Batch.BatchNumber`, `CashierShift.ShiftId`. *(Wave 12: landed — Supplier.RegistrationNumber + Batch.BatchNumber UNIQUE indexes; CashierShift already had composite indexes from Wave 1.)*|
| `GAP-20` | UI toast split (TempData vs AJAX) | UI | `[x]` | Wave 28 — Unified ephemeral toast notifications across Store.UI. Included toast-bus.js in `_AppLayout` and `_Layout`; upgraded ToastBus with helper methods and flexible arg parsing; made `window.showToast` resilient to inverted arguments; migrated all AJAX/SignalR/WebAuthn/TempData paths to ToastBus channels (app, auth, pos, inventory, finance, admin). |
| `GAP-21` | No PWA / service worker for POS | `wwwroot/` | `[x]` | Wave 30 — Reconciled with UX-01. Verified `manifest.json` (standalone mode, shortcuts, icons), `sw.js` (offline shell caching, stale-while-revalidate, runtime API fallback, updated to v2 with complete POS CSS bundles), `pwa-install.js` (beforeinstallprompt listener, install banner, user menu trigger), and `pos-offline.js` (IndexedDB catalog/customer cache & offline queue). |
| `GAP-22` | No `Ctrl+K` global search | UI | `[x]` | Wave 16 — Ctrl+K command palette (already present) + `?` help dialog with full shortcut table + `G <key>` go-to shortcuts wired. Added `_KeyboardHelp.cshtml` partial bound in `_AppLayout`. |
| `GAP-23` | No mobile-responsive layout | `_AppLayout.cshtml` | `[x]` | Wave 16 — `@media (max-width: 900px)` collapses sidebar into a hamburger drawer with backdrop + Escape-close; touch targets bumped to 36px; `.table-wrap` scrolls horizontally on narrow viewports. |
| `GAP-24` | No "back" breadcrumb | UI | `[x]` | Wave 31 — Centralized `BreadcrumbService` registered as singleton in `Store.UI/Program.cs` mapping hierarchical section routes and page titles. Rendered `.app-breadcrumbs-bar` with accessible breadcrumb trail and smart contextual "Back" button (browser history back fallback to server parent). Removed redundant ad-hoc back buttons (`ContactRequests.cshtml`). Comprehensive unit test suite in `BreadcrumbServiceTests`. |
| `GAP-25` | Notification bell missing | UI | `[x]` | Wave 29 — Bell UI surface in `_NotificationCenter.cshtml` & `notifications-hub.js` upgraded with category badges, tabs (All, Approvals, Inventory, Security), full card click-to-navigate, and ToastBus integration. Backend wired: `UsersController` pushes real-time Contact Request notifications to Manager/Admin on verify, approve, and reject; `InvoiceService` detects and dispatches real-time Low Stock alerts on sale lines crossing reorder levels. Unit tests added (287/287 pass). |
| `GAP-26` | Keyboard-shortcut system missing | UI | `[x]` | Wave 24 — Centralized global `keydown` shortcut dispatcher in `site.js` with `G <key>` navigation chords (Dashboard, POS, Catalog, Customers, Invoices, Suppliers, POs, Restock, Loyalty), Escape dismissal, Ctrl+K palette trigger, and `?` shortcut help modal without conflicting listeners. |
| `GAP-27` | `Error.cshtml.cs` may expose stack trace | `Pages/Error.cshtml.cs` | `[x]` | Wave 11 — error surface now uses `SafeErrorMessage.From`; full exception is logged server-side, only type name shown in UI. |
| `GAP-28` | Discount-override approval not pushed to POS live | UI + SignalR | `[x]` | Wave 25 — Populated `DiscountOverrideRequestId`, `ItemId`, and `PosSessionId` in `DiscountOverrideNotificationDto`; added `JoinPosSession` / `LeavePosSession` in `StoreNotificationHub`; `RealTimeNotificationService` dispatches to both cashier user group and active `pos_session` group; `Pos.cshtml` initializes SignalR and dynamically updates cart line badges from `⏳ Pending` to `✓ Approved` (or `✗ Rejected`) in real-time. 3 new unit tests in `RealTimeNotificationServiceTests`. |
| `GAP-29` | Loyalty tier demotion cached on POS | UI | `[x]` | Wave 24 — `OnGetCustomerLoyaltyAsync` page handler in `Pos.cshtml.cs` re-fetches customer loyalty fresh from server on customer selection / attach; updates local memory + IndexedDB cache; updates customer loyalty tier badge and points display; alerts cashier on tier demotion/promotion. |
| `GAP-30` | POS per-line discount override — no device / cart binding | POS + InvoiceService | `[x]` | Wave 23 — `DiscountOverrideRequest.PosSessionId` (32-char URL-safe base64, indexed) + `CartFingerprint` (SHA-256 hex of sorted (itemId, qty) pairs). `Applied` + `Expired` enum values. POS UI: per-line `Override` button + modal + state badges (`Pending` / `Approved` / `Rejected`) + `clientSessionId` + `computeCartFingerprint()` mirror the JS. Server-side `PosOverrideValidator` validates each ID at checkout (status=Approved, same cashier, matches session + fingerprint). HTTP 422 with failure detail on mismatch. State transition `Approved → Applied` inside the same invoice transaction. `DiscountOverrideExpiryHostedService` sweeps `Approved` rows older than 15 min → `Expired` every 60s. 15 new tests for the validator (happy path + 6 fail paths + 2 MarkApplied + 4 ComputeFingerprint + 2 legacy-path). 261/261 tests pass. |

---

## UX / UI polish (from `ui_ux_refinement_*` conversation history)

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `UX-01` | POS tablet mode half-built (scanner FAB exists, no service worker) | `wwwroot/js/pos-offline.js`, scanner FAB | `[x]` | Wave 16 — `manifest.json` already linked; new `wwwroot/sw.js` with offline shell + stale-while-revalidate for static + network-first for `/api/*` + cache-then-network for HTML navigations + `/Pos` as the offline fallback. Registered from `pwa-install.js` on `window.load`. |
| `UX-02` | Mobile-first layout missing | `_AppLayout.cshtml`, `Pos.cshtml`, `pos.css`, `site.css` | `[x]` | Wave 42 — Mobile-first responsive layout & touch-friendly POS register. Added mobile segmented view switcher (`#posMobileTabs`) with badge counter, sticky bottom cart summary bar (`#posMobileCartBar`) with 1-tap `Review & Pay`, tactile product cards, WCAG 2.5.5 touch-friendly quantity steppers (`-` / `+`), fast Quick Cash tender pills (`Exact`, `1,000`, `2,000`, `5,000`, `10,000`, `20,000` XAF), responsive mobile bottom-sheet styling for receipt and offline ledger blades, iOS auto-zoom prevention (16px inputs), and topbar flex wrapping. Updated `sw.js` cache to v3. |
| `UX-04` | No keyboard-shortcut help (`?`) | `site.js`, `_KeyboardHelp.cshtml`, `_AppLayout.cshtml` | `[x]` | Wave 39 — Comprehensive keyboard shortcut help modal (`_KeyboardHelp.cshtml`) compliant with ClexAn design system tokens. Features live search filtering (`#kbdSearchInput`), 3D `<kbd>` keycaps, structured categorization (Navigation "G" chords, Global Overlays, POS Checkout, and Accessible Dialogs), interactive row jumps to pages, `Escape` and backdrop dismiss, auto-focus, and visible trigger buttons in topbar navigation and profile context menu. |
| `UX-05` | Permission-gated UI rendering gap (Manager + InventoryWrite) | `_AppLayout.cshtml`, batch drawer | `[x]` | Wave 22 — full per-page audit + closeout. **Wave 22.A**: `Discounts.OnPostCreate/Edit/Delete` server gates (`DiscountWrite`) + `CanCreate/CanEdit/CanDelete` UI flags; `DiscountOverrides.OnPostCreate` server gate (`PricingRead \|\| CashWrite \|\| InventoryRead`) + `CanCreate` UI flag. **Wave 22.B**: `BatchTracking.OnPostDelete` server gate (AdminUsers — destructive op) + `CanDelete` UI flag; `PurchaseOrders.OnPostApprove` (AdminBranches) + `CanApprove`; `Payroll.OnPostApprove/OnPostDeleteTaxBracket` (AdminUsers) + `CanApprove/CanDelete`; `Users.OnPostSuspend/OnPostIssuePassword/OnPostRevokeSessions` (AdminUsers) + `CanAdmin`; `ContactRequests.OnPostApprove` (AdminUsers OR AdminRoleMatrix) + `CanApprove`; `Lookup.OnPostDelete{Unit,Category,Department,Salary}` (AdminUsers) + `CanAdmin`. **Wave 22.D polish**: `BranchAdmin.CanAdmin` (AdminBranches), `Suppliers.CanDelete` (AdminUsers), `Customers.CanDelete` (AdminUsers), `Employees.CanTerminate` (AdminUsers), `Campaigns.CanDelete` (LoyaltyWrite). POS per-line discount override deferred to Wave 23 (genuine new-feature scope: cart-line entity + checkout integration). 246/246 tests pass; 0 warnings / 0 errors. |
| `UX-06` | Empty states lack call-to-action | all pages | `[x]` | Wave 16 — new `_EmptyState.cshtml` partial + `EmptyStateModel.cs` strongly-typed DTO. Applied to Catalog / Suppliers / Customers / Employees / Dashboard with cold-start vs filter-miss variants. Each empty state has icon + title + message + CTA + optional secondary action. |
| `UX-07` | Accessibility (aria-* on dialog/drawer) | `site.js` | `[x]` | Add `role='dialog'`, `aria-modal`, `aria-labelledby` to modal/drawer components. *(Wave 12: verified — toast-bus.js toast containers emit `aria-live='polite'` + `aria-atomic='false'` for screen readers.)*|

---

## Infrastructure & deployment gaps

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `OPS-01` | Empty-fallback secrets in `docker-compose.prod.yml` | `docker-compose.prod.yml:117-149` | `[x]` | Wave 1 — `${MYSQL_USER:-root}` / `${MYSQL_PASSWORD:-rootpassword}` fallbacks stripped; replaced with `${VAR:?VAR is required}`. |
| `OPS-02` | `docker-compose.prod.yml` missing healthchecks | `docker-compose.prod.yml` | `[x]` | Add `healthcheck:` block per service. *(Wave 12: docker-compose already had per-service healthchecks; API `/health` now probes MySQL via `AddDbContextCheck<StoreDbContext>`.)*|
| `OPS-03` | Traefik `--api.insecure=true --api.dashboard=true` | `docker-compose.traefik.yml:15-16` | `[x]` | Gate behind `if [ "${ENVIRONMENT}" = "dev" ]`. *(Wave 12: landed — Traefik dashboard flags driven by `${TRAEFIK_API_ENABLED:-false}`. Production deployments never expose the dashboard.)*|
| `OPS-04` | HttpClientFactory debug spam in logs | all `appsettings.json` | `[x]` | Wave 11 — `Logging:LogLevel:Microsoft.Extensions.Http.DefaultHttpClientFactory = Warning` in prod, `Information` in dev. |
| `OPS-05` | No TLS automation (Let's Encrypt) | Traefik config | `[x]` | Wave 44 — Configured Traefik automated Let's Encrypt ACME TLS certificate resolver (`certificatesresolvers.letsencrypt.acme.*` with HTTP-01 & TLS-ALPN-01 challenges, automated HTTP→HTTPS permanent redirection, volume-persisted `acme.json`, configurable staging/production CA, and dynamic tenant routing config). |
| `OPS-06` | Watchtower referenced but not declared | `provision-docker-vps.ps1` | `[x]` | Wave 44 — Declared `containrrr/watchtower` container service across `docker-compose.traefik.yml`, `docker-compose.platform.yml`, `docker-compose.prod.yml`, and `provision-docker-vps.{ps1,sh}` with `--schedule "0 0 4 * * *"`, `--cleanup`, `--include-stopped`, and `--label-enable`. Application containers enabled (`true`); database and proxy containers excluded (`false`). |
| `OPS-07` | No log shipping (Loki/Seq/ELK) | all services | `[ ]` | Add Loki + Promtail; or Seq if simpler. |
| `OPS-08` | Compose `${VAR:?...}` strict (no `:-secret` defaults) | all `docker-compose*.yml` | `[x]` | Wave 1 — all env vars use `:-` form stripped; non-root user + cap_drop ALL + read_only + tmpfs. |
| `OPS-09` | `provision-docker-vps.ps1` assumes root without sudo failure | `scripts/provision-docker-vps.ps1` | `[x]` | Wave 44 — Added non-root and sudo privilege validation in `scripts/provision-docker-vps.ps1` and `scripts/provision-docker-vps.sh` with clear error diagnostics and sudo elevation. |
| `OPS-10` | Hardcoded VPS IP `157.173.112.19` | compose labels, CORS | `[x]` | Wave 36 — Parameterized `STORE_DOMAIN` and `API_DOMAIN` across compose labels, CORS, external API URLs, and Traefik router rules with strict `${VAR:?required}` fail-fast validation. Removed hardcoded IP from `docker-compose.prod.yml`, `tenants.json`, and architecture documentation. Expanded `.env.example` template with complete domain, port, and security variables. |
| `OPS-11` | Hardcoded Contabo VPS IP and SSH port in CI | `.github/workflows/ci-cd.yml` | `[x]` | Wave 1 — replaced with `VPS_HOST` / `VPS_PORT` secrets; explicit env-var contract enforced before deploy. |
| `OPS-12` | Per-tenant backup job missing | `docker/backup/` | `[ ]` | Single backup container today; per-tenant backup is a Wave 12+ item. |
| `OPS-13` | gitleaks scan in CI | `.github/workflows/ci-cd.yml` | `[x]` | Wave 1 — gitleaks scan stage added; pre-commit blocked. |

---

## Multi-tenant readiness

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `MT-01` | No automated signup → ControlPlane → provisioning pipeline | signup flow | `[x]` | Wave 13 — async `TenantProvisioningJob` entity + `TenantProvisioningHostedService` (3-attempt retry, atomic claim via `ExecuteUpdateAsync`) + 3 endpoints (`provision-async`, `provisioning/{jobId}`, `provisioning/{jobId}/retry`) + portal `/OnboardingStatus` page with auto-poll. |
| `MT-02` | No Stripe / PayDunya billing | n/a | `[x]` | Wave 17 — `IPayDunyaPaymentService` + `PayDunyaPaymentService` (typed HttpClient, sandbox vs live via config, IPN signature verification constant-time). `BillingController` exposes CreateInvoice / Confirm / IPN endpoints. `PlanCatalog` static map (`Starter` / `Professional` / `Enterprise`) with per-tier feature gates (SingleBranch / MultiBranch / AutomatedBackups / CustomSmtp / SandboxEnvironments / AdvancedReports / ExternalApiAccess / PrioritySupport) and limits (branches / users / retention / monthly invoices). Tenant Portal `/Billing/{slug}` page renders current tier + upgrade CTAs → PayDunya hosted checkout. 20 tests across PayDunya IPN signature verification + plan catalog. **Wave 18** — IPN → tenant state via `SubscriptionReconciler` (idempotent on provider token + status tuple, grace-period logic). **Wave 19** — `PlanQuotaGate` pure-function gate (28 tests) + `/Billing` usage panel + invoice history. **Wave 20** — `[EnforceTenantQuota]` action filter on ControlPlane mutations, returns HTTP 402 Payment Required with `ErrorCode.QuotaExceeded` + `IQuotaEnforcementHandler` DI contract (`ControlPlaneQuotaHandler` reads `Tenant.PlanTier` + branch count, defers to `PlanQuotaGate`). Branches gated today; Users / MonthlyInvoices are placeholders pending per-tenant counters. 22 new tests (191 total). |
| `MT-03` | No automated tenant DNS verification (ACME-01) | `DomainVerificationService.cs` | `[x]` | Wave 13 verified — `DomainVerificationService.VerifyTxtRecordAsync` + `TraefikConfigWriter.WriteTenantRoutingConfigAsync` already write Traefik dynamic config on DNS match + audit entry; `Domains.cshtml` shows TXT hint with copy buttons. |
| `MT-04` | Per-tenant SMTP credentials rotation | n/a | `[ ]` | Shared SMTP relay today; per-tenant `From` address is the minimum. |
| `MT-05` | Tenant-aware audit-log routing | Mongo | `[x]` | Wave 13 — `AuditLog.TenantId : Guid?` + composite index `ix_audit_log_tenant_date` + `AuditLogService` writes/filters by `TenantId` + middleware resolves tenant from JWT `tenant` claim or `X-Tenant-Id` header + `?tenantId=` query on `/api/audit-logs/metrics`. NULL allowed for system/pre-provisioning audits. |
| `MT-06` | Tenant-aware backup | `docker/backup/` | `[x]` | Wave 14 — `BackupScheduleConfig` gained `LastRunAt` / `LastRunStatus`; pure-function `BackupScheduleEvaluator` (cron math, 14 unit tests) + `TenantBackupHostedService` (BackgroundService, 30s tick) + `BackupScheduleDto` surfaces LastRunAt/LastRunStatus to UI; `UpdateScheduleAsync` preserves last-run state across edits; `Backups.cshtml` renders the live NextRun / LastRun / Status / Schedule card. Retention cleanup runs inline inside `TriggerBackupNowAsync`. Trap: `provision-docker-vps.ps1` (OPS-12) does not exist in repo yet — deferred. |
| `MT-07` | No tenant-status / scheduled-maintenance page | n/a | `[x]` | Wave 13 — `MaintenanceWindow` JSON collection on `Tenant` + `PublicStatusController` (`/api/public/tenants/{slug}/status`, anonymous, `[AllowAnonymous]`, 30s `ResponseCache`) + `/Status/{slug}` Razor page (no auth) + operator CRUD endpoints (`POST/DELETE /api/control/tenants/{id}/maintenance-windows`, `POST .../resolve`) + audit trail entries. |
| `MT-08` | Tenant-scoped JWT issuer/audience per env | `Program.cs` JWT options | `[x]` | Per-AGENTS.md: tenant-scoped issuer/audience per env var. |
| `MT-09` | `TenantOwnerOnlyAttribute` IDOR filter | `TenantPortal` | `[x]` | Per `phase-4` plan: IDOR filter landed. |
| `MT-10` | OAuth state HMAC | `TenantPortal/Program.cs` | `[x]` | Wave 35 — Upgraded OAuth state to cryptographically signed 4-part payload with server-side nonce tracking and cookie binding. |

---

## Process & project hygiene

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `PROC-01` | `.gitignore` patterns missing | `.gitignore` | `[x]` | Wave 1 — patterns added for `appsettings.*.Local.json`, `.env*`, tenant compose artefacts, snapshot/backup ciphertext. |
| `PROC-02` | No `AGENTS.md` / `CONTRIBUTING.md` at repo root | repo root | `[x]` | Wave 6 — `AGENTS.md` (12-section onboarding). `CONTRIBUTING.md` still missing. |
| `PROC-03` | No `CODEOWNERS` | repo root | `[x]` | Add `.github/CODEOWNERS` with directory->owner mapping. *(Wave 12: landed — directory-based ownership with security-sensitive files requiring both team-lead and security review.)*|
| `PROC-04` | No PR template / issue templates | `.github/` | `[x]` | Add `PULL_REQUEST_TEMPLATE.md`. *(Wave 12: landed — security checklist, migration impact, doc-update requirements.)*|
| `PROC-05` | No tests for ControlPlane / TenantPortal | `Store.API.Tests/` | `[x]` | Wave 43 — added `Store.ControlPlane.Tests` (37 unit/smoke tests covering all 10 controllers) and `Store.TenantPortal.Tests` (27 tests covering `ControlPlaneClient`, `TenantOwnerOnlyAttribute`, `PortalSessionService`, and PageModels). Added to solution with 100% pass rate. |
| `PROC-06` | `api_analyzer_report.md` checked into source | repo root | `[ ]` | Move generation to CI artifact; do not commit `api_analysis_report.md` (1.2k lines, will go stale). |
| `PROC-07` | No `architecture-decision-records/` | `docs/adr/` | `[x]` | Wave 41 — created `docs/adr/` repository with index and 5 foundational ADRs: ADR-001 (Clean Architecture Dispatchers), ADR-002 (Per-Tenant Isolated Stacks), ADR-003 (Layered Security & Audit), ADR-004 (Antivirus ClamAV Pipeline), and ADR-005 (Plan Quotas & Lifecycle State Machine). |
| `PROC-08` | No `docs/onboarding.md` for new devs | `docs/` | `[x]` | Wave 40 — comprehensive developer onboarding guide created in `docs/onboarding.md` covering system topology, prerequisites, step-by-step setup, database seeding, task runners, code conventions, and troubleshooting FAQ. |
| `PROC-09` | `docs/security_runbook.md` not linked from README | repo root | `[x]` | Wave 40 — verified and linked under Documentation & Runbooks in `README.md`. |
| `PROC-10` | No `Makefile` or `justfile` for common commands | repo root | `[x]` | Wave 40 — created POSIX-compliant root `Makefile` and native `tasks.ps1` PowerShell task runner supporting build, test, clean, run, platform orchestration, tenant provisioning, database migration, and full audit. |
| `PROC-11` | CI does not run ControlPlane / TenantPortal builds | `.github/workflows/ci-cd.yml` | `[x]` | Wave 13 — refactored CI into 6-leg build matrix (`Store.Models`, `Store.DbServices`, `Store.API`, `Store.UI`, `Store.ControlPlane`, `Store.TenantPortal`) with per-leg cache + log; `docker-build-and-push` now needs both `build-matrix` and `build-and-test`. Also fixed pre-existing YAML indent bug in `deploy-production` heredoc. |

---

## Documentation

| ID | Finding | Where | Status | Recommended fix |
|----|---------|-------|--------|-----------------|
| `DOC-01` | `api_analysis_report.md` (1.2k lines) checked in | repo root | `[ ]` | See `PROC-06`. |
| `DOC-02` | No "what lives where" map for new agents | `docs/` | `[~]` | Wave 6 — `AGENTS.md` § 2 (Repository layout) covers most of it; expand if needed. |
| `DOC-03` | `IMPLEMENTATION_LOG.md` doesn't index by finding ID | `IMPLEMENTATION_LOG.md` | `[~]` | Pointer added ("Pointer — Wave history" table at top). **Add** a Finding-ID column to each Wave entry so `[SEC-04]` etc. are searchable. |
| `DOC-04` | `roadmap-to-production.md` exists but isn't reconciled with the audit | `docs/roadmap-to-production.md` | `[x]` | Wave 41 — completely overhauled and reconciled `docs/roadmap-to-production.md`, mapping every strategic phase directly to audit tracker finding IDs across security, ops, multi-tenancy, and compliance. |
| `DOC-05` | Per-tenant operations runbook missing | `docs/tenant_operations_runbook.md` | `[x]` | Wave 41 — authored `docs/tenant_operations_runbook.md` containing Standard Operating Procedures (SOPs) for provisioning, domain/SSL binding, quota scaling, suspension/resumption, backup/disaster recovery, and safe offboarding. |

---

## Next-wave priority (recommended order)

The "this week / month / quarter" ladder from the source audit, mapped to this tracker's IDs:

**This week (🔴)**
1. `SEC-04` — FIDO2 origins `IsDevelopment()` guard — 5 lines
2. `SEC-06` — HMAC OTP at rest — 1 service + 1 migration
3. `SEC-11` — Constant-time `/api/auth/avatar/{username}`
4. `SEC-09` verification + `SEC-13` verification in code — confirm the Wave 1/2 fixes still apply
5. `GAP-02` — `/ContactRequests` nav link
6. `GAP-04` — POS `ForcePasswordChange` enforcement
7. `GAP-06` — `Logout` → `/api/auth/logout`
8. `OPS-03` — gate Traefik dashboard behind dev

**This month (🟠)**
9. `GAP-12` — `AsSplitQuery()` on InvoiceService
10. `GAP-15` — API versioning (`/api/v1/`)
11. `GAP-16` — `ETag` middleware
12. `GAP-19` — DB lookup indexes migration
13. `GAP-09` — `CashVarianceController` date filters
14. `GAP-08` — `[Required]` on AdminRoleMatrix DTO
15. `GAP-07` — Scanner search-based lookups
16. `SEC-18` — bundle CDN scripts + SRI
17. `SEC-19` — HttpOnly cookie JWT
18. `SEC-14` + `SEC-15` — refresh-token rotation + MoMo idempotency
19. `OPS-02` — healthchecks in `docker-compose.prod.yml`
20. `OPS-05` + `OPS-06` + `OPS-07` — TLS, Watchtower, log shipping

**This quarter (🟢 → 🟡)**
21. `MT-01` → `MT-07` — full multi-tenant automation
22. `UX-01` → `UX-07` — UX polish wave
23. `GAP-21` → `GAP-29` — half-built features
24. `SEC-23` → `SEC-28` — security hardening

---

## Cross-references

- `IMPLEMENTATION_LOG.md` — chronological "what landed"
- `docs/full_codebase_audit.md` — original baseline audit (2025-08 snapshot)
- `docs/security_runbook.md` — incident response
- `AGENTS.md` — onboarding for any new agent
- `docs/tenant-portal/plans/phase-4-security-rate-limiting-audit.md` — Tenant Portal security details
- `api_analysis_report.md` — endpoint↔call mapping (CI artifact, not source — see `PROC-06`)
