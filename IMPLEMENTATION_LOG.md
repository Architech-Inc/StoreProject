# StoreProject — Implementation Log

> Master tracker for the end-to-end remediation, hardening and feature-completion work
> initiated from the deep audit (`api_analysis_report.md`, `docs/full_codebase_audit.md`,
> `docs/tenant-portal/06-security-spec.md`).
>
> Every change made in this effort is appended below in execution order. **No item is
> considered done until it has its log entry, its code/doc change, and (where applicable)
> its tests passing.**
>
> **Companion file:** `docs/audit-tracker.md` is the forward-looking view — every open
> finding with severity, status, and recommended fix, plus the recommended Wave-12 priority
> order. The Pointer table at the top of that file shows which Wave closed which items.

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
- `[3.7-3.8]` **ROLLED BACK** → **COMPLETED** (see "Soft-delete migration" below). Adding `IsDeleted` to `BaseEntity` without a database migration caused every entity table to be queried for `is_deleted` at runtime and broke the API. After the proper migration was generated and the transitive child filters were added, this wave is fully landed.

---

## Soft-delete rollback (2026-09-13, same session)

**What happened:**
Adding `IsDeleted` / `DeletedAt` / `DeletedById` to `BaseEntity` and registering
`HasQueryFilter(e => !e.IsDeleted)` on `Item` / `Supplier` / `Employee` / `Customer`
triggered two runtime failures:

1. **`Unknown column 'i.is_deleted' in 'where clause'`** — EF Core generated a
   query that included `is_deleted` for every entity table that derives from
   `BaseEntity`. None of those tables have the column because no migration was
   generated.
2. **`Model.Validation[10622]`** — the four entities with the global filter
   have required navigation properties to ~20 child entities (Batch,
   BranchItemStock, BundleRule, …). EF Core correctly warned that filtering
   the parent without filtering the child could orphan the child rows.

**What was done next (same session):** Rolled forward, properly, with a coordinated migration. GAP-18 is now CLOSED — see the bottom of this file for the full "Soft-delete migration — completed" change-log entry.

**What's needed for soft-delete to land safely:**

1. Generate a migration via `dotnet ef migrations add AddSoftDeleteColumns --project
   Store.DbServices --startup-project Store.API` that adds:
   - `is_deleted TINYINT(1) NOT NULL DEFAULT 0`
   - `deleted_at DATETIME(6) NULL`
   - `deleted_by_id CHAR(36) NULL`
   to every BaseEntity-derived table.
2. Either:
   (a) Apply the filter only to entities whose `DeleteAsync` is being converted
       in the same change, **or**
   (b) Add the filter to a separate `SoftDeletableEntity` base class so other
       entities are unaffected.
3. Add matching `HasQueryFilter` to every child entity that has a required
   navigation to a soft-deletable parent (Batch → Item, etc.) to satisfy
   EF Core 10622. Easiest pattern: `b => !b.Item.IsDeleted` where applicable.
4. Convert service-layer `DeleteAsync` to set `IsDeleted = true, DeletedAt =
   utcNow, DeletedById = currentUser` instead of `Remove(entity)`.

**GAP-18 — CLOSED.** Soft-delete is in place end-to-end: schema columns, global
query filter (with transitive child coverage), service-layer deletes that set
the columns instead of removing the row, and audit columns populated from the
JWT user-id at the controller boundary.

### 2026-09-13 — Wave 8 (Restock Recommendations — completed)

The "Restock & Warehouse Orchestration" page (`/Restock`) was failing with
`Error: Failed to generate purchase order` because the UI was calling API
endpoints that didn't exist. The forecasting service (`IDemandForecastingService`)
was already implemented but had no controller surface.

**Files added / changed:**

1. **`Store.Models/DTOs/Operations/RestockDtos.cs`** (new) —
   `RestockRecommendationDto`, `RestockConversionResultDto`, `RestockSummaryDto`.

2. **`Store.API/Controllers/RestockController.cs`** (new) — `[Route("api/Restock")]`
   with five endpoints:
   - `GET api/Restock/pending?branchId=`
   - `GET api/Restock/summary`
   - `POST api/Restock/{id}/convert-to-transfer`
   - `POST api/Restock/{id}/convert-to-po`
   - `POST api/Restock/{id}/dismiss`
   - `POST api/Restock/run-forecast`
   
   Permission keys: `InventoryRead` for GET, `PurchaseOrderWrite` for PO/transfer,
   `InventoryWrite` for dismiss/forecast. JWT `uid` populates `requested_by_user_id`.

3. **`Store.UI/Pages/RestockRecommendations.cshtml.cs`** — uses the new DTOs;
   each conversion handler now reads the `Success` flag from the API response
   and shows the matching message (e.g. "Could not generate a purchase order —
   register a supplier first."). `OnPostSyncForecastAsync` wires the previously
   dead "Sync AI Forecast" button.

4. **`Store.UI/Pages/RestockRecommendations.cshtml`** — three new columns:
   - **Stock** — current on-hand qty (red if 0), days-of-stock at current
     velocity, reorder level
   - **AI Forecast / Reason** — severity badge (Critical / High / Medium) +
     reason text
   - **Projected** — PO value in XAF
   
   Empty-state colspan updated from 5 to 7.

### Severity mapping (in controller)

```
if current_stock == 0                          → "Critical"
elif days_of_stock < 3                          → "Critical"
elif days_of_stock < 7                          → "High"
else                                           → "Medium"
```

`days_of_stock` is parsed from the velocity engine's `Reason` string
(`"Velocity: 0.8/day"`) and combined with the stock figure also in the
same string (`"In stock (N)"`).

### What this fixes

| Symptom | Now |
|---|---|
| `Error: Failed to generate purchase order` | API endpoint exists; UI shows real reason if supplier missing. |
| `Sync AI Forecast` button is a no-op | `POST api/Restock/run-forecast` wired; response shows `before`, `after`, `generated`. |
| Critical shortages all count as one bucket | Severity bucketed Critical/High/Medium. |
| KPI cards show flat numbers | Summary endpoint returns authoritative counts. |

### 2026-09-14 — Wave 9 (Restock live updates, preferred-supplier, bulk-order — completed)

Three things the restock page was still missing:

1. **SignalR live updates** — connected managers/admins didn't see new
   recommendations until they refreshed the page.
2. **Preferred-supplier heuristic** — `ConvertToPurchaseOrderAsync` was
   blindly picking the first supplier in the DB. Production-unsafe.
3. **Bulk Order All Critical** — no way to convert every critical row into
   POs in one click.

**Files added / changed:**

1. **`Store.Models/DTOs/Notifications/StoreNotificationDtos.cs`** — new
   `RestockRecommendationNotificationDto` (denormalised payload for the
   hub broadcast) and new `NotificationCategory.RestockRecommendation`
   enum value.

2. **`Store.Models/DTOs/Operations/RestockDtos.cs`** — new DTOs:
   `BulkOrderPurchaseOrderDto`, `BulkOrderSkippedRecommendationDto`,
   `BulkOrderResultDto`.

3. **`Store.API/Hubs/IStoreNotificationClient.cs`** — new hub method
   `ReceiveRestockRecommendation(RestockRecommendationNotificationDto)`.

4. **`Store.Models/Interfaces/Services/IRealTimeNotificationService.cs`** —
   new method `NotifyRestockRecommendationAsync(...)`.

5. **`Store.API/Services/RealTimeNotificationService.cs`** — implementation:
   broadcasts the structured payload to `All` + `branch_{id}` groups, and
   also surfaces a generic toast (NotificationCategory = RestockRecommendation,
   Severity mapped from Critical/High/Medium) to the Manager + Admin role
   groups so the notification bell rings even on pages that aren't the
   restock UI.

6. **`Store.DbServices/Services/IDemandForecastingService.cs`** — new methods
   `PickSupplierForItemAsync(itemId)` and `BulkOrderCriticalAsync(branchId, userId)`.

7. **`Store.DbServices/Services/DemandForecastingService.cs`** —
   - Injects `IRealTimeNotificationService` so the forecasting cycle can
     broadcast each new recommendation.
   - **Preferred-supplier lookup** in `PickSupplierForItemAsync`:
     1. `Item.PreferredSupplierId` (if set and supplier not deleted)
     2. Fallback: any supplier that has previously supplied this item
        (via `OrderItem → ItemsOrder.SupplierId`)
     3. Else `null` with an actionable reason
   - `ConvertToPurchaseOrderAsync` now uses `PickSupplierForItemAsync` and
     stores the pick reason in the PO's `Notes`.
   - **`BulkOrderCriticalAsync`**: groups pending critical recommendations by
     supplier, creates one multi-item PO per supplier, marks every
     recommendation `ConvertedToPurchaseOrder`, returns per-supplier summary
     plus a list of skipped rows (so the UI can prompt the operator to set
     `PreferredSupplierId`).
   - Extracted `ExtractDaysOfStock` / `ComputeSeverity` to `internal static`
     so the controller and service compute the same severity bucket.

8. **`Store.API/Controllers/RestockController.cs`** —
   - Injects `StoreDbContext` to hydrate supplier/branch names in the
     bulk-order response.
   - `convert-to-po` now reads `PickSupplierForItemAsync` and surfaces its
     reason to the UI on failure (no more generic "failed" message).
   - New endpoint `POST /api/Restock/bulk-order-critical?branchId=`.

9. **`Store.UI/wwwroot/js/notifications-hub.js`** — new `connection.on('ReceiveRestockRecommendation', ...)`
   handler. Pushes a generic toast via `addNotification(...)` and dispatches
   the structured payload as a `CustomEvent('restock-recommendation-arrived')`
   so the page can update without a reload.

10. **`Store.UI/Pages/RestockRecommendations.cshtml(.cs)`** —
    - Page model: new `LastBulkResult` property (preserved across the
      post-redirect via `TempData["BulkResult"]`); new
      `OnPostBulkOrderCriticalAsync` handler.
    - Razor: new "Bulk Order All Critical" button (red, only renders when
      `criticalCount > 0`); bulk-result panel listing skipped recommendations
      with their actionable reason and the created POs.
    - Razor: `data-restock-id` attribute on each row so the SignalR handler
      can dedupe inserts.
    - Razor: inline JS in `@section Scripts` subscribes to the
      `restock-recommendation-arrived` event — injects a green-flash row at
      the top, increments KPI tiles, updates the bulk-order button counter.

### Operator runbook — verify locally

```bash
# 1. Trigger the AI forecast cycle
curl -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5135/api/Restock/run-forecast

# 2. Confirm SignalR broadcasts (open the page in a browser, watch the network tab for /hubs/notifications)
# Every new recommendation should push to all connected clients within ~1s.

# 3. Set Item.PreferredSupplierId on a Chin-chin item, then click Bulk Order All Critical.
UPDATE item SET preferred_supplier_id = (SELECT supplier_id FROM supplier LIMIT 1)
WHERE name = 'Chin-chin';
# Refresh /Restock, click the red button. You should see one new PO with the supplier you picked.

# 4. Verify the new PO has the supplier-pick reason embedded in Notes.
SELECT purchase_order_id, supplier_id, branch_id, status, notes
FROM purchase_order
WHERE notes LIKE 'Bulk generated from Restock Recommendations%'
ORDER BY date_created DESC LIMIT 3;
```

### Why this matters

- **Preferred-supplier lookup** turns a brittle "first row wins" hack into a
  deterministic, audit-traceable choice. Every PO generated now embeds the
  pick reason in its `Notes` column, so you can always tell why a particular
  supplier was selected.
- **Bulk Order All Critical** turns a multi-minute workflow (click 5+
  "Order PO" buttons, each picking a supplier, each waiting for a round-trip)
  into a single click. The result panel surfaces skipped rows so the
  operator knows exactly which items need their catalog fixed.
- **Live SignalR** turns a polling UX (refresh every minute to see new
  recommendations) into a push UX — critical for stores where stockouts
  cost real money per minute.

### 2026-09-14 — Wave 10 (Failed-message triage)

Triaged all 336 `failed`/`Failed` occurrences across the codebase:

| Category | Count | Status |
|---|---|---|
| A. Legitimate log + user-friendly error surface | ~250 | Kept |
| B. Log-only "failed" (notification hub, file storage) | 8 | Kept — correct semantics (swallow so call flow continues) |
| C. Hard-coded error codes (`disable_2fa_failed`, etc.) | 6 | Kept — machine-readable API codes |
| D. JS front-end failures (`pos-offline.js`, etc.) | 12 | Kept — offline queue state machine |
| E. Documentation / conversation history | ~50 | Kept |
| F. **Half-baked generic "Failed to" messages** | 3 | **Fixed** |

**Files changed:**

1. **`Store.API/Contracts/ApiErrorResponse.cs`** — default `Message` changed from
   `"Request failed."` (zero info) to
   `"The request could not be completed. See server logs for details."` — actionable,
   tells the operator where to look, doesn't leak internals. Callers that
   already set a specific message continue to work.

2. **`Store.Models/Interfaces/Services/ISystemSettingService.cs`** — replaced the
   silent `bool` return type with `SystemSettingUpdateResult(Success, FailureReason)`,
   a record. The service surfaces SQL constraint errors (`DbUpdateException`)
   via `FailureReason` so the controller can tell the operator *which*
   constraint fired instead of "Failed to update setting."

3. **`Store.DbServices/Services/SystemSettingService.cs`** — implementation
   catches `DbUpdateException` separately and embeds `InnerException.Message`
   (which carries the actual MySQL error like "Duplicate entry", "Data
   truncation", etc.). Generic `Exception` falls back to `ex.Message`.

4. **`Store.API/Controllers/SystemSettingsController.cs`** — controller maps
   `!result.Success` to
   `Failed to update setting '<key>': <reason>`
   with HTTP 400 and error code `"update_failed"`. TraceId included for log
   correlation.

**Files NOT changed (verified OK):**

- `PaymentsController.cs:153` "Failed to compute HMAC" — caller already
  returns `Unauthorized()` on false; the internal `return false` is the
  correct way to fail signature validation. TraceId on the response lets
  the operator grep logs.
- `PaymentsController.cs:181` "Failed to deserialize MoMo callback" —
  caller already returns `BadRequest("Invalid payload")`. Logged with
  full exception for post-mortem.
- `RealTimeNotificationService.cs` (7× "Failed to send …") — these are
  intentional; a failed SignalR push must not break the request flow.
- All `UsersController.cs` "failed" codes (`disable_2fa_failed`, etc.) —
  these are API error codes for machine consumers, not generic messages.
- All `Profile.cshtml.cs` / `Pos.cshtml.cs` / `Lookup.cshtml.cs` catch
  blocks — they log the exception and surface a `StatusMessage` to the
  user. The `ex.Message` surface is a separate security concern (raw
  exception leak) tracked in the audit but kept for now since the
  exception messages don't carry credentials or sensitive schema info
  in this codebase.

### Operator runbook — verify in your local DB

```sql
-- 1. Confirm there are pending recommendations
SELECT COUNT(*) FROM restock_recommendation WHERE status = 0;  -- 0 = Pending

-- 2. Confirm the API exposes the endpoints
curl -H "Authorization: Bearer $TOKEN" http://localhost:5135/api/Restock/pending
curl -H "Authorization: Bearer $TOKEN" http://localhost:5135/api/Restock/summary
curl -X POST -H "Authorization: Bearer $TOKEN" http://localhost:5135/api/Restock/run-forecast

-- 3. Confirm a conversion creates a real PO with the right supplier
SELECT po.purchase_order_id, po.status, s.name AS supplier, poi.ordered_quantity
FROM purchase_order po
JOIN supplier s ON s.supplier_id = po.supplier_id
LEFT JOIN purchase_order_item poi ON poi.purchase_order_id = po.purchase_order_id
WHERE po.notes LIKE 'Generated from Restock Recommendation%'
ORDER BY po.date_created DESC LIMIT 5;
```

If step 1 returns 0 and the table is empty, the `AutomatedReorderWorker` has
not run yet (it's a hosted service; it kicks off shortly after startup). You
can manually trigger it via step 2's `run-forecast` endpoint.
query filter (with transitive child coverage), service-layer deletes that set
the columns instead of removing the row, and audit columns populated from the
JWT user-id at the controller boundary.

To apply the migration against your DB:

```
dotnet ef database update --project Store.DbServices --startup-project Store.API
```

### 2026-09-13 — Wave 5 (Operational maturity)

- `[5.1]` `Store.DbServices/Abstractions/IVirusScanner.cs` (new), `Store.DbServices/Services/ClamAvVirusScanner.cs` (new) → ClamAV sidecar client; multipart upload to `/scan`, parses nikolaik/clamav-rest JSON envelope.
- `[5.1]` `Store.API/Controllers/FilesController.cs` → antivirus scan BEFORE image processing & saving; rejects uploads with `415 Unsupported Media Type` on a positive ClamAV hit.
- `[5.1]` `Store.API/Program.cs` → DI: `Antivirus:Provider=ClamAV` registers the ClamAV client + HttpClient; `NoOp` registers the warning-only dev fallback (refuses in non-dev).

### 2026-09-13 — Wave 4 (UX polish)

- `[4.1]` `Store.UI/wwwroot/js/toast-bus.js` (new) → `ToastBus.publish(channel, level, msg, opts)` API. Channels: `app | auth | pos | inventory | finance | admin`. Backward-compatible with existing `window.showToast`.

### 2026-09-13 — Wave 6 (Documentation)

- `[6.1]` `AGENTS.md` (new) → 12-section onboarding for any agent/human touching the repo
- `[6.4]` `docs/security_runbook.md` (new) → 8-section operator playbook for incidents

### 2026-09-14 — Wave 11 (Error surface hardening + structured codes)

**Goal** — Three asks in one wave, all done completely, with dev environment showing
full diagnostic detail while prod stays opaque. The original SafeErrorMessage patch
attempt damaged 41 page-model constructors; the cleanest repair was `git checkout` to
HEAD for those files and re-attempt with a strict text-substitution pipeline.

- `[11.1]` `Store.Models/DTOs/Common/ErrorCode.cs` (new) → canonical error-code taxonomy
  (HTTP-status-family grouping: 4xx validation/notfound/conflict/unauthorized/forbidden,
  5xx external/internal). Includes the `CategoryFor(code)` helper that returns one of
  `validation | notfound | unauthorized | forbidden | conflict | ratelimited | external | internal`
  for UI category-aware toasts.
- `[11.1]` `Store.UI/Services/SafeErrorMessage.cs` (new) → env-aware static helper:
  - **Dev** (`ASPNETCORE_ENVIRONMENT=Development`): returns
    `"<ExceptionType>: <ex.Message>"` so developers see the full diagnostic.
  - **Prod**: returns just `"<ExceptionType>"`. Full exception is logged server-side via
    `ILogger.LogError` with the structured operation context. Schema names, SQL fragments,
    and any future PII in the message never leak to the UI.
  - Usage: `StatusMessage = SafeErrorMessage.From(ex, _logger, "Operation Name")` —
    pass `NullLogger<TModel>.Instance` from page-models that don't yet have a logger.
- `[11.1]` **Patch pipeline** — 28 `.cshtml.cs` files now route their `ex.Message` surface
  through `SafeErrorMessage.From(...)`:
  - `ErrorMessage = ex.Message` → `ErrorMessage = SafeErrorMessage.From(ex, _logger, "Op")`
  - `{ex.Message}` inside interpolation → `{SafeErrorMessage.From(ex, _logger, "Op")}`
  - `error = ex.Message` / `message = ex.Message` inside `StatusCode`/`BadRequest`/`JsonResult`
    return paths → `SafeErrorMessage.From(...)`.
  - For files without a `_logger` field, `NullLogger<TModel>.Instance` is used (logging
    benefit restored later when the field is injected).
  - Pure text substitution — no constructor injection, no field manipulation — so the
    pipeline cannot corrupt constructors like the previous attempt did.
- `[11.1]` `Store.UI/Pages/Catalog.cshtml.cs`, `Customers.cshtml.cs` → re-applied
  Wave 7 soft-delete signature changes (`DeleteAsync(itemId, null, ct)` /
  `DeleteAsync(customerId, null, ct)`) after the page-model reset.
- `[11.1]` `Store.UI/Pages/RestockRecommendations.cshtml.cs` → Wave 8 page-model rewritten
  from scratch (constructor, properties `Summary`/`LastBulkResult`/`StatusMessage`/`StatusIsError`,
  `OnPostBulkOrderCriticalAsync`, TempData round-trip). The original was lost in the
  page-model reset; this restores it. Builds clean.
- `[11.2]` **PII audit complete** — `grep ex\.Message Store.UI/Pages` returns zero hits.
  Every exception message produced for a UI consumer now goes through `SafeErrorMessage.From`,
  so dev sees the full text and prod sees only the exception type. No further action.
- `[11.3]` **Structured error codes** — 15 API controllers now use `ErrorCode.*`
  constants in their `ApiErrorResponse.From(...)` calls:
  - `AdminRoleMatrixController.cs`, `AuditLogsController.cs`, `AuthController.cs`,
    `BatchesController.cs`, `CashVarianceController.cs`, `DiscountOverridesController.cs`,
    `DiscountsController.cs`, `LoyaltyCampaignsController.cs`, `LoyaltyController.cs`,
    `PurchaseOrdersController.cs`, `StockTransfersController.cs`, `SuppliersController.cs`,
    `SystemSettingsController.cs`, `UsersController.cs`, `WastageController.cs`.
  - Added `InvalidCredentials`, `InvalidTwoFactorCode`, `InvalidRefreshToken` constants
    for the auth flow; extended `CategoryFor` to map them to `"unauthorized"`.
  - Wire format is unchanged (`ErrorCode.NotFound` → `"not_found"` JSON). The benefit is
    compile-time safety and a single source of truth — typos are caught at build, and
    UI code can switch on `ErrorCode.*` constants rather than fragile strings.
- `[11.4]` **HttpClientFactory log spam fix** — `appsettings.json` for all four projects
  (Store.API, Store.UI, Store.ControlPlane, Store.TenantPortal) now silence
  `Microsoft.Extensions.Http.DefaultHttpClientFactory` to `Warning`. The
  HttpMessageHandler cleanup cycle was emitting a Debug log every 10s per
  `IHttpClientFactory`-registered client (ClamAV scanner in particular), drowning the
  actual application logs. Dev environment (`appsettings.Development.json`) keeps
  `Microsoft.Extensions.Http.DefaultHttpClientFactory: Information` so startup events
  are visible without the cleanup-cycle noise.
- `[11.5]` **Final build** — `dotnet build StoreProject.sln --nologo` → 0 warnings, 0 errors.
  `dotnet test Store.API.Tests --nologo` → Passed: 64, Failed: 0, Skipped: 0.

### 2026-09-14 — Wave 11 follow-up (API + ControlPlane + TenantPortal PII coverage)

- `[11.6]` `Store.Models/Common/SafeErrorMessage.cs` (new, consolidated) → moved out of
  `Store.UI.Services` and `Store.API.Common` into `Store.Models.Common` so all four
  projects (Store.API, Store.UI, Store.ControlPlane, Store.TenantPortal) share one
  source of truth. Old in-project duplicates replaced with stub comments so the
  namespace lookup doesn't accidentally resolve to a stale copy.
- `[11.7]` `Store.TenantPortal/Store.TenantPortal.csproj` → added `<ProjectReference>` to
  `Store.Models` (was missing — only referenced `Microsoft.Extensions.Http.Polly`).
  Without it, the TenantPortal couldn't resolve `Store.Models.Common.SafeErrorMessage`.
- `[11.8]` **API-side ex.Message coverage** — 7 API controllers + the central
  `ExceptionHandlingMiddleware` now route every user-facing exception through
  `SafeErrorMessage.From(ex, _logger, "<op>")`. The middleware (the central gateway for
  all unhandled errors) was rewritten to use `ErrorCode.*` constants and the helper.
  Controllers updated: `UsersController.cs`, `LoyaltyController.cs`, `LookupControllers.cs`,
  `InvoicesController.cs`, `PayrollController.cs`, `BranchController.cs`,
  `WebAuthnController.cs`. `LookupControllers.cs` and `InvoicesController.cs` were
  reverted to HEAD and patched with text-substitution only (they use single-line
  expression-bodied constructors that can't be safely extended by the script).
- `[11.9]` **ControlPlane coverage** — 6 controllers and 4 TenantPortal pages +
  `Program.cs` updated. Each ControlPlane controller got `_logger` injected via
  constructor (where the constructor was multi-line) or `NullLogger<T>.Instance`
  (for single-line expression-bodied ctors). `TenantPortal` page-models and
  `Program.cs` use `NullLogger` since they're Razor Pages where constructor
  injection isn't viable without rewriting.
- `[11.10]` `Store.API.Tests/LoyaltyControllerTests.cs` → updated `new LoyaltyController(...)`
  to pass `NullLogger<LoyaltyController>.Instance` for the new constructor parameter.
- `[11.11]` **Final PII audit** — `grep ex\.Message` across all production source
  returns only:
  - `Store.Models/Common/SafeErrorMessage.cs` (intentional — it's the helper).
  - Internal DbServices (`ClamAvVirusScanner`, `SystemSettingService`,
    `NotificationService`, `OfflineLogSyncWorker`) — these store `ex.Message` in
    internal `Result.Failed(message)` / DB columns, never returned to the browser
    directly. Their caller controllers use `SafeErrorMessage.From` for user-facing
    responses.
  - ControlPlane `TenantOrchestrator.LogStep` and `DomainVerificationService.Message` —
    provisioning-log entries and DNS verification status, surfaced only via the
    internal admin audit log.
  - `Store.API.Tests/TenantOrchestratorTests.cs` — `Assert.Contains(..., ex.Message)`
    assertions on intentionally-thrown test exceptions.
  - `IMPLEMENTATION_LOG.md` and `docs/` — documentation references to the OLD pattern.
  No user-facing response in any controller, page-model, or middleware returns
  raw `ex.Message` anymore. Schema names, SQL fragments, file paths, and any future
  PII in the exception message can no longer leak to the browser.

---

## Wave 12 — End-to-end security & architectural hardening

This is the **end-to-end** wave: complete features (no half-baked), strict security hardening, RBAC + design consistency, Dennis Ritchie / Uncle Bob Clean Architecture, micro-interactions preserved. Every entry below landed in code, builds green, and (where applicable) has tests passing.

### 12.1 Security — SEC-04 (FIDO2 origins), SEC-06 (HMAC OTP), SEC-11 (constant-time avatar), SEC-14 (refresh-token rotation), SEC-15 (MoMo idempotency), SEC-18 (CDN SRI)

**SEC-04** — FIDO2 localhost origins already gated by `IsDevelopment()` in `Store.API/Program.cs:66-70` (Wave 1). Verified HEAD; no change required.

**SEC-06** — HMAC OTP at rest.
- `Store.Models/Entities/Otp.cs` — `Code` column removed; new `CodeHash : string` (max length 64).
- `Store.DbServices/Configurations/MiscConfiguration.cs:OtpConfiguration` — `CodeHash` configured + composite index `IX_Otp_User_Purpose_Used_ExpiresAt`.
- `Store.DbServices/Services/PasswordRecoveryService.cs` — `HashOtpCode(rawCode)` keys HMAC-SHA256 with `Auth:OtpPepper`; verify via `CryptographicOperations.FixedTimeEquals`. Temp password generation switched from `Guid.NewGuid().ToString("N")` to `RandomNumberGenerator.GetString` over a 12-char alphabet (no `0/O/1/l` confusion). Reset token issued via `Convert.ToHexString(RandomNumberGenerator.GetBytes(32))`.
- `Store.DbServices/Services/OrderAndOtpService.cs:OtpService` — refactored alongside `PasswordRecoveryService` to use the same HMAC + fixed-time path.
- `Store.Models/Common/SafeErrorMessage.cs` — new consolidated helper (replaces the UI/API duplicates from Wave 11).
- `Store.DbServices/Extensions/ServiceCollectionExtensions.cs` — `OtpPepperOptions` bound + `ValidateOnStart()` with min-32-byte requirement.
- `Store.DbServices/Migrations/20260915120000_OtpHashAtRest_SEC06.cs` (and `.Designer.cs`) — drops `code`, adds `code_hash`, creates composite index. Pre-existing OTPs are purged in `Up` (plaintext codes can't be hashed after the fact; 15-min expiry means very few users affected).

**SEC-11** — Constant-time avatar endpoint.
- `Store.API/Controllers/AuthController.cs:GetAvatar` — Stopwatch + `WaitConstantAsync(sw, TargetLatencyMs, ct)` enforces 80 ms minimum wall-clock on every code path (hit, miss, error, malformed). The endpoint stays `[AllowAnonymous]` because the login page needs to display avatars before login; the constant-time is the substitute for auth-required.

**SEC-14** — Refresh-token rotation race fix.
- `Store.DbServices/Services/AuthenticationService.cs:RefreshTokenAsync` — replaced EF-tracked `Update + SaveChanges` with `ExecuteUpdateAsync(... WHERE Token = @old AND IsRevoked = false)`. The race loser observes `rowsAffected == 0` and returns `null` (caller re-authenticates). AuditLoggingMiddleware logs the resulting 401 with the traceId so operators can detect a flood.

**SEC-15** — Mobile-money callback idempotency.
- `Store.Models/Entities/MobileMoneyTransaction.cs` already had `ProviderTransactionId`; new UNIQUE filtered index enforces dedup at the DB layer.
- `Store.DbServices/Context/StoreDbContext.cs:OnModelCreating` — `HasIndex(x => x.ProviderTransactionId).IsUnique().HasFilter("provider_transaction_id IS NOT NULL AND provider_transaction_id <> ''")`.
- `Store.DbServices/Migrations/20260916110000_MobileMoneyCallbackIdempotency_SEC15.cs` (and `.Designer.cs`) — pre-existing duplicate rows are revoked (`status = Failed`) before the index is created, so deploys on a tenant with legacy dupes still succeed.

**SEC-18** — CDN script SRI.
- `Store.UI/Pages/_Host.cshtml` — Bootstrap 5.3.0 CSS + JS now loaded with `integrity="sha384-..."` + `crossorigin="anonymous"` + `referrerpolicy="no-referrer"`. SHA-384 hashes computed from the upstream CDN files at Wave 12 prep time. The `browser-image-compression` and `microsoft-signalr` CDN tags in `_AppLayout.cshtml` already had SRI from earlier waves (verified, no change).

### 12.2 Architecture — GAP-15 (API versioning), GAP-16 (ETag middleware)

**GAP-15** — API versioning (non-breaking foundation).
- `Store.API/Store.API.csproj` — added `Microsoft.AspNetCore.Mvc.Versioning 5.1.0`.
- `Store.API/Program.cs` — `AddApiVersioning(...)` with `DefaultApiVersion = 1.0`, `AssumeDefaultVersionWhenUnspecified = true`, `ReportApiVersions = true`, and a combined query + header reader. Every response now carries `X-Api-Version: 1.0`. No controller route changes; future v2 endpoints can opt in with `[ApiVersion("2.0")]`.

**GAP-16** — ETag middleware for catalog GETs.
- `Store.API/Middleware/ETagMiddleware.cs` (new) — buffers the response body, computes SHA-256, emits a weak ETag (`W/"hex"`), and short-circuits to 304 if `If-None-Match` matches. Cache-Control set to `private, max-age=0, must-revalidate` (correct for multi-tenant SaaS where prices change frequently). Skipped for non-GET/HEAD and for controllers that already emit their own ETag.
- `Store.API/Program.cs` — `app.UseETag()` registered after `UseAuthorization`, before `MapControllers`.

### 12.3 Half-baked features — GAP-04 (POS ForcePasswordChange), GAP-09 (CashVariance dates), GAP-19 (DB lookup indexes)

**GAP-04** — POS now honors `force_password_change`.
- `Store.DbServices/Services/AuthenticationService.cs` — added `force_password_change=true` JWT claim whenever `user.Password.ForcePasswordChange` is true.
- `Store.UI/Services/SessionClaims.cs` (new) — base64url-claim reader that doesn't pull in `System.IdentityModel.Tokens.Jwt`. Exposes `IsForcePasswordChangeRequired(ISession)` and `Get(ISession, claimType)`.
- `Store.UI/Pages/Pos.cshtml.cs:OnGetAsync` — uses `SessionClaims.IsForcePasswordChangeRequired(HttpContext.Session)` to short-circuit to `/ForceResetPassword`.

**GAP-09** — CashVarianceController date range.
- Already had `fromDate`/`toDate` parameters on `GetAll`. Now also plumbed through `ExportCsv` so a year-end CSV export doesn't dump the entire history.

**GAP-19** — DB lookup indexes.
- `Store.DbServices/Configurations/MiscConfiguration.cs:SupplierConfiguration` — UNIQUE filtered index `ix_supplier_registration_number`.
- `Store.DbServices/Configurations/ItemConfiguration.cs:BatchConfiguration` — UNIQUE index `ix_batch_batch_number`.
- `Store.DbServices/Migrations/20260916103000_LookupIndexes_GAP19.cs` (and `.Designer.cs`) — creates both indexes. `CashierShift` already had its composite indexes from Wave 1.

### 12.4 Operational — OPS-02 (deep healthcheck), OPS-03 (Traefik dashboard gate)

**OPS-02** — `/health` now actually tests MySQL.
- `Store.API/Store.API.csproj` — added `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.4`.
- `Store.API/Program.cs` — `AddHealthChecks().AddDbContextCheck<StoreDbContext>("mysql", tags: new[] { "ready" })`. The docker-compose `curl /health` will now fail readiness when MySQL is unreachable, so Traefik stops routing traffic to the broken instance.
- `docker-compose.prod.yml` already had MySQL, MongoDB, API, and UI healthchecks wired; verified HEAD.

**OPS-03** — Traefik dashboard is no longer exposed by default.
- `docker-compose.traefik.yml` — added a top-of-file warning banner + replaced hard-coded `--api.insecure=true --api.dashboard=true` with `--api.insecure=${TRAEFIK_API_ENABLED:-false}` + `--api.dashboard=${TRAEFIK_API_ENABLED:-false}`. Production deployments leave the env unset and never expose the dashboard.
- `.env.example` (new) — documents `TRAEFIK_API_ENABLED=false` and `TRAEFIK_DASHBOARD_PORT=8080`. Production should never set this to true.

### 12.5 UX polish — UX-03 (toast bus), UX-07 (aria-live)

**UX-03 / UX-07** — `Store.UI/wwwroot/js/toast-bus.js` from Wave 4 already provides:
- `ToastBus.publish(channel, level, message, opts)` canonical API with channel-aware containers.
- Per-container `aria-live="polite"` + `aria-atomic="false"` for screen readers.
- Auto-surfaces server-rendered TempData banners.
- Backward-compat: existing `window.showToast(type, msg)` calls still work via the legacy bridge.

No code changes needed; verified HEAD.

### 12.6 Process hygiene — PROC-03 (CODEOWNERS), PROC-04 (PR template)

**PROC-03** — `.github/CODEOWNERS` (new) — directory-based ownership:
- Default rule + team-specific (api-team, web-team, tenant-portal-team, platform-team).
- Security-sensitive files require `@store-lead` + `@security` review (auth, payments, webauthn, mobile money, SafeErrorMessage, ErrorCode).
- Migrations require `@store-lead` + `@api-team` (these run on every deploy).

**PROC-04** — `.github/pull_request_template.md` (new) — checklist:
- Type of change, tracker ID, migration impact, env-var impact.
- Security checklist (no `ex.Message` leak, no committed secret, ErrorCode usage, no `Task.Delay` outside SafeErrorMessage path).
- Build + test confirmation, smoke test scenarios, doc updates (IMPLEMENTATION_LOG + audit-tracker + AGENTS.md).
- Operator rollout notes.

### 12.7 Verification — Build + tests + migrations

- `dotnet build StoreProject.sln --nologo` → **0 warnings, 0 errors**.
- `dotnet test Store.API.Tests --nologo` → **64 passed, 0 failed, 0 skipped**.
- Three EF migrations added in this wave: `OtpHashAtRest_SEC06`, `LookupIndexes_GAP19`, `MobileMoneyCallbackIdempotency_SEC15`. All reversible via `Down()` for safe rollback.

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

---

## Wave 13 — Multi-tenant production readiness

Goal: close every remaining gap that stops StoreProject from running as a true
multi-tenant SaaS on a single Traefik front, with anonymous public surfaces
(status / maintenance) and audited, tenant-scoped audit routing.

Scope selected over cookie-JWT-only so SEC-19 lands alongside MT-01/03/05/07
on the same wave, keeping multi-tenant foundations coherent.

### 13.A — SEC-19: HttpOnly Secure SameSite=Strict cookie JWT
- `Store.API/Auth/AuthCookieHelper.cs` (new) — `SetAuthCookies(secureContext)` /
  `ClearAuthCookies()` extension methods on `HttpResponse`. Cookie names:
  `store_at` (access token) + `store_rt` (refresh token). Secure flag is
  driven by `IsSecureContext` (X-Forwarded-Proto aware so it works behind
  Traefik).
- Wired into `AuthController` — every login path (password + 2FA + recovery +
  initial), `RefreshTokenAsync`, and `LogoutAsync` now sets/clears cookies.
- JWT `OnMessageReceived` middleware now reads the `store_at` cookie when the
  `Authorization` header is missing — clients that don't ship a header
  (server-rendered Razor pages, SSR fetch helpers) still authenticate.
- LocalStorage fallback kept intact for clients that explicitly want a Bearer
  header (e.g. Postman, the Scanner SPA).

### 13.B — MT-01: async provisioning + status polling UI
- `Store.ControlPlane/Models/TenantProvisioningJob.cs` (new) — entity
  tracking job lifecycle with `AdminPasswordCipher` (AES-GCM at rest, decrypted
  in-memory only when the worker runs the pipeline).
- `Store.ControlPlane/Services/TenantProvisioningHostedService.cs` (new) —
  `BackgroundService` polls every 3s, atomically claims pending jobs via
  `ExecuteUpdateAsync(... WHERE Status = Pending)`, runs the full
  `ProvisionTenantAsync` pipeline, and on success links the tenant → user
  account via `PortalAuthService.CreateAccountForAsync`. 3-attempt retry with
  exponential backoff before giving up.
- `Store.ControlPlane/Controllers/TenantsController.cs` — three new
  endpoints: `POST /api/control/tenants/provision-async`,
  `GET /api/control/tenants/provisioning/{jobId:guid}` (for the polling UI),
  `POST /api/control/tenants/provisioning/{jobId:guid}/retry` (operator can
  force retry after manual fix).
- DB migration `20260916133000_AsyncProvisioningJobs_MT01` + Designer +
  snapshot update.
- `Store.TenantPortal/Services/ControlPlaneClient.cs` — 3 new MT-01 methods.
- `Store.TenantPortal/Pages/Onboarding.cshtml.cs` — switched from sync
  `ProvisionTenantAsync` to async `ProvisionTenantAsyncJobAsync`; sets
  `TempData["ProvisioningJobId"]` + `["ProvisioningSlug"]`; redirects to
  `/OnboardingStatus`.
- `Store.TenantPortal/Pages/OnboardingStatus.cshtml(.cs)` (new) — status
  pill + ARIA-live + JS auto-poll every 3s + indeterminate progress bar +
  retry button on failure.

### 13.C — MT-03: verified already in place
- `DomainVerificationService.VerifyTxtRecordAsync` + `TraefikConfigWriter.
  WriteTenantRoutingConfigAsync` + `TenantOrchestrator.VerifyCustomDomainAsync`
  already write Traefik dynamic config on DNS match + audit entry.
- `Domains.cshtml` shows TXT-record hint with copy-to-clipboard buttons.
- Closed in tracker without code changes.

### 13.D — MT-05: tenant-aware audit routing
- `Store.Models/Entities/AuditLog.cs` — added `TenantId : Guid?` with
  docstring documenting the tenant-isolation contract (NULL for
  system-level / pre-provisioning audits).
- `Store.DbServices/Context/StoreDbContext.cs` — composite index
  `ix_audit_log_tenant_date` on `(TenantId, DateCreated)`.
- DB migration `20260916150000_AuditLogTenantId_MT05` + Designer +
  snapshot update (new column + new index).
- `Store.Models/DTOs/Audit/AuditLogDtos.cs` — `AuditLogDto.TenantId`,
  `AuditLogFilterRequest.TenantId` (read filter), `CreateAuditLogEntryRequest.
  TenantId` (write field).
- `Store.Models/Interfaces/Services/IAuditLogService.cs` — `GetMetricsAsync`
  gains `Guid? tenantId` parameter.
- `Store.DbServices/Services/AuditLogService.cs` — `LogAsync` writes
  `TenantId`, `GetAuditLogsPagedAsync` and `GetMetricsAsync` honor
  `request.TenantId` / parameter. NULL preserved for system audits.
- `Store.API/Controllers/AuditLogsController.cs` — `GET /metrics` accepts
  `?tenantId=`. Razor UI updated (`AuditLog.cshtml.cs` calls the new
  signature).
- `Store.API/Middleware/AuditLoggingMiddleware.cs` — resolves tenant from
  JWT `tenant` claim OR `X-Tenant-Id` header (X-Tenant-Id used by Traefik
  forwardAuth or by the Razor UI when calling tenant-scoped endpoints).
- `Store.UI/Services/ApiAuditLogService.cs` + `IAuditLogManager` + UI page
  all updated to the new signatures.
- Build green, 64/64 tests pass.

### 13.E — MT-07: tenant status / maintenance page (public-facing)
- `Store.ControlPlane/Models/TenantMaintenanceWindow.cs` (new) —
  `MaintenanceWindow` entity with title, description, StartUtc/EndUtc,
  Severity (Info / Warning / Critical), IsResolved, createdBy, timestamp.
- `Store.ControlPlane/Models/Tenant.cs` — added
  `List<MaintenanceWindow> MaintenanceWindows`.
- `Store.ControlPlane/Data/ControlPlaneDbContext.cs` — JSON-converted column
  on `tenants.maintenance_windows` (longtext).
- DB migration `20260916170000_TenantMaintenanceWindows_MT07` + Designer +
  snapshot update.
- `Store.ControlPlane/Models/DTOs/TenantDtos.cs` — `MaintenanceWindowDto`,
  `TenantStatusDto` (public payload), `CreateMaintenanceWindowRequest`.
- `Store.ControlPlane/Services/ITenantOrchestrator.cs` + `TenantOrchestrator.cs`
  — `GetPublicStatusAsync` (anonymous), `AddMaintenanceWindowAsync` (operator,
  validates `EndUtc > StartUtc`, writes audit-trail entry), `RemoveMaintenance
  WindowAsync`, `ResolveMaintenanceWindowAsync`. Public payload is sanitized —
  never leaks secrets, internal IDs, or operator notes.
- `Store.ControlPlane/Controllers/TenantsController.cs` — 3 new operator
  endpoints.
- `Store.ControlPlane/Controllers/PublicStatusController.cs` (new) — anonymous
  controller at `/api/public/tenants/{slug}/status` with 30s `ResponseCache`.
- `Store.TenantPortal/Services/ControlPlaneClient.cs` + interface + DTO
  records — public + CRUD wiring.
- `Store.TenantPortal/Pages/Status.cshtml(.cs)` (new) — `/Status/{slug}` Razor
  page with `[AllowAnonymous]`, severity badges, ARIA-labelled sections,
  maintenance-in-progress banner, footer with generation timestamp.

### 13.F — PROC-11: CI matrix
- `.github/workflows/ci-cd.yml` — refactored into:
  - `secret-scan` (unchanged)
  - `build-matrix` (NEW) — 6-leg parallel matrix:
    `Store.Models`, `Store.DbServices`, `Store.API`, `Store.UI`,
    `Store.ControlPlane`, `Store.TenantPortal`. Each leg restores + builds
    only its own csproj with project-scoped cache key.
  - `build-and-test` (existing) — now waits for the matrix, then runs the
    full solution-wide build + tests + coverage.
  - `docker-build-and-push` — `needs: [build-matrix, build-and-test]`.
- Fixed pre-existing YAML indent bug in `deploy-production` heredoc body
  (was at column 1, broke YAML parser — the `<<ENV` lines are now indented
  correctly so YAML treats the whole `run: |` block as one scalar).
- YAML validated via Python `yaml.load`.

### 13.G — docs + verification
- `docs/audit-tracker.md` — SEC-19, MT-01, MT-03, MT-05, MT-07, PROC-11
  flipped to `[x]` with implementation notes.
- `IMPLEMENTATION_LOG.md` — this entry.
- Final build green: 0 warnings, 0 errors.
- Final tests: 64/64 passing.

### Build & test verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:01:13.91

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 64, Skipped: 0, Total: 64, Duration: 2 s
```

---

## Wave 14 — Tenant-aware backup automation (MT-06)

Goal: turn the per-tenant `BackupScheduleConfig` into a real automated
scheduler that fires `TriggerBackupNowAsync` on each tenant's configured
cadence. Until this wave, every backup was manual — operators had to call
the API by hand. Now the ControlPlane runs it on its own.

### 14.A — Schedule model + cron evaluator
- `Store.ControlPlane/Models/Tenant.cs` — added `BackupScheduleConfig.LastRunAt`
  and `LastRunStatus` (UTC timestamp + outcome string). Stored as part of the
  existing JSON-serialized `BackupSchedule` column on `tenants` — no schema
  migration required.
- `Store.ControlPlane/Services/BackupScheduleEvaluator.cs` (new) — pure
  static class with two methods:
  - `ShouldRunNow(schedule, nowUtc)` — returns true when the schedule is
    enabled, frequency is not Manual, and `NextRunAt <= now`.
  - `ComputeNextRunAt(schedule, lastRunAtUtc, nowUtc)` — for Hourly:
    anchor + 1h. For Daily/Weekly: snap to 02:00 UTC, skip past both anchor
    and now. Returns `DateTime.MaxValue` for Manual.
- 14 unit tests in `Store.API.Tests/BackupScheduleEvaluatorTests.cs` cover:
  manual never-fires, disabled never-fires, hourly past/future, daily
  same-day 02:00 snap when last was later in the day, weekly 7-day math,
  never-schedule-in-the-past guarantee, first-run-without-last, sentinel
  for Manual.

### 14.B — Hosted service + retention + UI
- `Store.ControlPlane/Workers/TenantBackupHostedService.cs` (new) —
  `BackgroundService` polling every 30s. Creates a DI scope per tick,
  evaluates every active tenant, fires `IBackupService.TriggerBackupNowAsync`
  for each due schedule, persists `LastRunAt` / `LastRunStatus` /
  `NextRunAt`, and appends a `ProvisioningLog` entry (so operators see the
  schedule firing in the audit trail). Failures are caught + persisted so
  one tenant's problem never kills the worker.
- `Store.ControlPlane/Program.cs` — registered as the third hosted
  service alongside `TenantHealthMonitorWorker` and
  `TenantProvisioningHostedService`.
- `Store.ControlPlane/Services/BackupService.cs` — `GetBackupSummaryAsync`
  and `UpdateScheduleAsync` now populate `BackupScheduleDto.LastRunAt` /
  `LastRunStatus`. `UpdateScheduleAsync` preserves the existing
  `LastRunAt` / `LastRunStatus` when operators change frequency or
  retention — without this, every schedule edit would silently wipe the
  audit trail of past runs.
- `Store.ControlPlane/Models/DTOs/BackupDtos.cs` + `Store.TenantPortal/
  Models/DTOs/ControlPlaneDtos.cs` — `BackupScheduleDto` extended with
  `LastRunAt` + `LastRunStatus`.
- `Store.TenantPortal/Pages/Backups.cshtml` — new MT-06 status card
  showing **Next Run**, **Last Run**, **Last Status**, and **Schedule**
  pills above the existing schedule form. Failed statuses render in red,
  successful in green, missing in muted gray.
- Retention cleanup was already inline in `TriggerBackupNowAsync` (the
  schedule's `RetentionCount` prunes the JSON `BackupHistory` list after
  each run). Verified — no separate worker needed.

### 14.C — OPS-09 deferred
- The audit-tracker entry `OPS-09` references `scripts/provision-docker-vps.ps1`,
  which does not exist in the repo. The existing `provision-tenant.ps1` is
  an HTTP client wrapper (no root needed). Creating a real VPS bootstrap
  script is its own project — deferred to a later wave. OPS-12 is the
  same item as MT-06 (per-tenant scheduled backup) and is closed here.

### Build & test verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 78, Skipped: 0, Total: 78, Duration: 1 s
```

---

## Wave 15 — Auth hardening (SEC-26 / SEC-27 / SEC-28)

Goal: tighten the authentication surface against credential-stuffing and
XSS-driven token exfiltration. Three of the four items I planned landed in
this wave — SEC-23 (WebAuthn enrolled-device binding) is its own project
(device fingerprinting + per-device row + IP-change reauth middleware)
and is deferred to a dedicated wave.

### 15.A — SEC-26 server-side password complexity
- `Store.Models/DTOs/Auth/PasswordPolicyOptions.cs` (new) — config-driven
  policy: `MinLength=12`, `MaxLength=128`, `MinClassCount=3` (upper / lower
  / digit / symbol), `CommonPasswords` (90-entry embedded blocklist seeded
  by default), `EnableBreachCheck` (off by default). `WithSecureDefaults()`
  ensures a missing config block still produces a hard policy.
- `PasswordComplexityAttribute` (ValidationAttribute) — runs length + class
  + blocklist (case-insensitive) checks; resolves `IBreachChecker` and the
  policy holder via `ValidationContext.GetService` so DI is preserved.
  Fail-open when the holder is unbound so a missing config never breaks
  the pipeline.
- Applied to `CreateUserRequest.Password` and
  `ChangePasswordRequest.NewPassword`. `StringLength(MinimumLength = 12)`
  stays as a hard ceiling so misconfigured policies can't widen the door.
- `Store.API.Tests/PasswordComplexityTests.cs` — 13 unit tests covering
  null/empty passthrough, length boundaries, class-count edge cases,
  blocklist match (lowercase / mixed / uppercase), and the fail-open
  contract. All use a stub `IServiceProvider` so no HIBP network call is
  required during CI.

### 15.B — SEC-27 HIBP k-anonymity breach checker
- `Store.Models/Interfaces/Services/IBreachChecker.cs` (new) — interface
  with `IsBreachedAsync(password, ct)` returning `bool`. Documents the
  k-anonymity contract.
- `Store.DbServices/Services/PwnedPasswordsBreachChecker.cs` (new) —
  SHA-1 hashes the password, sends only the first 5 hex chars to
  `https://api.pwnedpasswords.com/range/{first5}`, parses the response,
  matches the remaining 35 chars. Adds `Add-Padding: true` per HIBP ToS.
  3-second timeout via `CancellationTokenSource.CreateLinkedTokenSource`.
  Fails open on every error path (network / non-2xx / malformed / timeout)
  — breach check is defense in depth, not the sole gate.
- Registered as a typed `HttpClient<IBreachChecker>` in `Program.cs` so
  the framework owns DNS rotation and connection pooling. Timeout enforced
  at the HttpClient level (3s) and re-checked in the implementation.

### 15.C — SEC-28 refresh-token-in-cookie-only
- `Store.Models/DTOs/Auth/AuthRequests.cs` — `RefreshTokenRequest.Token`
  and `RefreshToken` are now both nullable. `Token` is still useful for
  audit / user identification; `RefreshToken` is now optional in the body.
- `Store.API/Controllers/AuthController.cs` — `Refresh` reads the `store_rt`
  HttpOnly cookie as the primary source. Falls back to body only when the
  cookie is missing (CLI / Postman / mobile). Returns a clear 401 with
  guidance when both are absent. The `SetAuthCookies(...)` rotation
  (SEC-19) still runs on every successful refresh so the cookie value is
  updated in flight.
- `Store.DbServices/Services/AuthenticationService.cs` — `RefreshTokenAsync`
  null-checks the body fields at the top and returns 401 cleanly when the
  refresh token is empty.

### 15.D — SEC-23 deferred
- Enrolled-device binding needs:
  - new `WebAuthnDevice` entity + EF migration
  - per-device last-IP + last-used-at tracking
  - reauth middleware that compares the request IP to the last-seen IP and
    forces a WebAuthn assertion on mismatch
  - rate limiter for WebAuthn assertion attempts
- This is its own multi-day wave. Deferred rather than crammed in here.

### Build & test verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 100, Skipped: 0, Total: 100, Duration: 2 s
```

---

## Wave 16 — UX consistency pass (UX-06 / UX-05 / GAP-23 / UX-04 / UX-01)

Goal: per-page polish to make the operator UI feel coherent and tablet-ready.
Five sub-items shipped in one wave — each was small enough that splitting
would have been paperwork.

### 16.A — UX-06 reusable empty-state partial
- `Store.UI/Pages/Shared/_EmptyState.cshtml` (new) + `EmptyStateModel.cs` —
  strongly-typed DTO with `Icon / Title / Message / CtaText / CtaHref /
  SecondaryText / SecondaryHref / Variant`. The partial renders a
  border-dashed card with an icon + headline + body + primary CTA + (opt)
  secondary link. Variants: `info` / `warning` / `muted`.
- Applied to **Catalog** (filter-miss vs cold-start), **Suppliers**,
  **Customers**, **Employees**, **Dashboard** (recent invoices). Each page
  distinguishes cold-start ("No suppliers yet — add your first") from
  filter-miss ("No suppliers match these filters — clear filters") so the
  CTA stays meaningful.
- Pattern: declared `var empty = new EmptyStateModel { ... }` then
  `<partial name="_EmptyState" model="empty" />`. Anonymous object
  initializers don't survive Razor's parser cleanly, so the strongly-typed
  model is the contract going forward.

### 16.B — UX-05 permission-gated affordances
- `Store.UI/Pages/Wastage.cshtml.cs` — added `CanRecord` (InventoryWrite)
  and `CanDelete` (AdminUsers) properties populated in `OnGetAsync`. Used
  `out var permissions` instead of `out _` so the permissions reach the
  flag setters.
- `Store.UI/Pages/Wastage.cshtml` — Record Wastage button gated on
  `Model.CanRecord`; row-level Delete button gated on `Model.CanDelete`.
  Server-side `[Authorize]` policies remain in force; the UI gating is a
  friendliness layer (less confusing buttons = fewer support tickets).
- Most other pages already had gating (`InventoryOps.CanWrite` etc.).
  Remaining per-page audit is tracked in the audit tracker as `UX-05 [~]`.

### 16.C — GAP-23 / UX-02 mobile-first responsive
- `Store.UI/wwwroot/css/site.css` — added `@media (max-width: 900px)` block:
  sidebar collapses into a fixed drawer with `translateX(-100%)` and
  `.is-open` toggle, backdrop overlay, `.mobile-sidebar-toggle` button
  shown only on mobile, `.app-main` and `.app-content` lose their left
  margin and padding shrinks. Touch-target sizing bumped to 36px min on
  `.btn-*` classes. `.table-wrap` gets horizontal scroll on narrow
  viewports.
- `Store.UI/Pages/Shared/_AppLayout.cshtml` — added the hamburger toggle
  button + backdrop overlay at the top of `<body>`. The toggle is a 40×40
  button with `aria-controls`/`aria-expanded` for screen readers.
- `Store.UI/wwwroot/js/site.js` — `window.toggleMobileSidebar(force?)` plus
  Escape-to-close listener. Clicking the backdrop also closes.

### 16.D — UX-04 keyboard shortcut help
- `Store.UI/Pages/Shared/_KeyboardHelp.cshtml` (new) — dialog with full
  shortcut table (Ctrl+K, ?, Esc, G→D, G→P, G→C, G→M, G→I, G→S, G→O).
  ARIA roles (`role="dialog"`, `aria-modal`, `aria-labelledby`).
- Wired in `_AppLayout.cshtml` after `_CommandPalette`. Backdrop click +
  Escape both close.
- `Store.UI/wwwroot/js/site.js` — added `openKbdHelp` / `closeKbdHelp` and
  a keydown listener that fires on `?` (with input-target suppression so
  typing in a text field doesn't open the dialog).

### 16.E — UX-01 PWA service worker
- `Store.UI/wwwroot/sw.js` (new) — offline shell + stale-while-revalidate
  for static assets + network-first for `/api/*` + cache-then-network for
  HTML navigations with `/Pos` as the offline fallback. `CACHE_VERSION`
  baked in so a `skipWaiting()` invalidates old entries.
- `Store.UI/wwwroot/js/pwa-install.js` — registers `navigator.serviceWorker
  .register('/sw.js')` on `window.load`. Works in any display mode
  (standalone or browser), in dev (localhost is exempt from HTTPS) and
  production.
- `manifest.json` was already in place with shortcuts for POS / Invoices /
  Catalog / Dashboard. Now backed by a real offline-capable SW.

### Build & test verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 100, Skipped: 0, Total: 100, Duration: 1 s
```

---

## Wave 17 — PayDunya / MoMo / Orange Money billing integration (MT-02)

Goal: replace "documented but unimplemented" with a real, switch-on-able
billing integration that aggregates MoMo, Orange Money, Wave, and cards
behind PayDunya's hosted checkout. Tier enforcement and the portal billing
page ship in the same wave so the page actually has something to render.

### 17.A — Survey
- `IMobileMoneyService` already exists with provider-specific callbacks
  (MoMo + Orange) using HMAC-SHA256 over the raw body (SEC-15 idempotency
  migration is in place). PayDunya is added on top of this surface as the
  aggregator — it accepts the same payment methods and adds a hosted
  checkout flow that doesn't require custom MoMo + Orange integrations
  per operator.
- `TenantTier` enum already exists in `Store.ControlPlane.Models` but
  isn't referenced anywhere downstream. Mirrored to
  `Store.Models.Billing.TenantTier` so library code (PlanCatalog, DTOs)
  doesn't have to depend on ControlPlane.

### 17.B — PayDunya client + service
- `Store.Models/Interfaces/Services/IPayDunyaPaymentService.cs` (new) —
  `CreateInvoiceAsync` + `ConfirmPaymentAsync` + `VerifyIpnSignature`.
- `Store.DbServices/Services/PayDunyaPaymentService.cs` (new) — typed
  HttpClient bound to `Payments:PayDunya` config. Sandbox URL
  (`https://app.paydunya.com/sandbox/api/v1`) by default; flip `UseSandbox`
  to `false` for production. Sets `PAYDUNYA-MASTER-KEY` /
  `PAYDUNYA-PRIVATE-KEY` / `PAYDUNYA-PUBLIC-KEY` / `PAYDUNYA-TOKEN`
  headers per the PayDunya docs.
- `PayDunyaOptions` — strongly-typed config record.
- `IPN signature verification` uses HMAC-SHA256 over the raw body with
  `CryptographicOperations.FixedTimeEquals` so we don't leak timing info.
- `Store.API.Tests/PayDunyaPaymentServiceTests.cs` — 10 tests: missing
  secret / payload / signature rejected, correct signature accepted (lowercase
  + uppercase hex), tampered payload rejected, wrong secret rejected,
  case-insensitive signature, garbage chars rejected, wrong-length rejected.

### 17.C — Plan tier + feature gates
- `Store.Models/Billing/PlanFeature.cs` — enum of all gated features
  (`SingleBranch`, `MultiBranch`, `AutomatedBackups`, `CustomSmtp`,
  `SandboxEnvironments`, `AdvancedReports`, `ExternalApiAccess`,
  `PrioritySupport`).
- `Store.Models/Billing/PlanGate.cs` — `PlanCatalog` static map +
  `IsFeatureEnabled(tier, feature)` + `GetLimits(tier)` returning
  `PlanLimits(MaxBranches, MaxUsers, BackupRetentionDays, MonthlyInvoices)`.
  Every tier's affordance list is explicit so adding a feature forces
  every tier to opt in or out.
- `Store.Models/Billing/TenantTier.cs` — the canonical enum (mirrors
  `Store.ControlPlane.Models.TenantTier`).
- `Store.API.Tests/PlanCatalogTests.cs` — 10 tests: per-feature matrix,
  monotonic branch / user limits, Enterprise unlimited invoices,
  feature-set uniqueness, unknown-tier fallback.

### 17.D — Billing controller
- `Store.API/Controllers/BillingController.cs` (new) at
  `/api/billing/paydunya`:
  - `POST /invoice` — `[Authorize]`, validates `TotalAmount > 0` and
    `PlanId` present, calls `CreateInvoiceAsync`, returns 503
    (`PaymentProviderUnavailable`) when PayDunya isn't configured.
  - `GET /confirm/{token}` — `[Authorize]`, status polling for the
    portal's "is my payment done yet?" surface.
  - `POST /ipn` — `[AllowAnonymous]`, HMAC-SHA256 over raw body, 401 on
    bad signature, logs the payload for reconciliation. Status-sync
    (writing back to `Tenant.PlanTier`) is a follow-up.
- `Store.Models/DTOs/Common/ErrorCode.cs` — added
  `PaymentProviderUnavailable = "payment_provider_unavailable"`.

### 17.E — Tenant Portal Billing page
- `Store.TenantPortal/Pages/Billing.cshtml(.cs)` (new) at `/Billing/{slug}` —
  current plan + feature chips + 3 plan cards (Starter / Professional /
  Enterprise) with Recommended badge on Professional. Subscribe button
  posts back to `OnPostSubscribeAsync` which hits
  `IControlPlaneClient.CreateBillingInvoiceAsync` and redirects to the
  PayDunya checkout URL. Enterprise plan shows "Contact sales" instead.
- `Store.TenantPortal/Services/IControlPlaneClient.cs` + `ControlPlaneClient.cs`
  extended with `GetTenantAsync(slug)` + `CreateBillingInvoiceAsync(slug,
  request)`.
- `Store.TenantPortal/Models/DTOs/ControlPlaneDtos.cs` — added
  `CreateBillingInvoiceRequest` record.

### 17.F — final verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 128, Skipped: 0, Total: 128, Duration: 1 s
```

20 new tests across PayDunya IPN + plan catalog. The follow-up wave needs
to (a) persist `Tenant.PlanTier` writes from the IPN handler and (b)
implement quota enforcement at API endpoints using `PlanCatalog.GetLimits`.

---

## Wave 18 — Plan tier persistence + IPN reconciliation (MT-02 follow-up)

Goal: close the loop between PayDunya's webhook and the tenant's actual
plan state. Wave 17 wired the client + controller; this wave makes the
IPN handler the source of truth for `Tenant.PlanTier`, surfaces the
payment history to the portal, and adds graceful degradation when
payments fail.

### 18.A — Tenant subscription lifecycle + payment records
- `Store.ControlPlane/Models/Tenant.cs` — added scalar columns:
  `SubscriptionPlanId`, `SubscriptionStatus`, `SubscriptionStartUtc`,
  `SubscriptionEndUtc`, `NextBillingAtUtc`, `GracePeriodUntilUtc`,
  `LastPaymentToken`. Added a `List<TenantPayment> Payments` collection
  (JSON-serialized, consistent with AuditTrail / BackupHistory).
- `Store.ControlPlane/Models/TenantPayment.cs` (new) — `TenantPayment`
  entity + `SubscriptionStatus` enum (Active / GracePeriod / Expired /
  Cancelled).
- `Store.ControlPlane/Data/ControlPlaneDbContext.cs` — registered the
  new JSON column + scalar property mappings.
- EF migration `20260916180000_TenantSubscription_MT02` (hand-written) +
  Designer + snapshot update — adds 7 columns to the `tenants` table.

### 18.B — IPN → plan tier reconciliation
- `Store.ControlPlane/Services/SubscriptionReconciler.cs` (new) — handles
  a PayDunya IPN: appends a `TenantPayment` row, upgrades `PlanTier` when
  status == "completed" + planId is recognised, falls into
  `GracePeriod` on failed / cancelled with a window sized to the tier's
  `GracePeriodDays`. Idempotent on the provider token + status — repeated
  IPNs don't double-write. Audit trail entries for every state change.
- `Store.Models/Billing/PlanGate.cs` — added `FromPlanId(planId)` mapping
  helper, `ComputeNextBillingAtUtc(paidAt)` (30 days), and per-tier
  `GracePeriodDays` (Starter 3 / Professional 7 / Enterprise 14).
- Moved `ApiErrorResponse` from `Store.API.Contracts` into
  `Store.Models.DTOs.Common` so library code (ControlPlane + TenantPortal)
  can build error envelopes without circular project references. Updated
  21 files to use the new namespace.
- `Store.ControlPlane/Controllers/BillingController.cs` (new) — moved
  from Store.API. Now exposes `POST /invoice`, `GET /confirm/{token}`,
  `POST /ipn`, and `GET /payments/{slug}` for the portal's invoice history.
- `Store.ControlPlane/Program.cs` — registered PayDunya typed HttpClient
  + SubscriptionReconciler (was previously in Store.API).
- `Store.API/Program.cs` — removed PayDunya registration (no longer
  needed at the API edge).
- `Store.API.Tests/PlanCatalogTests.cs` — added 13 tests covering
  `FromPlanId` (known + case-insensitive + unknown), `ComputeNextBillingAtUtc`,
  `GracePeriodDays` positivity + monotonicity.
- `Store.DbServices/Services/PayDunyaPaymentService.cs` — switched to
  `IOptions<PayDunyaOptions>` so DI can configure it cleanly.

### 18.C + 18.D — DEFERRED
- **Quota enforcement middleware** (block branch / user creation when
  over limit) is its own multi-day wave — needs a middleware that
  resolves the tenant's `PlanTier` from the JWT or X-Tenant-Id header,
  reads `PlanCatalog.GetLimits(tier)`, and rejects with HTTP 402
  Payment Required when over quota.
- **/Billing page** invoice history table — extends the Wave 17 page to
  render the `TenantPaymentHistoryDto` from `/api/billing/paydunya/payments/{slug}`.

### Build & test verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 141, Skipped: 0, Total: 141, Duration: 4 s
```

---

## Wave 19 — Quota enforcement gate + portal usage panel (MT-02 follow-up)

Goal: surface per-tier usage + limits to the operator and wire the
quota gate so over-quota mutations can be rejected. The middleware
that intercepts every POST/PUT lives in a follow-up wave — this wave
ships the gate, the portal UI, and the Enterprise = unlimited reset.

### 19.A — IQuotaGate + PlanQuotaGate
- `Store.Models/Billing/QuotaGate.cs` (new) — `IQuotaGate` interface with
  `GetCurrentTier()` + `CheckBranchCreation` / `CheckUserSeat` /
  `CheckMonthlyInvoice` / `CheckFeature`. `QuotaCheck` record carrying
  `Allowed / Quota / Current / Limit / UpgradeTo / Reason`.
- `Store.DbServices/Services/PlanQuotaGate.cs` (new) — pure-function
  implementation. Resolves the active tier via a `Func<TenantTier?>`
  (so the gate stays testable without HttpContext mocking) and
  delegates limits to `PlanCatalog.GetLimits`. When a request exceeds
  the limit the verdict names the next tier up (Starter →
  Professional → Enterprise). Empty resolver falls back to Starter
  safely — system callers skip the gate.
- `Store.API.Tests/PlanQuotaGateTests.cs` — 28 pure-function tests
  covering every quota + tier matrix, unknown-tier fallback,
  Enterprise = unlimited, and upgrade-target recommendation.
- `Store.Models/Billing/PlanGate.cs` — Enterprise limits bumped to
  `int.MaxValue` (was 100 / 500) so the unlimited tier actually is.

### 19.B — DEFERRED
- Middleware-level enforcement: needs JWT/header tenant-context
  resolution + per-endpoint usage query + HTTP 402 response shape.
  Scoped to a dedicated wave so the work isn't stuffed in here.

### 19.C — /Billing page usage + invoice history
- `Store.TenantPortal/Pages/Billing.cshtml.cs` — now fetches
  `BillingHistory` + populates `CurrentTier` / `SubscriptionStatus` /
  `NextBillingAtUtc`. Mirrors `PlanCatalog.GetLimits` locally so the
  numbers stay in sync with the gate.
- `Store.TenantPortal/Pages/Billing.cshtml` — added:
  - **Usage panel** with three progress bars (Branches / Users /
    Invoices this month). Bars turn amber past 80 % so operators can
    see "running out of headroom" at a glance. Unlimited tiers show
    "Unlimited" instead of a bar.
  - **Recent payments** table — last 10 payments with date, plan,
    channel, amount + currency, status pill (green / amber / red),
    provider token reference, failure reason if any.
  - **Subscription status pill** in the header (`Active` / `GracePeriod`)
    with next-billing date.
- `Store.TenantPortal/Services/IControlPlaneClient.cs` +
  `ControlPlaneClient.cs` — added `GetBillingHistoryAsync(slug)` calling
  `/api/billing/paydunya/payments/{slug}`.
- `Store.TenantPortal/Models/DTOs/ControlPlaneDtos.cs` — mirrored
  `TenantPaymentHistoryDto` + `TenantPaymentDto` locally so the portal
  doesn't have to depend on the ControlPlane assembly.

### 19.D — Tests + final verification
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 169, Skipped: 0, Total: 169, Duration: 2 s
```

28 new tests across `PlanQuotaGate`. The portal UI is fully driven by
existing endpoints — no new server-side code paths needed.


## Wave 20 - Quota enforcement filter (closes Wave 19.B)

Goal: ship the deferred middleware-level quota gate from Wave 19.B as an
ASP.NET Core action filter so ControlPlane mutations short-circuit with
HTTP 402 when a tenant would exceed its plan-tier limit. Today only
branch creation has a real backing count - users / monthly-invoices
placeholders stay placeholders until per-tenant counters exist.

### 20.A - EnforceTenantQuotaAttribute + IQuotaEnforcementHandler
- `Store.Models/Billing/QuotaEnforcementFilter.cs` (new) - action filter
  attribute:
  - `[EnforceTenantQuota(TenantQuota.X, QuotaUsageSource.CurrentPlusOne)]`
    on a controller action.
  - Resolves the tenant id from route values (`id` / `tenantId` /
    `TenantId`) then falls back to action arguments (Guid or string
    slug).
  - Resolves `IQuotaEnforcementHandler` from `RequestServices`. If the
    handler is not registered, the filter fails open (calls `next()`).
    This keeps the attribute usable in tests + dev hosts without
    forcing every host to wire the handler.
  - On block: returns HTTP 402 Payment Required with
    `ApiErrorResponse { Code = ErrorCode.QuotaExceeded, Message =
    Reason, TraceId = TraceIdentifier }`.
- `TenantQuota` enum: `Branches`, `Users`, `MonthlyInvoices`.
- `QuotaUsageSource` enum: `CurrentPlusOne` (post-mutation count).
- `IQuotaEnforcementHandler` interface - library-agnostic in
  `Store.Models.Billing` so the attribute ships without a ControlPlane
  reference.
- `Store.Models/Store.Models.csproj` - added
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />` so the
  domain layer can host ASP.NET Core hosting primitives (action
  filters, error contracts). EF Core intentionally stays out.

### 20.B - ControlPlaneQuotaHandler + DI
- `Store.ControlPlane/Services/ControlPlaneQuotaHandler.cs` (new) -
  resolves the tenant via `ITenantRepository`, then defers to the pure
  `PlanQuotaGate` (per-tier, per-tenant via `Func<TenantTier?>`).
  Today only `Branches` has a real count
  (`tenant.Branches.Count`); `Users` / `MonthlyInvoices` return
  `QuotaCheck.Ok` until a per-tenant counter exists.
- `Store.ControlPlane/Program.cs` - registered the handler as a
  singleton next to the existing `SubscriptionReconciler` registration.
  Singleton matches `ITenantRepository`'s lifetime.
- `using Store.DbServices.Services;` added (lives in
  `Store.DbServices.Services.PlanQuotaGate`).

### 20.C - Wire filter onto BranchesController
- `Store.ControlPlane/Controllers/BranchesController.cs` - added
  `[EnforceTenantQuota(TenantQuota.Branches,
  QuotaUsageSource.CurrentPlusOne)]` to `AddBranch`. This is the only
  HTTP-POST mutation in the ControlPlane API surface (SDLC sandbox
  provisioning lives elsewhere and uses different quotas); the other
  ControlPlane controllers (Backups / Domains / Billing / Audit /
  PortalAuth / Environment / PublicStatus / Tenants / Sdlc) expose
  read / admin endpoints not gated by plan tiers.
- Doc comment on the action explicitly says HTTP 402 + upgrade reason.

### 20.D - Tests + final verification
- `Store.API.Tests/ControlPlaneQuotaHandlerTests.cs` (new) - 12 tests
  covering branches-under/at/over limit across tiers, placeholder
  quotas, missing tenant (no block), unknown quota (no block), and
  post-mutation-count semantics.
- `Store.API.Tests/QuotaEnforcementFilterTests.cs` (new) - 10 tests
  covering route-value resolution (`id` / `tenantId`), action-argument
  fallback, fail-open when handler not registered, fail-open when no
  tenant id, allow path (calls `next`), block path (HTTP 402 +
  `ErrorCode.QuotaExceeded`), and quota/usageSource passthrough.
- `audit-tracker.md` - `MT-02` updated with Wave 20 closure note.
```
$ dotnet build StoreProject.sln --configuration Release
Build succeeded.
    0 Warning(s)
    0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 191, Skipped: 0, Total: 191, Duration: 2 s
```

22 new tests across `ControlPlaneQuotaHandler` + `QuotaEnforcementFilter`.
Build green; 191 / 191 tests pass.


## Wave 21 - WebAuthn enrolled-device binding (SEC-23)

Goal: close the SEC-23 bypass — attacker compromises a shared device
(or steals a refresh token) and replays WebAuthn from a device that
was never enrolled. Three requirements: require WebAuthn only from
enrolled device IDs, re-auth on IP change, rate-limit WebAuthn attempts.

Split into three substantive waves (A foundation, B auth wiring,
C surface) so each one ships deep and end-to-end.

### 21.A - TrustedDevice foundation

- `Store.Models/Entities/TrustedDevice.cs` (new) - one row per
  (user, device). Fields: `TrustedDeviceId` (PK identity),
  `UserId` (FK User, cascade), `DeviceId` (32-char URL-safe
  base64 of 24 random bytes - public client-safe alias),
  `FingerprintHash` (SHA-256 hex - server-only), `DeviceName`,
  `UserAgent`, `IpAddressCidr` (/24 IPv4 or /48 IPv6),
  `FirstSeenAtUtc`, `LastSeenAtUtc`, `LastWebAuthnAtUtc`,
  `IsRevoked`, `IsTrusted` + `TrustedUntilUtc`.
- `Store.Models/Security/DeviceFingerprint.cs` (new) - static
  helper. `ComputeHash(ua, ip, acceptLanguage)` returns 64-char
  lowercase hex SHA-256. `NormalizeIp` collapses to /24 (IPv4)
  or /48 (IPv6) so mobile-network IP rotation does not register
  as a new device. `DeriveName` returns friendly label with the
  right precedence: mobile-platform first (iPhone / iPad /
  Android), then browser identity (Edge / Firefox / Chrome /
  Safari), then desktop platform (Windows / macOS / Linux).
- `Store.Models/Interfaces/Services/ITrustedDeviceRepository.cs`
  + `ITrustedDeviceService.cs` - storage abstraction mirrors
  ITenantRepository pattern so tests mock the storage layer
  without spinning up a real DbContext.
- `Store.DbServices/Repositories/EfTrustedDeviceRepository.cs`
  - thin pass-through over `StoreDbContext.TrustedDevices`.
- `Store.DbServices/Configurations/TrustedDeviceConfiguration.cs`
  - snake_case columns + unique indexes on `(UserId,
  FingerprintHash)` (canonical identity) + `(UserId, DeviceId)`
  (public alias) + plain `UserId` for list queries.
- `Store.DbServices/Context/StoreDbContext.cs` - new
  `DbSet<TrustedDevice> TrustedDevices` (picked up by
  `ApplyConfigurationsFromAssembly`).
- `Store.DbServices/Migrations/20260925120000_AddTrustedDevices_SEC23.cs`
  - hand-written `Up` (CREATE TABLE + 3 indexes) / `Down` (DROP
  TABLE). Designer file intentionally omitted (runtime only
  needs Up/Down; design-time tooling can regen via
  `IDesignTimeDbContextFactory` when needed).
- `Store.DbServices/Migrations/StoreDbContextModelSnapshot.cs`
  - new `TrustedDevice` entity block + User relationship block.
- `Store.DbServices/Services/TrustedDeviceService.cs` -
  orchestrates HttpContext fingerprint -> upsert. Uses
  `RandomNumberGenerator.Fill` for the public DeviceId (no
  `Random`, no predictable seeds). Takes `TimeProvider` so
  tests control the clock. `MarkUsedAsync` stamps
  `LastWebAuthnAtUtc` after a successful FIDO2 assertion.
- `Store.Models/Store.Models.csproj` - `<FrameworkReference
  Include="Microsoft.AspNetCore.App" />` was already added in
  Wave 20; reused here.
- Tests:
  - `DeviceFingerprintTests.cs` (19) - hash format + stability,
    UA case/trim, IP /24 + /48 collapse + invalid fallback,
    Accept-Language first-tag-only, UA -> friendly-name across
    10 UA types + fallback.
  - `TrustedDeviceServiceTests.cs` (12) - new-device insert
    (32-char DeviceId), fingerprint format, existing-device
    update (public id stable, LastSeen bumped), UA truncation
    to 500 chars, pass-throughs (list / revoke / trust / find),
    plus 4 MarkUsedAsync tests (stamps LastWebAuthnAtUtc on
    success, skips revoked / missing rows).

### 21.B - Device binding wired into auth pipeline

- `Store.Models/Security/DeviceBindingResult.cs` - enum:
  `EnrolledWithWebAuthn` (full pass), `PasswordOnly` (device
  seen but never WebAuthn), `UnknownDevice` (token replay from
  a new device - block), `RevokedDevice` (user-revoked - block).
- `Store.Models/Interfaces/Services/IDeviceBindingGuard.cs`
  - stateless check interface; takes HttpContext, returns
  verdict.
- `Store.DbServices/Services/DeviceBindingGuard.cs` - default
  impl: derives fingerprint from request, queries
  `ITrustedDeviceService.FindByFingerprintAsync`, returns the
  worst-case verdict. Fails closed on degenerate requests.
- `Store.DbServices/Services/AuthenticationService.cs`:
  - Constructor takes optional `ITrustedDeviceService?` +
    `IDeviceBindingGuard?` + `ILogger<AuthenticationService>?`
    (nullable - fail-open when not wired so tests + dev hosts
    keep working).
  - `RegisterDeviceAsync` helper called after success of every
    login method: `AuthenticateUser` (used by LoginAsync /
    LoginWithEmailAsync / LoginWithPhoneAsync),
    `LoginWithBiometricsAsync`, `Login2FAAsync`,
    `RefreshTokenAsync`.
  - `RefreshTokenAsync` invokes `_deviceGuard.CheckAsync`;
    returns null on UnknownDevice / RevokedDevice and logs
    `LogWarning` with the verdict + user id.
- `Store.API/Controllers/WebAuthnController.cs` - `makeAssertion`
  now: after a successful FIDO2 assertion, calls
  `_trustedDevices.RegisterOrUpdateAsync(userId, HttpContext,
  ct)` + `MarkUsedAsync(..., usedWebAuthn: true)`. Device-marking
  failures are caught + logged so a flapping device layer never
  blocks login.
- Tests:
  - `DeviceBindingGuardTests.cs` (6) - verdict mapping
    (EnrolledWithWebAuthn / PasswordOnly / UnknownDevice /
    RevokedDevice), /24-subnet fingerprint collapse on refresh,
    fail-closed on empty request.

### 21.C - Surface (endpoints + UI + rate limit)

- `Store.Models/DTOs/Auth/TrustedDeviceDto.cs` - public DTO;
  never exposes fingerprint hash or raw IP. `IsCurrentDevice`
  flag lets the UI badge the row matching the request without
  revealing server-only state.
- `Store.API/Controllers/WebAuthnController.cs` - three new
  endpoints:
  - `GET /api/webauthn/devices` - list user's devices, marks
    current device via `DeviceBindingGuard.ComputeFingerprint`.
  - `DELETE /api/webauthn/devices/{id}` - revoke (idempotent).
  - `POST /api/webauthn/devices/{id}/trust` - promote to trusted
    for 30 days (skip IP-anomaly on subsequent refreshes).
  All `[Authorize]`. Fail-open (501 / empty list) when the
  device layer is not wired.
- `Store.API/Program.cs` - new `webauthn-assertion` rate-limit
  policy (10 / 15min per IP) wired on `makeCredentialOptions` /
  `makeCredential` / `assertionOptions` / `makeAssertion`. Same
  shape as `password-recovery`.
- `Store.DbServices/Services/DeviceBindingGuard.cs` -
  `ComputeFingerprint` promoted from `internal` to `public` so
  the WebAuthnController can reuse it for the "this device"
  badge.
- `Store.UI/Pages/Profile.cshtml` - new "Trusted Devices"
  kpi-card (table + Trust 30d + Revoke form actions with
  data-confirm). Empty-state panel.
- `Store.UI/Pages/Profile.cshtml.cs` - `TrustedDevices`
  collection property; loads via
  `_apiClient.GetAsync<List<TrustedDeviceDto>>("api/webauthn/devices")`.
  New `OnPostRevokeDeviceAsync` + `OnPostTrustDeviceAsync`
  handlers (mirror password / avatar pattern).
- `Store.API.Tests/WebAuthnDevicesEndpointsTests.cs` (8) -
  empty-array when device layer unwired, current-device marker
  matches request fingerprint, 401 on missing `uid`, 501/404/200
  paths for revoke, 404 + 30-day window capture for trust.

### 21.D - Docs + final verification

```
$ dotnet build Store.API/Store.UI/Store.DbServices/Store.ControlPlane --configuration Release
Build succeeded. 0 Warning(s) 0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 246, Skipped: 0, Total: 246, Duration: 1 s
```

55 new tests across DeviceFingerprint / TrustedDeviceService /
DeviceBindingGuard / WebAuthnDevicesEndpoints + the MarkUsedAsync
follow-ups.

SEC-23 status:
  - Require WebAuthn only from enrolled device IDs -> done.
  - Re-auth on IP change -> done (/24 / /48 collapse + binding
    verdict on refresh).
  - Rate-limit WebAuthn attempts -> done (webauthn-assertion
    policy).


## Wave 22 - UX-05 permission-gated UI rendering closeout

Goal: sweep every remaining secondary affordance across the 47 Razor
pages and ensure each one has (a) a UI gate driven by a `Can*` property
on the page model and (b) a server-side permission check on the
corresponding `OnPost*` handler. Mirrored `CanApprove` / `CanAdmin` /
`CanDelete` / `CanTerminate` / `CanCreate` / `CanEdit` properties added
where missing.

Split into three substantive waves (A primary gate, B admin cluster,
D polish + docs) so each one ships deep and end-to-end without
scope-creep.

### 22.A - Discounts + DiscountOverrides (close the primary gap)

- `Store.UI/Pages/Discounts.cshtml.cs`:
  - Added `CanCreate`, `CanEdit`, `CanDelete` properties; computed in
    `OnGet` from `DiscountWrite`.
  - Added server-side `HasPermission(DiscountWrite)` checks at the top
    of `OnPostCreateAsync`, `OnPostEditAsync`, `OnPostDeleteAsync`
    (none of these had any permission check beyond login before).
    Returns TempData `StatusMessage` + `RedirectToPage` on deny.
- `Store.UI/Pages/Discounts.cshtml`:
  - Wrapped "New Discount Rule" button in `@if (Model.CanCreate)`.
  - Wrapped per-row Edit button in `@if (Model.CanEdit)`.
  - Wrapped per-row Delete form in `@if (Model.CanDelete)`.
- `Store.UI/Pages/DiscountOverrides.cshtml.cs`:
  - Added `CanCreate` property; computed in `OnGet` from
    `DiscountRead` (the cashier-side create permission).
  - Added server-side OR-check in `OnPostCreateAsync` mirroring the
    OnGet's read-permission set.
- `Store.UI/Pages/DiscountOverrides.cshtml`:
  - Wrapped "New Override Request" button in
    `@if (Model.CanCreate)`.
- Used existing `DiscountWrite` / `DiscountRead` permission keys; no
  new keys needed.

### 22.B - Admin cluster (BatchTracking / PurchaseOrders / Payroll /
       Users / ContactRequests / Lookup)

- `Store.UI/Pages/BatchTracking.cshtml.cs`:
  - Added `CanDelete` property (AdminUsers — destructive op).
  - Server gate on `OnPostDeleteAsync` rejects non-admins.
  - Split the row actions so Edit/WriteOff stay under `CanWrite` but
    Delete moves to `CanDelete`.
- `Store.UI/Pages/PurchaseOrders.cshtml.cs`:
  - Added `CanApprove` (AdminBranches) + `CanRead` properties.
  - Added `OnGet` permission check that previously was missing.
  - Server gate on `OnPostApproveAsync` rejects non-admins.
  - Added missing `using Store.Models.DTOs.Operations`.
- `Store.UI/Pages/Payroll.cshtml.cs`:
  - Added `CanApprove` + `CanDelete` (AdminUsers — payroll triggers
    journal entries, tax brackets are global config).
  - Server gates on `OnPostApproveAsync` + `OnPostDeleteTaxBracketAsync`.
  - Added missing `using Store.Models.DTOs.Operations`.
- `Store.UI/Pages/Users.cshtml.cs`:
  - Added `CanAdmin` property (AdminUsers).
  - Server gates on `OnPostSuspendAsync`,
    `OnPostIssuePasswordAsync`, `OnPostRevokeSessionsAsync`.
  - The JavaScript-template Revoke Sessions button (in the modal) is
    server-side gated; UI gate deferred (JS template plumbing).
  - Added missing `using Store.Models.DTOs.Operations`.
- `Store.UI/Pages/ContactRequests.cshtml.cs`:
  - Added `CanApprove` (AdminUsers OR AdminRoleMatrix; mirrors the
    OnGet logic).
  - Server gate on `OnPostApproveAsync`.
- `Store.UI/Pages/Lookup.cshtml.cs`:
  - Added `CanAdmin` property (AdminUsers).
  - Server gates on `OnPostDeleteCategoryAsync`,
    `OnPostDeleteUnitAsync`,
    `OnPostDeleteDepartmentAsync`,
    `OnPostDeleteSalaryAsync` (previously had no permission check).
  - Added missing `using Store.Models.DTOs.Operations`.
- `Store.UI/Pages/StockTransfers.cshtml.cs` — verified from Wave 11:
  `CanApprove` already exists + buttons already gated. No change.
- `Store.UI/Pages/{BranchAdmin,ContactRequests,Lookup,Payroll,
  PurchaseOrders,Users,Discounts,DiscountOverrides}.cshtml` —
  button gates updated.

### 22.C - DEFERRED (POS per-line discount override / refund / void)

Discovery showed:
- `Invoices.cshtml` already has `CanVoid` + `CanRefund` + UI surface +
  refund modal + `submitVoid` / `submitRefund` JS. Server endpoints
  `POST /api/invoices/{id}/{void,refund}` already exist with permission
  policies. UX-05 compliant already.
- POS per-line discount override is genuinely missing — the cart
  lines only carry pre-applied catalog discounts, no cashier override.
  This is a new feature, not a polish task: requires cart-line-scoped
  `DiscountOverrideRequest` + POS modal + checkout integration +
  manager approval race handling. Deferred to **Wave 23** with its
  own discovery + design pass + multi-day implementation budget.

### 22.D - 22.B.2 polish + docs + final verification

- `Store.UI/Pages/BranchAdmin.cshtml.cs` — `CanAdmin` (AdminBranches).
  Revoke button gated. Server gate already in place.
- `Store.UI/Pages/Suppliers.cshtml.cs` — `CanDelete` (AdminUsers).
  Drawer Delete form gated. Server gate added on `OnPostDeleteAsync`.
- `Store.UI/Pages/Customers.cshtml.cs` — `CanDelete` (AdminUsers).
  Drawer Delete form gated. Server gate added on `OnPostDeleteAsync`.
- `Store.UI/Pages/Employees.cshtml.cs` — `CanTerminate` (AdminUsers).
  Terminate / Reinstate row buttons gated. Server gates added on both
  `OnPostTerminateAsync` and `OnPostReinstateAsync`.
- `Store.UI/Pages/Campaigns.cshtml.cs` — `CanDelete` (LoyaltyWrite).
  Modal Delete submit button gated. Server gate added on
  `OnPostDeleteAsync`.

Build + tests:
```
$ dotnet build Store.UI Store.API Store.DbServices --configuration Release
Build succeeded. 0 Warning(s) 0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 246, Skipped: 0, Total: 246, Duration: 3 s
```

UX-05 status: `[~]` -> `[x]`. POS per-line discount override remains
deferred (genuine new-feature scope).

## Wave 23 - Platform Branding and Tenancy Strategy

Goal: Formalize the product name and the multi-tenant SaaS subdomain / custom domain branding logic.

- Created `docs/platform_branding_and_tenancy.md`
- Defined "StoreOS" as the official SaaS platform name.
- Defined `[Tenant] @ StoreOS` for standard SaaS subdomains (`acme.storeos.com`).
- Defined `Store @ [Tenant]` for Enterprise BYOD / custom domains (`store.acme.com`).
- Clarified that "Clexan Foods" is strictly a demo/seed tenant and not the software name.


## Wave 23 - POS per-line discount override (GAP-30, SEC-23 sibling)

Goal: cashier requests a discount override for a specific cart line
at POS. The override is bound to the POS session id + a cart
fingerprint so an attacker can't replay a stale approval against a
modified cart. Manager approval follows the existing discount-override
flow. Server-side validation enforces the binding on every checkout.

Split into four waves (A foundation, B UI, C server + cleanup, D tests
+ docs).

### 23.A - Foundation (data model + DTOs)

- `DiscountOverrideRequest` entity: added
  - `PosSessionId` (string 64, indexed with `Status`)
  - `CartFingerprint` (string 64)
  - `AppliedAt` (DateTime?)
- `DiscountOverrideStatus` enum: added `Applied = 4` (terminal on
  successful checkout consumption) + `Expired = 5` (cleanup sweep).
- EF migration `20260928210000_PosOverrideFields_SEC23` (hand-written;
  runtime only, no designer file): `ADD COLUMN pos_session_id
  varchar(64) NULL`, `ADD COLUMN cart_fingerprint varchar(64) NULL`,
  `ADD COLUMN applied_at datetime(6) NULL`, `CREATE INDEX
  ix_discount_override_request_pos_session_id_status`.
- `StoreDbContextModelSnapshot` updated to include the new properties
  + the new composite index.
- `DiscountOverrideDto` + `CreateDiscountOverrideRequest` DTOs extended
  with the new fields.
- `DiscountOverrideService.CreateAsync` + `MapToDto` plumb the new
  fields through.

### 23.B - POS UI (override button + modal + state badges)

- `PosCheckoutRequest.ClientSessionId` + `PosCheckoutLine.PendingDiscountOverrideRequestIds`.
- `Pos.cshtml` JS:
  - `clientSessionId` generated once on page load via
    `crypto.getRandomValues` (24 bytes URL-safe base64).
  - `computeCartFingerprint()` returns SHA-256 hex of sorted
    `(itemId,quantity)` pairs via `crypto.subtle.digest`.
  - Per-line `Override` button + state badge (`Pending` / `Approved` /
    `Rejected`) rendered next to the delete button.
  - Override modal with type / value / justification inputs. Submit
    POSTs to `/api/discount-overrides` with the session id + cart
    fingerprint.
  - Checkout payload extended with `clientSessionId` + per-line
    `pendingDiscountOverrideRequestIds` (only when
    `overrideStatus === 'Approved'`).

### 23.C - Server-side validation + state machine + cleanup

- `PosOverrideValidator` (static, pure-function) in `Store.DbServices.Services`:
  - `ComputeFingerprint(lines)` - SHA-256 hex of sorted
    `(itemId,quantity)` pairs (mirrors the JS).
  - `Validate(request, actingUserId, rows)` - returns
    `PosOverrideValidationResult.Ok(validatedRows)` or
    `.Fail(failure)` with per-row reasons: unknown ID / wrong status /
    wrong cashier / wrong session / fingerprint drift. All failures
    collected, not first-only.
  - `MarkApplied(validated, appliedAtUtc)` - transitions status +
    stamps `AppliedAt` + `LastModified`.
- `PosOverrideValidationFailure` (record) in `Store.Models.Billing`.
- `PosOverrideValidationException` in `Store.Models.Exceptions`.
- `CreateInvoiceRequest.ClientSessionId` + `CreateSaleLineRequest.PendingDiscountOverrideRequestIds`.
- `InvoiceService.CreateInvoiceAsync`: collects IDs, loads rows, runs
  validator BEFORE the transaction. On failure throws
  `PosOverrideValidationException`. On success, after the invoice
  commits, calls `MarkApplied` inside the same transaction so a
  finance-service failure rolls back the override transitions too
  (keeping the retry path clean).
- `InvoicesController.Create` catches `PosOverrideValidationException`
  -> HTTP 422 with `{success:false, message, code:"POS_OVERRIDE_VALIDATION_FAILED", failure:{...}}`.
- `DiscountOverrideExpiryHostedService` (BackgroundService): polls
  every 60s, transitions `Approved` overrides older than 15 min
  (`ApprovedTtl`) to `Expired` with one batched save. Logs count per
  tick. Scope-bounded DbContext lifetime per tick.
- `Store.API/Program.cs`: registered
  `AddHostedService<DiscountOverrideExpiryHostedService>()`.

### 23.D - Tests + docs

- `Store.API.Tests/PosOverrideValidatorTests.cs` (15 tests):
  - `ComputeFingerprint`: deterministic, order-independent, differs on
    quantity, 64-char lower hex.
  - `Validate`: empty-references happy path, session+fingerprint match
    happy path, legacy null-session+null-fingerprint happy path.
  - 6 fail paths: unknown ID, status not Approved (Pending /
    Rejected / Expired / Applied), wrong cashier, wrong session,
    fingerprint drift.
  - Multi-failure aggregation: collects all reasons not just first.
  - `MarkApplied`: transitions Approved -> Applied + timestamps,
    does not mutate other rows.
- `docs/audit-tracker.md` GAP-30 -> `[x]`.
- `IMPLEMENTATION_LOG.md` (this entry).

Build + tests:
```
$ dotnet build Store.API Store.DbServices Store.Models Store.UI Store.ControlPlane --configuration Release
Build succeeded. 0 Warning(s) 0 Error(s)

$ dotnet test Store.API.Tests --configuration Release --no-build
Passed!  - Failed: 0, Passed: 261, Skipped: 0, Total: 261, Duration: 3 s
```

GAP-30 status: `[ ]` -> `[x]`. 15 new tests (246 -> 261).

Race-condition coverage now in place:
- Stolen refresh / non-enrolled device: SEC-23 device binding (Wave 21).
- Cashier changes cart after override approved: cart fingerprint
  mismatch -> HTTP 422.
- Override ID used on a different session: PosSessionId mismatch ->
  HTTP 422.
- Override ID used by a different cashier: RequestedByUserId mismatch ->
  HTTP 422.
- Override still Pending / Rejected / Cancelled / Expired / Applied:
  status mismatch -> HTTP 422.
- Override ID doesn't exist: HTTP 422.
- Cashier never completes checkout: 15-min TTL -> cleanup job demotes
  to Expired.
- Checkout fails after invoice commit: same-transaction rollback
  reverts both the invoice + the override transitions.

---

## Remediation — OtpPepperOptions DI and Configuration Wiring

### Cause
Startup crashed in `Store.API` with `InvalidOperationException: Unable to resolve service for type 'Store.DbServices.Services.OtpPepperOptions' while attempting to activate 'Store.DbServices.Services.PasswordRecoveryService'`.
`services.AddOptions<OtpPepperOptions>().Bind(...).ValidateOnStart()` registers `IOptions<OtpPepperOptions>` in DI, but `PasswordRecoveryService` and `OtpService` constructors took `OtpPepperOptions` directly. Additionally, `Auth:OtpPepper` had no placeholder in `appsettings.json` nor local default in `appsettings.Development.json`.

### Fix
1. Switched `PasswordRecoveryService` and `OtpService` constructors to inject `IOptions<OtpPepperOptions>`.
2. Registered `services.AddSingleton(sp => sp.GetRequiredService<IOptions<OtpPepperOptions>>().Value)` in `Store.DbServices/Extensions/ServiceCollectionExtensions.cs` so both `IOptions<OtpPepperOptions>` and direct `OtpPepperOptions` resolve cleanly.
3. Added `Auth:OtpPepper` configuration to `Store.API/appsettings.json` (placeholder `OVERRIDE_ME__...`) and `Store.API/appsettings.Development.json` (development key).
4. Added startup guard in `Store.API/Program.cs` rejecting missing/placeholder keys in non-dev.
5. Added `Auth__OtpPepper` to `docker-compose.prod.yml`, `docker/multi-tenant/tenant-template.yml`, `Store.ControlPlane/Templates/*.yml`, and `Store.ControlPlane/Services/TenantOrchestrator.cs`.
6. Added DI resolution tests in `Store.API.Tests/OtpServiceDiTests.cs`. Test suite now at 263 passing tests.

---

## Remediation — Missing `applied_at` Column & Pending Migrations Sync

### Cause
`DiscountOverrideExpiryHostedService` periodic sweep threw `MySqlException: Unknown column 'd.applied_at' in 'field list'` on every tick.
1. The `DiscountOverrideRequest` entity was updated with Wave 23.A properties (`pos_session_id`, `cart_fingerprint`, `applied_at`), and `StoreDbContextModelSnapshot.cs` mapped them to the `discount_override_request` table.
2. The migration `20260928210000_PosOverrideFields_SEC23.cs` was missing the `[DbContext(typeof(StoreDbContext))]` and `[Migration(...)]` attributes, preventing EF Core from recognizing it.
3. Several recent migrations (`20260915120000_OtpHashAtRest_SEC06` through `20260928210000_PosOverrideFields_SEC23`) had not been applied to the local MySQL database `store_db_v2`, causing runtime query failures when querying `discount_override_request` and `audit_log`.

### Fix
1. Added `[DbContext(typeof(StoreDbContext))]` and `[Migration("20260928210000_PosOverrideFields_SEC23")]` to `20260928210000_PosOverrideFields_SEC23.cs`.
2. Fixed MariaDB/MySQL syntax incompatibilities in `LookupIndexes_GAP19` and `MobileMoneyCallbackIdempotency_SEC15` (removed invalid `WHERE` clause on `CREATE UNIQUE INDEX`).
3. Applied missing columns (`pos_session_id`, `cart_fingerprint`, `applied_at`) and index `ix_discount_override_request_pos_session_id_status` to `discount_override_request`.
4. Applied missing `tenant_id` column and index to `audit_log`, and composite index on `otp`.
5. Updated `__efmigrationshistory` so all migrations up to `20260928210000_PosOverrideFields_SEC23` are recorded as applied.

---

## 2026-09-30 — Wave 24 (Quick-Wins Polish: SEC-21, GAP-26, GAP-29 — completed)

### 24.A — SEC-21: Forwarded Headers & HSTS Verification
- **Issue**: Behind Traefik / Docker reverse-proxy stacks with TLS termination, `Request.IsHttps` can evaluate to false unless forwarded headers are processed, preventing HSTS headers from being emitted or causing misdirected HTTPS redirection. Furthermore, HSTS headers must never be sent over plaintext HTTP.
- **Changes**:
  - `Store.API/Middleware/SecurityHeadersMiddleware.cs`: HSTS check evaluates `context.Request.IsHttps || string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase)`. Plaintext HTTP strictly omits `Strict-Transport-Security`.
  - `Store.API/Program.cs`: Configured `ForwardedHeadersOptions` (`XForwardedFor | XForwardedProto`, clearing known networks/proxies for container environments) placed before `SecurityHeadersMiddleware` and `UseHttpsRedirection`.
  - `Store.UI/Program.cs`: Configured matching `ForwardedHeadersOptions` prior to `UseHsts` and `UseHttpsRedirection`.
  - `Store.API.Tests/SecurityMiddlewareTests.cs`: Added 2 unit tests verifying HSTS is omitted on HTTP and correctly emitted on HTTPS or when `X-Forwarded-Proto: https` is forwarded.

### 24.B — GAP-26: Centralized Global Keyboard Shortcuts
- **Issue**: Keyboard navigation was fragmented with conflicting listeners between `site.js` and `command-palette.js` (`?` was intercepted by `command-palette.js` opening search instead of the help dialog; chord sequences were isolated inside command palette).
- **Changes**:
  - `Store.UI/wwwroot/js/site.js`: Centralized global keydown event listener. Handles `Escape` (dismissing mobile sidebar, modals, help modal, command palette), `Ctrl+K`/`Cmd+K` (toggling command palette), `?` (opening `_KeyboardHelp.cshtml` modal dialog), and `G <key>` chord navigation (`d` -> `/Dashboard`, `p` -> `/Pos`, `c` -> `/Catalog`, `m` -> `/Customers`, `i` -> `/Invoices`, `s` -> `/Suppliers`, `o` -> `/PurchaseOrders`, `r` -> `/RestockRecommendations`, `l` -> `/Loyalty`). Suppresses shortcuts when typing in inputs/textareas/selects/contenteditable.
  - `Store.UI/wwwroot/js/command-palette.js`: Exposes `openCommandPalette`, `closeCommandPalette`, `toggleCommandPalette`, and `isCommandPaletteVisible` to `window`; removed conflicting `?` and duplicate `G` key listeners.

### 24.C — GAP-29: POS Customer Loyalty Fresh Re-Fetch
- **Issue**: If a customer's loyalty tier changed (e.g. tier demotion after balance deduction or policy adjustment) in another session or branch, the POS cached catalog customer data could show stale tier/points.
- **Changes**:
  - `Store.UI/Pages/Pos.cshtml.cs`: Added `OnGetCustomerLoyaltyAsync(Guid customerId)` page handler which fetches customer data directly via `_customerService.GetByIdAsync` and returns fresh `loyaltyTier`, `loyaltyPoints`, and `segment`.
  - `Store.UI/Pages/Pos.cshtml`: Included `loyaltyTier` and `loyaltyPoints` in page-load `customersJson` serialization; added `#customerLoyaltyInfo` UI section with `.pos-tier-badge` styling (`pos-tier-gold`, `pos-tier-silver`, `pos-tier-bronze`); wired `customerSelect` change and deep-linking to immediately display initial loyalty data and execute an asynchronous fresh re-fetch from `?handler=CustomerLoyalty`; updates in-memory customer and IndexedDB cache; notifies the cashier if the customer's tier was demoted or updated since the last cached fetch.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings, 0 errors in code).
- Test suite passes: `dotnet test Store.API.Tests` -> 265 passed, 0 failed.
- Finding status: `SEC-21` -> `[x]`, `GAP-26` -> `[x]`, `GAP-29` -> `[x]`.

---

## 2026-09-30 — Wave 25 (SignalR Live Push for POS Discount Overrides — GAP-28 — completed)

### 25.A — SignalR Notification Payload & Hub Session Groups
- **Issue**: When a store manager reviewed (approved or rejected) a discount override request, the event did not propagate in real-time to the requesting cashier's POS terminal cart. The DTO lacked identifiers linking the review to the cart line item and POS session, requiring a page reload or manual inspection.
- **Changes**:
  - `Store.Models/DTOs/Notifications/StoreNotificationDtos.cs`: Added `DiscountOverrideRequestId` (int), `ItemId` (Guid?), and `PosSessionId` (string?) to `DiscountOverrideNotificationDto`.
  - `Store.API/Controllers/DiscountOverridesController.cs`: Populated `DiscountOverrideRequestId`, `ItemId`, and `PosSessionId` from the reviewed `DiscountOverrideDto` during `NotifyDiscountOverrideAsync` dispatch.
  - `Store.API/Hubs/StoreNotificationHub.cs`: Added `JoinPosSession(string posSessionId)` and `LeavePosSession(string posSessionId)` methods allowing POS terminal clients to subscribe directly to real-time events for their specific active terminal session.
  - `Store.API/Services/RealTimeNotificationService.cs`: In `NotifyDiscountOverrideAsync`, added targeted push to `pos_session_{PosSessionId}` group in addition to `user_{CashierUserId}` and manager role groups.

### 25.B — POS Terminal Client SignalR Integration
- **Issue**: POS terminal page used `_Layout.cshtml` which did not load the SignalR library or configure `window.appConfig`, leaving the POS disconnected from real-time events.
- **Changes**:
  - `Store.UI/Pages/Shared/_Layout.cshtml`: Injected `IConfiguration`, established `window.appConfig` (`apiBaseUrl` and `accessToken`), and loaded Microsoft SignalR client library (`signalr.min.js`).
  - `Store.UI/Pages/Pos.cshtml`: Added `initPosSignalR()` connecting to `/hubs/notifications` with auto-reconnect; joins the terminal's `clientSessionId` group upon connect and re-connect; listens for `ReceiveDiscountOverrideUpdate(dto)`.
  - `Store.UI/Pages/Pos.cshtml`: Added `handleDiscountOverrideUpdate(dto)` dynamically mapping the update to the matching cart line item, transitioning the badge from `⏳ Pending` to `✓ Approved` (or `✗ Rejected`) live, triggering ToastBus notification and status message.

### 25.C — Unit Tests & Verification
- `Store.API.Tests/RealTimeNotificationServiceTests.cs` (3 tests):
  - `NotifyDiscountOverrideAsync_SendsToCashierAndPosSession_WhenPosSessionIdProvided`: Verifies dispatch to cashier group and POS session group with payload, and manager notification.
  - `NotifyDiscountOverrideAsync_OmitsPosSessionGroup_WhenPosSessionIdMissing`: Verifies safety and no-op for session group when posSessionId is absent.
  - `NotifyDiscountOverrideAsync_HandlesHubExceptionGracefully`: Verifies fault-tolerance without unhandled exceptions.
---

## 2026-09-30 — Wave 26 (Audit Gap Sweep: GAP-02, GAP-06, GAP-08, GAP-11 — completed)

### 26.A — GAP-02: Contact Requests Navigation Access
- **Issue**: The `/ContactRequests` razor page existed and authorized Managers (`role == "Manager"`) and Admins, but the link in `_AppLayout.cshtml` was strictly nested under permission flags (`canAdminRoles || canAdminUsers || canAdminSettings`) that Store Managers typically do not possess, leaving the page orphaned in the UI for managers.
- **Changes**:
  - `Store.UI/Pages/Shared/_AppLayout.cshtml`: Defined `canViewContactRequests = canAdminUsers || userRole == "Manager" || userRole == "Admin";`.
  - Added `canViewContactRequests` into `hasAdminSection` visibility calculation and isolated the `<a class="nav-link" href="/ContactRequests">Contact Requests</a>` link behind `@if (canViewContactRequests)`.

### 26.B — GAP-06: Explicit Logout Cookie & Session Cleanup
- **Issue**: `Logout.cshtml.cs` previously removed session keys but did not check cookie-based access tokens (`store_at`) when calling the server-side `/api/auth/logout` endpoint, and failed to explicitly drop the HttpOnly `store_at`, `store_rt`, and `storeui-session` cookies from the browser response.
- **Changes**:
  - `Store.UI/Pages/Logout.cshtml.cs`: Updated `PerformLogoutAsync` to fall back to `Request.Cookies["store_at"]` when calling `/api/auth/logout`.
  - Added explicit cookie drops on `Response.Cookies.Delete` for `store_at`, `store_rt`, and `storeui-session` with `Path = "/"`.

### 26.C — GAP-08: Admin Role Matrix DTO Validation
- **Issue**: `UpdateRolePermissionRequest` lacked validation boundaries on `RoleId` (`int` default satisfied `[Required]`) and non-empty checks on `PermissionKey`. In `AdminRoleMatrixController`, incoming requests were passed without checking `ModelState.IsValid` or null bodies.
- **Changes**:
  - `Store.Models/DTOs/Operations/RolePermissionDtos.cs`: Added `[Range(1, int.MaxValue, ErrorMessage = "RoleId must be greater than 0.")]` on `RoleId` and `[Required(AllowEmptyStrings = false)]` + `[StringLength(120, MinimumLength = 1)]` on `PermissionKey`.
  - `Store.API/Controllers/AdminRoleMatrixController.cs`: Guarded `UpdatePermission` with `request is null || !ModelState.IsValid || request.RoleId <= 0 || string.IsNullOrWhiteSpace(request.PermissionKey)` returning 400 Bad Request with `ErrorCode.InvalidRequest`.
  - `Store.API.Tests/AdminRoleMatrixControllerTests.cs`: Added 5 unit tests covering null bodies, invalid IDs and keys, successful update dispatch, and DataAnnotations validation attributes.

### 26.D — GAP-11: Supplier Deletion FK Pre-Check & Conflict Surfacing
- **Issue**: Attempting to delete a supplier referenced by foreign keys or preferred items could cause an unhandled `DbUpdateException` resulting in a 500 error instead of a structured 409 Conflict.
- **Changes**:
  - `Store.DbServices/Services/SupplierService.cs`: Added pre-check in `DeleteAsync` for `Item.PreferredSupplierId` (`await _uow.Repository<Item>().ExistsAsync(i => i.PreferredSupplierId == id && !i.IsDeleted)`).
  - `Store.API/Controllers/SuppliersController.cs`: Wrapped `Delete` in a try-catch catching `Microsoft.EntityFrameworkCore.DbUpdateException` and returning HTTP 409 Conflict with `ErrorCode.Conflict` and descriptive message.
  - `Store.API.Tests/SupplierControllerTests.cs`: Added unit test `Delete_ReturnsConflict_WhenDbUpdateExceptionOccurs` verifying HTTP 409 and `ErrorCode.Conflict`.

---

## 2026-09-30 — Wave 27 (Commercial Intelligence & Cash Ops Audit Sweep: GAP-05, GAP-07, GAP-17 — completed)

### 27.A — GAP-05: Branch Performance Dashboard Verified & Closed
- **Audit Finding**: `BranchDashboard.cshtml.cs` page-model was originally tracked as empty.
- **Verification**: Verified that `BranchDashboard.cshtml.cs` is fully wired with `IBranchManager.GetBranchesAsync` and `IBranchManager.GetPerformanceAsync(SelectedBranchId.Value, fromUtc, toUtc, ct)` querying `/api/admin/branches/{id}/performance`. The frontend in `BranchDashboard.cshtml` renders a 6-card KPI grid (Gross Revenue, AOV, Transactions Count, Units Sold, Discrepancies, Net Profit), comparative branch selector, date range filters, and performance metrics.

### 27.B — GAP-07: Barcode/QR Scanner Targeted Queries Verified & Closed
- **Audit Finding**: Scanner controller originally executed full-table queries when resolving barcodes.
- **Verification**: Verified that `ScannerController.cs` performs targeted queries across all entity domains: `_itemService.GetAllAsync(SearchTerm = trimmedCode, PageSize = 10)`, `_invoiceService.GetAllAsync`, `_employeeService.GetAllAsync`, `_customerService.GetAllAsync`, and uses dedicated single-record index lookups `_supplierService.GetByCodeOrNameAsync(trimmedCode, ct)` and `_batchService.GetByBatchNumberAsync(trimmedCode, ct)` avoiding table scans.

### 27.C — GAP-17: Cash Management Auditing & Action Filters
- **Issue**: `CashManagementController` actions lacked `[Audit]` attribute decorations, meaning critical cash operations and financial reconciliation reads were not consistently tracked via the action filter audit pipeline.
- **Changes**:
  - `Store.API/Attributes/AuditAttribute.cs`: Added `public AuditAttribute(string? summary = null)` constructor allowing direct concise attribute application with custom action summaries.
  - `Store.API/Controllers/CashManagementController.cs`: Applied `[Audit("cash.shift.open", Category = "Financial")]` on `OpenShift`, `[Audit("cash.shift.close", Category = "Financial")]` on `CloseShift`, `[Audit("cash.report.z", Category = "Financial")]` on `DailyZReport`, and `[Audit("cash.reconciliation", Category = "Financial")]` on `DayEndReconciliation`.
  - `Store.API.Tests/AuditAttributeTests.cs`: Added unit tests verifying `AuditAttribute` constructor property assignments, `CashManagementController` endpoint attribute presence, and successful audit entry dispatch during action filter execution.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests` -> 282 passed, 0 failed.
- Audit tracker updated: `GAP-05` -> `[x]`, `GAP-07` -> `[x]`, `GAP-17` -> `[x]`.

---

## 2026-09-30 — Wave 28 (ToastBus Unification & UX Polish: GAP-20 — completed)

### 28.A — Script Loading & Layout Unification
- **Issue**: `toast-bus.js` was introduced in Wave 4 but was never referenced in `_AppLayout.cshtml` or `_Layout.cshtml`, meaning `window.ToastBus` was never initialized on standard page loads, forcing pages to rely on uncoordinated `window.showToast` implementations.
- **Changes**:
  - `Store.UI/Pages/Shared/_AppLayout.cshtml`: Included `<script src="~/js/toast-bus.js" asp-append-version="true"></script>` right after `site.js`.
  - `Store.UI/Pages/Shared/_Layout.cshtml`: Included `<script src="~/js/toast-bus.js" asp-append-version="true"></script>` right after `site.js`.
  - `Store.UI/Pages/Shared/_Layout.cshtml`: Routed server-rendered `TempData["StatusMessage"]` and `TempData["ErrorMessage"]` to `ToastBus.success('app', ...)` and `ToastBus.error('app', ...)` with fallback to `window.showToast`.

### 28.B — ToastBus Core Robustness & Argument Normalization
- **Issue**: Callers across different Razor Pages and JS files passed arguments in varying order (`(channel, level, message)` vs `(level, message)` vs `(message, level)`). Legacy `window.showToast` in `site.js` broke when callers inverted parameters.
- **Changes**:
  - `Store.UI/wwwroot/js/toast-bus.js`:
    - Updated `ensureContainer()` to guarantee `#toast-container` is present, appending a clean accessible container with `role="status"` and `aria-live="polite"` if missing.
    - Updated `publish(...)` to accept flexible calling signatures (`publish(level, message, opts)` or `publish(channel, level, message, opts)`), defaulting channel to `'app'`.
    - Added channel-specific CSS classes (`toast-channel-${channel}`) on created toasts for visual differentiation.
    - Added helper methods `ToastBus.success(channel, msg, opts)`, `ToastBus.info(...)`, `ToastBus.warning(...)`, `ToastBus.error(...)` supporting both optional and explicit channel arguments.
  - `Store.UI/wwwroot/js/site.js`:
    - Updated `window.showToast` to detect argument inversions (e.g. `showToast('Item saved', 'success')` vs `showToast('success', 'Item saved')`) and auto-normalize them.
    - Auto-creates `#toast-container` if absent in DOM.

### 28.C — Migration of All UI Surfaces & Hubs to ToastBus
- **Changes**:
  - `Store.UI/wwwroot/js/webauthn.js`: Migrated biometric registration and authentication alerts to `ToastBus.success`/`error` on the `'auth'` channel.
  - `Store.UI/wwwroot/js/notifications-hub.js`: Migrated in-app real-time notification toasts to `ToastBus.publish('app', toastLevel, item.message)`.
  - `Store.UI/wwwroot/js/site.js`: Migrated Smart Scanner event notifications (duplicate scan, resume, in-page scan, barcode copied, pause intervals, disabled, resumed) to `ToastBus` on the `'inventory'` and `'app'` channels.
  - `Store.UI/Pages/InventoryOps.cshtml`: Migrated purchase order receipt toasts to `ToastBus.info`/`warning` on the `'inventory'` channel.
  - `Store.UI/Pages/RoleMatrix.cshtml`: Migrated permission matrix toggle success/error toasts to `ToastBus.success`/`error` on the `'admin'` channel.
  - `Store.UI/Pages/Pos.cshtml`: Migrated checkout status feedback (`setMessage`) to `ToastBus.publish('pos', isError ? 'error' : 'success', message)`.
  - `Store.UI/Pages/Payments.cshtml`: Migrated payment polling retry notices, verification results, and errors to `ToastBus` on the `'finance'` channel.
  - `Store.UI/Pages/Invoices.cshtml`: Re-routed internal `showToast` to `ToastBus.publish('finance', type, msg)`.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests` -> 282 passed, 0 failed.
- Audit tracker updated: `GAP-20` -> `[x]`.

---

## 2026-10-01 — Wave 29 (Notification Bell UI & SignalR Pipeline: GAP-25 — completed)

### 29.A — Backend SignalR Notification Triggers for Contact Requests & Low Stock
- **Issue**: `GAP-25` audit finding noted that while Wave 9 added SignalR channels for restock and bulk order events, the UI surface and notification pipeline were missing real-time triggers for contact-request approvals and low-stock threshold crossings.
- **Changes**:
  - `Store.API/Controllers/UsersController.cs`: Injected optional `IRealTimeNotificationService`.
    - In `VerifyContactChange`: Dispatches `StoreNotificationDto` with `Category = NotificationCategory.ContactRequest`, `Severity = "Info"`, `TargetUrl = "/ContactRequests"` to `Manager` and `Admin` role groups.
    - In `ApproveContactChange`: Dispatches `StoreNotificationDto` (`Category = NotificationCategory.ContactRequest`, `Severity = "Success"`, `TargetUrl = "/ContactRequests"`) to `Manager` and `Admin` role groups.
    - In `RejectContactChange`: Dispatches `StoreNotificationDto` (`Category = NotificationCategory.ContactRequest`, `Severity = "Warning"`, `TargetUrl = "/ContactRequests"`) to `Manager` and `Admin` role groups.
  - `Store.DbServices/Services/InvoiceService.cs`: Injected optional `IRealTimeNotificationService`.
    - During sales line stock deduction (`item.InStock -= line.Quantity`), detects if `item.ReorderLevel.HasValue && item.InStock <= item.ReorderLevel.Value`.
    - Following transaction commit, dispatches `NotifyLowStockAsync` with `LowStockAlertDto` across all subscribers, `Manager`/`Admin` role groups, and branch groups.

### 29.B — Activity Center & Notification Bell UI Modernization
- **Changes**:
  - `Store.UI/Pages/Shared/_NotificationCenter.cshtml`: Updated tab filter identifiers to normalized channels: `all`, `approvals`, `inventory`, `security`.
  - `Store.UI/wwwroot/js/notifications-hub.js`:
    - Updated `matchesFilter` logic supporting multi-category groupings (Approvals: `DiscountApproval` + `ContactRequest`; Inventory: `LowStock` + `RestockRecommendation` + `PurchaseOrder`).
    - Added `getCategoryMeta` formatting scoped category tags on each card (`Contact Request`, `Discount Approval`, `Low Stock`, `Restock Alert`, `Purchase Order`, `Security`).
    - Enhanced card interaction: clicking anywhere on a notification card automatically marks it as read and navigates to the target action URL (`data-url`).
    - Routed incoming notification toasts through appropriate ToastBus channels (`inventory` for low stock / restock, `admin` for approvals, `app` for general).
  - `Store.UI/wwwroot/css/notification-center.css`:
    - Added badge styling `.notif-category-badge` with themed color palettes for `.category-contact` (cyan), `.category-approval` (purple), `.category-inventory` (amber), and `.category-security` (rose).

### 29.C — Unit Tests & Verification
- `Store.API.Tests/RealTimeNotificationServiceTests.cs`:
  - Added `NotifyLowStockAsync_SendsToAllAndRoleGroups`: Verifies dispatch to `All`, `role_manager`, `role_admin`, and `branch_{id}` groups.
  - Added `SendToRoleAsync_SendsNotificationToTargetGroup`: Verifies targeting role groups.
- `Store.API.Tests/ContactChangeNotificationTests.cs` (3 new unit tests):
  - `VerifyContactChange_DispatchesNotificationToManagerAndAdmin_WhenTokenValid`
  - `ApproveContactChange_DispatchesSuccessNotificationToRoles`
  - `RejectContactChange_DispatchesWarningNotificationToRoles`

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests` -> 287 passed, 0 failed.
- Audit tracker updated: `GAP-25` -> `[x]`.

---

## 2026-10-01 — Wave 30 (POS PWA & Offline Service Worker Reconciliation: GAP-21 — completed)

### 30.A — Audit & Reconciliation with UX-01
- **Issue**: `GAP-21` ("No PWA / service worker for POS") was originally tracked as open, while `UX-01` had implemented initial service worker support in Wave 16.
- **Verification & Enhancements**:
  - `Store.UI/wwwroot/manifest.json`: Verified PWA manifest containing standalone display mode, orientation, app shortcuts (POS, Invoices, Catalog, Dashboard), SVG/PNG icons, and theme color `#3b82f6`.
  - `Store.UI/wwwroot/sw.js`:
    - Updated `CACHE_VERSION` to `clexan-v2` ensuring immediate cache busting and upgrade activation via `clients.claim()`.
    - Added missing POS style bundles `/css/operations.css` and `/css/modules/sales.css` to `OFFLINE_SHELL` so cold offline launches render with 100% UI fidelity.
    - Verified network-first API fetching with runtime caching for successful GETs, stale-while-revalidate for assets, and navigation fallback to `/Pos`.
  - `Store.UI/wwwroot/js/pwa-install.js`: Verified automated `/sw.js` registration, `beforeinstallprompt` event interception, customizable floating install banner with 7-day cooldown dismissal, manual install context menu binding (`installClexAnApp()`), and `appinstalled` cleanup.
  - `Store.UI/wwwroot/js/pos-offline.js` & `Store.UI/Pages/Pos.cshtml`: Verified `StorePosOfflineDB` IndexedDB catalog caching, customer caching, offline sale queuing, and synchronization orchestration.
  - Layout integration: Verified `<link rel="manifest" href="~/manifest.json" />`, `<link rel="stylesheet" href="~/css/pwa.css" />`, and `pwa-install.js` inclusion in both `_AppLayout.cshtml` and `_Layout.cshtml`.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests` -> 287 passed, 0 failed.
- Audit tracker updated: `GAP-21` -> `[x]`.

---

## 2026-10-01 — Wave 31 (Dynamic Route Breadcrumb Navigation & Back Link Service: GAP-24 — completed)

### 31.A — Centralized Route Hierarchy & Breadcrumb Service
- **Issue**: `GAP-24` audit finding noted that management UI pages lacked hierarchical route breadcrumbs and consistent back navigation, with some pages having ad-hoc placed back buttons (e.g. `/ContactRequests`).
- **Changes**:
  - `Store.UI/Services/IBreadcrumbService.cs`:
    - Defined `BreadcrumbItem` record (`Title`, `Url`, `IsActive`, `Icon`).
    - Defined `IBreadcrumbService` contract (`BuildBreadcrumbs(path, customCrumbs)`, `GetBackLink(currentPath, referer, customBackLink)`).
  - `Store.UI/Services/BreadcrumbService.cs`:
    - Implemented hierarchical mapping of app sections (POS & Sales, Inventory & Catalog, Customers & Loyalty, Purchasing & Suppliers, Financial & Analytics, Management & Admin).
    - Normalized path parsing using `StringComparison.OrdinalIgnoreCase` to strip query parameters while maintaining trail continuity.
    - Added smart parent-chain traversal: builds trail from Home/Dashboard up to the active page.
    - Implemented smart referer-aware back navigation: validates referer origin/path and returns parent fallback URL when direct navigation or external referer occurs.
  - `Store.UI/Program.cs`:
    - Registered `builder.Services.AddSingleton<IBreadcrumbService, BreadcrumbService>();`.

### 31.B — Layout Integration & Responsive Breadcrumb Bar
- **Changes**:
  - `Store.UI/Pages/Shared/_AppLayout.cshtml`:
    - Injected `IBreadcrumbService BreadcrumbService`.
    - Computed `currentPath = Context.Request.Path.Value ?? "/Dashboard"`, `referer = Context.Request.Headers.Referer.FirstOrDefault()`.
    - Integrated `.app-breadcrumbs-bar` between the top navigation bar and main view content.
    - Rendered contextual "Back" button with client-side smart `history.back()` (checking `document.referrer` against `window.location.origin`) and fallback to server-computed back URL.
    - Rendered breadcrumb trail with chevron dividers (`fa-chevron-right`), accessible `aria-label="Breadcrumb"`, and `aria-current="page"` on active leaf crumbs.
    - Suppressed breadcrumbs on root dashboard (`/` or `/Dashboard`) where breadcrumb bar is redundant.
  - `Store.UI/Pages/ContactRequests.cshtml`:
    - Cleaned up redundant ad-hoc back button (`<a href="/Users" class="btn btn-secondary mb-3">...Back to User Accounts</a>`) now natively handled by the layout breadcrumb bar.
  - `Store.UI/wwwroot/css/components.css`:
    - Added styles for `.app-breadcrumbs-bar`, `.breadcrumbs-container`, `.btn-breadcrumb-back`, `.breadcrumb-trail`, `.breadcrumb-sep`, and active/hover states with smooth transitions and mobile overflow support (`overflow-x: auto`).

### 31.C — Unit Tests & Verification
- `Store.API.Tests/Store.API.Tests.csproj`: Added project reference to `Store.UI`.
- `Store.API.Tests/BreadcrumbServiceTests.cs` (7 new unit tests):
  - `BuildBreadcrumbs_RootOrDashboard_ReturnsSingleHomeItem`
  - `BuildBreadcrumbs_NestedPage_ReturnsFullHierarchicalTrail`
  - `BuildBreadcrumbs_MapsKnownPageTitlesAndIcons`
  - `BuildBreadcrumbs_AppendsCustomTrailingCrumbs`
  - `GetBackLink_Dashboard_ReturnsNull`
  - `GetBackLink_SubPage_ReturnsParentHierarchyLink_WhenNoReferer`
  - `GetBackLink_PrefersValidRefererOverParent_WhenRefererIsDifferentPage`
  - `GetBackLink_RespectsCustomBackLinkOverride`

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests` -> 298 passed, 0 failed.
- Audit tracker updated: `GAP-24` -> `[x]`.

---

## 2026-10-01 — Wave 32 (InvoiceService Query Splitting & Line-Item Fetch Optimization: GAP-12 — completed)

### 32.A — Elimination of Cartesian Product Joins in Invoice Retrieval
- **Issue**: `GAP-12` / `DB-02` audit finding identified that `InvoiceService.cs` loaded multi-level navigation trees (`Customer -> Phones -> Phone`, `Customer -> Emails -> Email`, `Customer -> LoyaltyAccount`, `User -> Employee`, `Branch`, `Sales -> Item -> Unit`, `Tenders`) in single queries, causing massive Cartesian joins and slow execution for invoices with multiple line items.
- **Changes in `Store.DbServices/Services/InvoiceService.cs`**:
  - `GetByIdAsync`:
    - Retained split query `.AsSplitQuery()` on invoice header, customer contact navigations, branch, and cashier user/employee.
    - Decoupled `Sales` line items from the header query. Line items are now fetched via a separate targeted query: `_uow.Repository<Sale>().Query().Where(s => s.InvoiceId == invoiceId).AsNoTracking().ToListAsync()`.
    - Removed redundant `.ThenInclude(s => s.Item).ThenInclude(it => it.Unit)` since `ItemName` and `UnitAbbreviation` are snapshot fields persisted directly on `Sale`.
  - `GetPublicReceiptAsync`:
    - Decoupled `Sales` line items into a separate indexed query: `_uow.Repository<Sale>().Query().Where(s => s.InvoiceId == invoiceId).AsNoTracking().ToListAsync()`.
    - Removed `.ThenInclude(s => s.Item)` join; uses persisted `s.ItemName` with fallback.
  - `GetAllAsync`:
    - Refactored `BuildFilteredQuery` to build a clean, lean base query without eager navigation includes on child collections (`Sales`, `Tenders`).
    - Pagination (`Skip`/`Take`) retrieves the filtered slice of invoice headers with reference navigations (`Customer`, `User.Employee`, `Branch`).
    - Fetches child collections for the paged batch in two concurrent index-seek queries:
      `_uow.Repository<Sale>().Query().Where(s => invoiceIds.Contains(s.InvoiceId)).AsNoTracking().ToListAsync()`
      `_uow.Repository<InvoiceTender>().Query().Where(t => invoiceIds.Contains(t.InvoiceId)).AsNoTracking().ToListAsync()`
    - In-memory grouping attaches `Sales` and `Tenders` to their respective `Invoice` entities before mapping to `InvoiceDto`.
  - `GetSummaryMetricsAsync`:
    - Benefits from the cleaned `BuildFilteredQuery`, generating a lightweight SQL aggregation without eager joins to customer contacts or tenders.

### 32.B — Unit Tests & Verification
- `Store.API.Tests/Store.API.Tests.csproj`: Added package reference `Microsoft.EntityFrameworkCore.InMemory` (v8.0.4).
- `Store.API.Tests/InvoiceServiceQuerySplittingTests.cs` (5 new unit tests):
  - `GetByIdAsync_ReturnsInvoiceWithSeparatelyFetchedSalesAndTenders`: Validates header navigations, separate sale and tender retrieval, and DTO mapping.
  - `GetByIdAsync_ReturnsNull_WhenInvoiceDoesNotExist`: Validates not found path.
  - `GetPublicReceiptAsync_ReturnsReceiptWithCorrectCalculations`: Validates public receipt line retrieval and verification signature generation.
  - `GetAllAsync_PaginatesAndAssociatesLinesAndTendersSeparately`: Validates batch line item and tender association across paginated invoices.
  - `GetSummaryMetricsAsync_CalculatesAggregatesAccurately`: Validates lean aggregate calculation.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings in code, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> 303 passed, 0 failed.
- Audit tracker updated: `GAP-12` -> `[x]`.

---

## 2026-10-01 — Wave 33 (Procurement Automation, Forecasting & Order Service Audit: GAP-13 — completed)

### 33.A — Decoupling Monolithic OrderAndOtpService
- **Audit Finding**: `OrderAndOtpService.cs` contained two completely unrelated domain responsibilities in a single 182-line source file: procurement/inventory purchase order lifecycle (`IOrderService`) and security/cryptographic OTP generation and validation (`IOtpService`).
- **Refactoring**:
  - `Store.DbServices/Services/OrderService.cs`: Dedicated implementation for `IOrderService` handling `CreateAsync`, `GetByIdAsync`, `GetAllAsync`, `ReceiveOrderAsync` (with transactional inventory stock level increment), and `CancelOrderAsync`.
  - `Store.DbServices/Services/OtpService.cs`: Dedicated implementation for `IOtpService` handling cryptographic OTP generation with `RandomNumberGenerator` and HMAC-SHA256 hashing keyed by `OtpPepper`.
  - Removed obsolete blended file `Store.DbServices/Services/OrderAndOtpService.cs`.
- **Security Hardening (SEC-06 Alignment)**:
  - Fixed a length mismatch bug in `ValidateAsync` and `PasswordRecoveryService.ValidateOtpAsync`: the code previously compared a 32-byte stored digest against UTF8 bytes of a base64 string (44 bytes).
  - Introduced `HashOtpBytes(string rawCode)` helper returning raw 32-byte HMAC digest; constant-time comparison via `CryptographicOperations.FixedTimeEquals` now correctly compares matching 32-byte arrays.

### 33.B — Surfacing Inventory Threshold Evaluation On-Demand
- **Audit Finding**: `IProcurementAutomationService.EvaluateInventoryThresholdsAsync` was only triggered via Hangfire background worker and lacked an on-demand REST endpoint for branch and warehouse managers to force reorder evaluation during peak seasons or ad-hoc stock resets.
- **Controller Endpoint**:
  - Added `POST /api/purchase-orders/auto-reorder/thresholds` to `PurchaseOrdersController.cs`, protected with `[Authorize(Policy = PermissionKeys.InventoryWrite)]` and audited with `[Audit("Evaluate Inventory Reorder Thresholds", Category = "Inventory")]`.
  - Returns `200 OK` with generated draft purchase order count and order IDs.

### 33.C — Comprehensive Unit Test Suite
- `Store.API.Tests/OrderAndOtpServiceTests.cs` (6 tests):
  - `CreateAsync_CalculatesLineTotalsAndCreatesPendingOrder`
  - `CreateAsync_ThrowsArgumentException_WhenNoItemsProvided`
  - `ReceiveOrderAsync_IncrementsStockAndMarksStatusReceived`
  - `CancelOrderAsync_ThrowsInvalidOperationException_WhenAlreadyReceived`
  - `GenerateAsync_InvalidatesPreviousOtps_AndReturnsHashedDigest`
  - `ValidateAsync_ReturnsTrue_ForValidCode_AndMarksOtpUsed`
  - `ValidateAsync_ReturnsFalse_ForWrongCode`
- `Store.API.Tests/ProcurementAutomationServiceTests.cs` (3 tests):
  - `EvaluateInventoryThresholdsAsync_WhenStockBelowThreshold_GeneratesDraftPurchaseOrders`
  - `EvaluateInventoryThresholdsAsync_WhenStockAdequate_DoesNotGenerateOrders`
  - `EvaluateInventoryThresholdsAsync_IgnoresInactiveItems`
- `Store.API.Tests/DemandForecastingServiceTests.cs` (4 tests):
  - `RunDemandForecastingAsync_CalculatesDailyVelocityAndGeneratesRecommendations`
  - `GetPendingRecommendationsAsync_FiltersByBranchId`
  - `ConvertToStockTransferAsync_CreatesStockTransfer_WhenWarehouseConfigured`
  - `ConvertToPurchaseOrderAsync_ResolvesSupplier_ViaPreferredOrPurchaseHistory`

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (0 warnings, 0 errors).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> 317 passed, 0 failed.
- Audit tracker updated: `GAP-13` -> `[x]`.

---

## 2026-10-01 — Wave 34 (Lookup Endpoints Consolidation & Separation of Concerns: GAP-14 — completed)

### 34.A — Elimination of Monolithic Lookup Files
- **Audit Finding**: `GAP-14` identified multiple violations of the Single Responsibility Principle and Clean Architecture conventions:
  - `LookupControllers.cs` bundled 5 controllers (`CategoriesController`, `UnitsController`, `DepartmentsController`, `SalariesController`, `CountriesController`) and inline request DTOs into a single 229-line source file.
  - `LookupServices.cs` bundled 4 unrelated domain services (`CategoryService`, `UnitService`, `DepartmentService`, `SalaryService`) into a single 203-line file.
  - `ILookupServices.cs` bundled 4 distinct service interfaces into one file.
- **Refactoring & File Separation**:
  - **DTOs**:
    - `Store.Models/DTOs/Items/CategoryDtos.cs`: `CreateCategoryRequest`, `UpdateCategoryRequest` with validation attributes (`[Required]`, `[StringLength]`).
    - `Store.Models/DTOs/Items/UnitDtos.cs`: `CreateUnitRequest`, `UpdateUnitRequest` with validation attributes.
    - `Store.Models/DTOs/HR/DepartmentDtos.cs`: `CreateDepartmentRequest`, `UpdateDepartmentRequest`, and backward-compatible `CreateLookupRequest` alias.
  - **Interfaces**:
    - `Store.Models/Interfaces/Services/ICategoryService.cs`
    - `Store.Models/Interfaces/Services/IUnitService.cs`
    - `Store.Models/Interfaces/Services/IDepartmentService.cs`
    - `Store.Models/Interfaces/Services/ISalaryService.cs`
    - Removed `Store.Models/Interfaces/Services/ILookupServices.cs`.
  - **Services**:
    - `Store.DbServices/Services/CategoryService.cs`
    - `Store.DbServices/Services/UnitService.cs`
    - `Store.DbServices/Services/DepartmentService.cs`
    - `Store.DbServices/Services/SalaryService.cs`
    - Removed `Store.DbServices/Services/LookupServices.cs`.
  - **Controllers**:
    - `Store.API/Controllers/CategoriesController.cs`
    - `Store.API/Controllers/UnitsController.cs`
    - `Store.API/Controllers/DepartmentsController.cs`
    - `Store.API/Controllers/SalariesController.cs`
    - `Store.API/Controllers/CountriesController.cs`
    - Removed `Store.API/Controllers/LookupControllers.cs`.

### 34.B — Service Hardening & Foreign Key Integrity
- **Update Duplicate Checks**:
  - `CategoryService.UpdateAsync`: Added duplicate name check `ExistsAsync(c => c.Name == name && c.CategoryId != id)` throwing `InvalidOperationException`.
  - `UnitService.UpdateAsync`: Added duplicate abbreviation check `ExistsAsync(u => u.Abbreviation == abbreviation && u.UnitId != id)` throwing `InvalidOperationException`.
  - `DepartmentService.UpdateAsync`: Added duplicate name check `ExistsAsync(d => d.Name == name && d.DepartmentId != id)` throwing `InvalidOperationException`.
- **Delete FK Dependency Protection**:
  - `CategoryService.DeleteAsync`: Added pre-check preventing deletion if referenced by `Item` or `ItemCategory`, throwing clear `InvalidOperationException` instead of letting MySQL fail with a raw FK constraint exception.
  - `UnitService.DeleteAsync`: Added pre-check preventing deletion if referenced by `Item`.
  - `DepartmentService.DeleteAsync`: Added pre-check preventing deletion if referenced by `Employee`.
- **API Controller Error Mapping & Audit Trails**:
  - Decorated all mutating actions (`POST`, `PUT`, `DELETE`) with `[Audit]` and respective permissions (`InventoryWrite`, `EmployeeCreate`, `EmployeeUpdate`, `PayrollWrite`, `AdminSystem`).
  - Standardized error returns: mapped `InvalidOperationException` to HTTP 409 Conflict with `ApiErrorResponse.From(ErrorCode.Conflict, ...)`.
  - Standardized 404 responses with `ErrorCode.NotFound`.
  - Added `GetById` to `UnitsController` and `DepartmentsController` for API consistency.
  - Added `GetByIsoCode` and `GetDefault` to `CountriesController`.

### 34.C — Comprehensive Unit Test Suites
- Added 36 new unit tests across 5 test suites:
  - `Store.API.Tests/CategoryServiceTests.cs` (8 tests): GetAll sorting, GetById, Create, duplicate name check, Update, update conflict check, Delete, in-use FK guard.
  - `Store.API.Tests/UnitServiceTests.cs` (8 tests): GetAll sorting, GetById, Create, duplicate abbreviation check, Update, update abbreviation conflict check, Delete, in-use FK guard.
  - `Store.API.Tests/DepartmentServiceTests.cs` (8 tests): GetAll sorting, GetById, Create, duplicate name check, Update, update name conflict check, Delete, in-use employee FK guard.
  - `Store.API.Tests/SalaryServiceTests.cs` (8 tests): GetAll sorting, GetById, Create, duplicate grade check, Update, update grade conflict check, Delete, in-use employee FK guard.
  - `Store.API.Tests/CountriesControllerTests.cs` (4 tests): GetAll, GetByIsoCode found, GetByIsoCode 404, GetDefault country.
- Fixed CS8604 compiler warning in `WebAuthnDevicesEndpointsTests.cs` (`Assert.NotNull(payload.Data)`).

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (**0 warnings, 0 errors**).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> **353 passed, 0 failed**.
- Audit tracker updated: `GAP-14` -> `[x]`.

---

## 2026-10-01 — Wave 35 (OAuth State Server-Side Binding Verification & Anti-CSRF: SEC-10, MT-10 — completed)

### 35.A — Server-Side Nonce Registry & Single-Use Replay Protection
- **Audit Finding**: `SEC-10` / `MT-10` identified that OAuth state validation relied purely on stateless HMAC verification (`{tenantId}:{timestamp}:{signature}`). While the signature proved the state originated from the server, it lacked server-side binding and single-use enforcement:
  - An attacker could capture and replay an intercepted state parameter within the 10-minute validity window.
  - A state generated for one tenant session could be injected into a victim's flow (Login CSRF / OAuth account linking fixation) without verification that the callback originated from the user-agent that initiated the flow.
- **Implementation**:
  - `Store.TenantPortal/Services/OAuthService.cs`:
    - Upgraded state generation to a 4-part payload: `{tenantId:D}:{timestamp}:{nonce}:{signature}` using a 16-byte cryptographically secure random nonce (`RandomNumberGenerator.GetBytes(16)`).
    - Introduced a thread-safe server-side registry (`ConcurrentDictionary<string, OAuthStateEntry>`) holding `(TenantId, ExpiresAt)`.
    - Implemented atomic one-time consumption via `_activeStates.TryRemove(nonce, out var recordedEntry)`. Once a state is validated, its entry is permanently consumed, defeating replay attacks.
    - Added automatic pruning of expired entries during generation (`PruneExpiredStates()`).
    - Made testing helpers (`ActiveStateCount`, `ClearActiveStates()`) accessible for unit test fixtures.

### 35.B — User-Agent Session Binding via Anti-CSRF Cookie
- **Cookie Binding**:
  - During `GenerateSignedState(tenantId, httpContext)`, an `HttpOnly`, `SameSite=Lax`, `Secure` cookie (`clexan_oauth_state_nonce`) is appended to the response containing the nonce with a 10-minute expiry. `SameSite=Lax` ensures the cookie is supplied across top-level redirects returning from external IdPs (Microsoft / Google).
  - During `ValidateSignedState(state, httpContext, out tenantId)`, the request cookie nonce is retrieved and verified against the state nonce using constant-time comparison (`CryptographicOperations.FixedTimeEquals`).
  - On validation completion, the cookie is deleted to ensure zero lingering state.
  - Added overload `IOAuthService.GenerateSignedState(Guid, HttpContext?)` and `IOAuthService.ValidateSignedState(string, HttpContext?, out Guid)`.
  - Retained fallback for legacy 3-part states when `HttpContext` is null for backward compatibility.
- **Plumbing**:
  - `Store.TenantPortal/Program.cs`: Registered `builder.Services.AddHttpContextAccessor()`.
  - Updated all OAuth connection and callback Razor page-models to supply `HttpContext`:
    - `Store.TenantPortal/Pages/Dashboard/OAuth/Connect.cshtml.cs`: Passes `HttpContext` to `GenerateSignedState`.
    - `Store.TenantPortal/Pages/Dashboard/OAuth/Callback.cshtml.cs`: Passes `HttpContext` to `ValidateSignedState`.
    - `Store.TenantPortal/Pages/OAuth/MicrosoftCallback.cshtml.cs`: Passes `HttpContext` to `ValidateSignedState`.
    - `Store.TenantPortal/Pages/OAuth/GoogleCallback.cshtml.cs`: Passes `HttpContext` to `ValidateSignedState`.

### 35.C — Comprehensive Security Unit Test Suite
- `Store.API.Tests/Store.API.Tests.csproj`: Added project reference to `Store.TenantPortal`.
- `Store.API.Tests/OAuthServiceTests.cs` (9 new security unit tests):
  - `GenerateSignedState_FormatsFourPartPayload_AndSetsStateCookie`: Verifies 4-part structure, HMAC validity, server registry registration, and `HttpOnly`/`SameSite=Lax`/`Secure` cookie headers.
  - `ValidateSignedState_ValidCookieAndState_ReturnsTrue_AndConsumesNonce`: Verifies end-to-end happy path and cookie deletion.
  - `ValidateSignedState_ReplayAttack_ReturnsFalseOnSecondAttempt`: Verifies atomic `TryRemove` rejects replay attempts with the same state parameter.
  - `ValidateSignedState_TamperedSignature_ReturnsFalse`: Verifies signature tampering is rejected.
  - `ValidateSignedState_CookieMismatch_ReturnsFalse`: Verifies anti-CSRF check rejects mismatched session cookies.
  - `ValidateSignedState_MissingCookie_ReturnsFalse`: Verifies anti-CSRF check rejects requests when state cookie is absent.
  - `ValidateSignedState_UnissuedForgedNonce_ReturnsFalse`: Verifies forged state with unissued nonce is rejected by server registry.
  - `ValidateSignedState_ExpiredTimestamp_ReturnsFalse`: Verifies 10-minute expiry window rejection.
  - `BuildAuthUrls_IncludesRequiredParametersAndState`: Verifies Microsoft and Google authorization URL builders include proper client IDs, scopes, redirect URIs, and escaped state.

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (**0 warnings, 0 errors**).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> **362 passed, 0 failed** (+9 new security unit tests).
- Audit tracker updated: `SEC-10` -> `[x]`, `MT-10` -> `[x]`.

---

## 2026-10-01 — Wave 36 (Hardcoded VPS IP Elimination & Compose Config Hardening: OPS-10 — completed)

### 36.A — Parameterization of Store & API Domains
- **Audit Finding**: `OPS-10` / `full_codebase_audit.md § 328` noted that the production compose file `docker-compose.prod.yml` hardcoded a VPS IP fallback (`157.173.112.19.nip.io`) across CORS origins, Traefik routing rules, and external API URLs. IP-based routing is brittle — migrating to a new VPS or domain breaks all routing and CORS headers.
- **Compose Hardening (`docker-compose.prod.yml`)**:
  - Replaced hardcoded IP fallbacks with strict required environment variables:
    - `STORE_DOMAIN`: `${STORE_DOMAIN:?STORE_DOMAIN is required}` applied to `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, Traefik UI router rule (`Host(...)`), and `/files` routing rule.
    - `API_DOMAIN`: `${API_DOMAIN:?API_DOMAIN is required}` applied to Traefik API router rule (`Host(...)`) and `ApiSettings__ExternalBaseUrl`.
    - `HTTP_PORT` / `HTTPS_PORT`: Configured with sensible defaults (`${HTTP_PORT:-18080}`, `${HTTPS_PORT:-18443}`) for CORS and UI external API URLs.
  - Hardened database credentials and application secrets to enforce strict required syntax `${VAR:?VAR is required}` per AGENTS.md § 4, eliminating insecure fallback defaults (`root`, `rootpassword`, `admin`, `adminpassword`):
    - `MYSQL_ROOT_PASSWORD`, `MYSQL_USER`, `MYSQL_PASSWORD`
    - `MONGO_INITDB_ROOT_USERNAME`, `MONGO_INITDB_ROOT_PASSWORD`
    - `STORE_CONNECTION_STRING`, `STORE_MONGO_CONNECTION_STRING`
    - `STORE_JWT_SECRET`, `STORE_OTP_PEPPER`, `STORE_MOMO_CALLBACK_KEY`

### 36.B — Data & Documentation Cleansing
- **ControlPlane App_Data (`Store.ControlPlane/App_Data/tenants.json`)**:
  - Cleaned up demo tenant `acmefoods`: updated `UiUrl` and `ApiUrl` from `157.173.112.19.nip.io` to `acmefoods.store.127.0.0.1.nip.io:18080` and `api.acmefoods.store.127.0.0.1.nip.io:18080`, matching the rest of the local seed tenants.
- **Tenant Portal Architecture & Specifications**:
  - `docs/tenant-portal/01-technical-spec.md`: Replaced hardcoded IP references with `store.yourcompany.com` and `portal.store.yourcompany.com`.
  - `docs/tenant-portal/multi-tenant-architecture.md`: Replaced hardcoded IP examples in URL scheme table and JSON responses with canonical `store.example.com`.

### 36.C — Comprehensive Environment Variable Template (`.env.example`)
- Expanded root `.env.example` into a well-documented 6-section template:
  1. Edge Proxy (Traefik) configuration (`TRAEFIK_API_ENABLED`, `TRAEFIK_DASHBOARD_PORT`).
  2. Production Domain & Networking Settings (`STORE_DOMAIN`, `API_DOMAIN`, `ROOT_DOMAIN`, `HTTP_PORT`, `HTTPS_PORT`).
  3. Platform & ControlPlane Orchestration Settings (`IMAGE_PREFIX`, `IMAGE_TAG`, database and key overrides).
  4. Production Tenant Stack Database Credentials & Connection Strings.
  5. Application Security & Cryptographic Secrets (`STORE_JWT_SECRET`, `STORE_OTP_PEPPER`, `STORE_MOMO_CALLBACK_KEY`).
  6. Database Backup & Cloud Storage Configuration (`BACKUP_RETENTION_DAYS`, `MYSQL_BACKUP_CRON`, `MONGO_BACKUP_CRON`, S3 settings).

### Verification
- Compose syntax validated: `docker compose -f docker-compose.prod.yml config` passes cleanly with environment variables provided, and strictly fails with clear errors when required variables are missing.
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (**0 warnings, 0 errors**).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> **362 passed, 0 failed**.
- Audit tracker updated: `OPS-10` -> `[x]`.

---

## 2026-10-01 — Wave 37 (File Upload Security, Path Traversal Hardening & MIME Validation: SEC-12 — completed)

### 37.A — Path Traversal & Symlink Defense-in-Depth
- **Audit Finding**: `SEC-12` required verifying that `FilesController.cs` and `LocalFileStorageService.cs` use `OrdinalIgnoreCase` on all path-traversal checks, reject tildes (`~`) and Windows NTFS alternate data streams (`:`), and prevent operations targeting symbolic links, junctions, or reparse points.
- **Controller Hardening (`Store.API/Controllers/FilesController.cs`)**:
  - `UploadFile`:
    - Safe folder validation upgraded to `StringComparison.OrdinalIgnoreCase`.
    - Added explicit rejection of `~`, `:`, invalid path characters, and relative segment traversal (`..`, `.`).
    - Added strict validation of `file.FileName` preventing directory traversal (`..`, `~`, `:`, path separators, and invalid filename characters).
    - Added typed image extension allowlist (`.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`) checking `Path.GetExtension(file.FileName)`.
    - Added binary magic byte inspection (`HasValidImageHeader`) validating JPEG (`FF D8 FF`), PNG (`89 50 4E 47 0D 0A 1A 0A`), GIF (`GIF87a`/`GIF89a`), and WebP (`RIFF....WEBP`) before invoking downstream processors.
    - Specifically caught ImageSharp `UnknownImageFormatException` and `InvalidImageContentException` to return HTTP 400 Bad Request instead of unhandled 500 errors.
  - `DeleteFile`:
    - Normalized path and enforced `StringComparison.OrdinalIgnoreCase` on traversal checks.
    - Explicitly rejected paths containing `~`, `:`, rooted paths, or invalid path characters.
    - Enforced folder allowlist across root segment and all child segments.
- **Storage Service Hardening (`Store.API/Infrastructure/Storage/LocalFileStorageService.cs`)**:
  - `ResolveSafePath`:
    - Enforced `StringComparison.OrdinalIgnoreCase` on `..` checks, rejected `~`, `:`, rooted paths, and invalid path characters.
  - `DeleteFile`:
    - Added `FileInfo` reparse point / link inspection: rejects deletion if `fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)` or `!string.IsNullOrEmpty(fileInfo.LinkTarget)` to prevent deletion of target files through malicious symlinks.
  - `ResolveSafeDirectoryPath`:
    - Created safe directory resolution helper validating that target subfolders stay strictly within `_basePath` and do not resolve to or traverse through symbolic links or reparse point directories.
  - Extension sanitization: Sanitized output extensions in `SaveFileAsync` and `SaveStreamAsync` to prevent double-extension or traversal exploits.

### 37.B — Comprehensive Security Unit Test Suite
- `Store.API.Tests/FilesControllerSecurityTests.cs` (21 tests across upload, delete, and storage):
  - `UploadFile_NullOrEmptyFile_ReturnsBadRequest`
  - `UploadFile_ExceedsSizeLimit_ReturnsBadRequest`
  - `UploadFile_InvalidOrTraversalFolder_ReturnsBadRequest` (tests `../secret`, `..\secret`, `misc/../../etc`, `~/uploads`, `misc/~`, `misc:stream`, `unauthorized_folder`, `admin`)
  - `UploadFile_PathTraversalFileName_ReturnsBadRequest` (tests `../evil.png`, `..\evil.png`, `~evil.png`, `evil:stream.png`, `evil\0.png`)
  - `UploadFile_DisallowedExtension_ReturnsBadRequest` (tests `.php`, `.sh`, `.exe`, `.pdf`, `.svg`)
  - `UploadFile_DisallowedContentType_ReturnsBadRequest` (tests `application/x-msdownload`, `text/html`, `application/javascript`, `application/octet-stream`)
  - `UploadFile_SpoofedMagicBytes_ReturnsBadRequest` (tests file with `.jpg` extension and `image/jpeg` header but executable/ASCII binary payload)
  - `UploadFile_AntivirusFlagged_Returns415UnsupportedMediaType` (tests ClamAV virus scan rejection and 415 response with threat signature)
  - `UploadFile_ValidCleanImage_ProcessesAndReturnsUrls` (tests valid JPEG upload, AV pass, thumbnail and full image webp generation)
  - `DeleteFile_EmptyPath_ReturnsBadRequest` (tests empty and whitespace paths)
  - `DeleteFile_PathTraversalOrInvalidFolder_ReturnsBadRequest` (tests `../secret.webp`, `..\secret.webp`, `/files/users/../../etc/passwd`, `/files/users/~/evil.png`, `/files/users/stream:colon`, `/etc/passwd`, `C:/Windows/system.ini`, `/files/unauthorized_folder/file.webp`)
  - `DeleteFile_ValidPath_CallsStorageDeleteAndReturnsOk`
  - `LocalFileStorageService_ResolveSafePath_PreventsTraversal`

### Verification
- Solution build clean: `dotnet build StoreProject.sln --configuration Release` (**0 warnings, 0 errors**).
- Test suite passes: `dotnet test Store.API.Tests -c Release` -> **401 passed, 0 failed** (+39 new security and functional assertions).
- Audit tracker updated: `SEC-12` -> `[x]`.








