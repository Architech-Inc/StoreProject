# Batch & Expiry Tracking Hub — Deep Dive Analysis & Systems Design Report
### Cross-referenced against the Design System Specification, Clean Architecture, and Suite-Wide Benchmarks
---

## 1. Executive Summary & Current State

The **Batch & Expiry Tracking Hub** (`/BatchTracking`) is the mission-critical food safety and shelf-life compliance module within ClexAn Foods. It is designed to track production lot numbers, supplier batch codes, received dates, cost valuations, and expiration timelines to prevent spoilage, reduce stock losses, and ensure FEFO (First-Expired, First-Out) inventory integrity.

### Current Health Score: ~30%
The current page is a legacy prototype:
- **Layout & Visuals**: Uses deprecated `.ops-page`, zero KPI cards, hardcoded legacy currency symbols (`GHS` instead of suite-standard `XAF`), raw emoji action buttons (`✏️`, `🗑️`), and un-styled filter selects.
- **Architecture**: `BatchTrackingModel` handles raw HTTP calls and validation without an application-level manager service.
- **Functional Gaps**: Lacks search filtering, pagination, batch valuation metrics, direct expired-to-wastage write-off workflows, CSV audit exports, and batch shelf-life countdown progress indicators.

---

## 2. UI/UX & Design System Gaps (vs. `docs/design_system_specification.md`)

### 2.1 Missing Standard KPI Banner (Design Spec §5.1)
**Current state:** A simple red status banner `⚠️ @Model.ExpiringBatches.Count batch(es) expiring within next 30 days.` without comprehensive inventory metrics.

**Required 4-Card KPI Banner:**
1. **Expiring Batches (≤ 30 Days)** (Amber/Warning icon): Total batches approaching shelf-life cutoff.
2. **Expired Batches (Action Required)** (Red/Danger icon): Overdue batches requiring immediate quarantine or write-off.
3. **Active Batches / Units** (Emerald/Brand icon): Total healthy batches and total units tracked.
4. **Total Batch Stock Valuation** (Teal/Valuation icon): Monetary value of stock locked in tracked batches (`XAF`).

*Each card must support interactive click-to-filter.*

---

### 2.2 Currency Inconsistency
**Current state:** Lines 74 and 132 in `BatchTracking.cshtml` hardcode `GHS @b.CostPrice.ToString("N2")` and `Cost Price (GHS)`.
**Fix:** Standardize to suite-wide **`XAF`** (Central African CFA franc) currency formatting.

---

### 2.3 Filter Dock & Search Deficiencies (Design Spec §5.2)
**Current state:** A single `<select name="expiryStatus">` with a raw `<button>` submitting a full page reload.
**Required Filter Dock:**
- Search box with magnifying glass SVG icon (search by Batch Number, Item Name, Barcode, or Supplier Lot).
- Status filter pills:
  - `All Batches`
  - `Expiring Soon (≤30d)` (`.badge-warning`)
  - `Expired` (`.badge-danger`)
  - `Healthy / Active` (`.badge-success`)
- `📥 Export CSV` button for audit compliance.
- Primary CTA buttons: `+ Record Batch` and `⚡ Write-off Expired`.

---

### 2.4 Data Table & Shelf-Life Ergonomics (Design Spec §5.3)
**Current state:** Basic HTML table with raw dates and emoji buttons (`✏️`, `🗑️`).

**Required Table Enhancements:**
1. **Batch & Item Composite Cell**: Monospace `BatchNumber` pill, bold item name, item category badge, and barcode.
2. **Shelf-Life Countdown Indicator**:
   - `Expired (Xd ago)`: `.badge-danger`
   - `Expiring (Xd remaining)`: `.badge-warning` with countdown
   - `Healthy (Xd)`: `.badge-success`
3. **Batch Valuation**: Unit cost + Total line valuation (`Qty × Unit Cost` in `XAF`).
4. **Action Buttons**: Standardized `.btn-table-action`:
   - `✏️ Edit`: Opens modern edit modal with pre-populated values.
   - `🗑️ Write-off to Wastage`: Directly writes off expired/damaged batch into the Wastage system.
   - `🗑️ Delete`: Secure delete with confirmation dialog.

---

### 2.5 Modals & Blades Modernization (Design Spec §6)
**Current state:** Modals toggle `hidden` attribute without backdrop blur or animations.
**Required Upgrades:**
1. **Record Batch Modal / Blade**:
   - Smart Item Lookup with real-time cost price autofill.
   - Batch Number generator / custom lot entry.
   - Received Date + Expiry Date with smart presets (+30d, +90d, +180d, +1yr).
   - Quantity & Unit Cost with live **Batch Total Valuation calculator** (`Quantity × Unit Cost = Total XAF`).
   - Notes & Storage Condition / Location field.
2. **Edit Batch Modal**:
   - Live adjustment of quantity, cost price, expiry date, and notes.
3. **Direct Write-Off Modal (Cross-Module Synergy)**:
   - One-click trigger from an expired batch row that pre-populates a Wastage record (`WastageType.Expired`, quantity, and notes) and deducts batch stock!

---

## 3. Systems Design & Clean Architecture (Dennis Ritchie & Uncle Bob)

### 3.1 Principle of Orthogonality & SRP (Uncle Bob)
- **Violation**: Presentation model handles API communication, validation, and serialization.
- **Solution**: Introduce `IBatchManager` and `BatchManager` in `Store.UI/Services/`:
  - Encapsulates `GetAllBatchesAsync`, `GetExpiringBatchesAsync`, `GetBatchMetricsAsync`, `CreateBatchAsync`, `UpdateBatchAsync`, `DeleteBatchAsync`, and `WriteOffBatchAsync`.
  - Encapsulates error handling and user security context.
  - Keeps `BatchTrackingModel` thin and decoupled.

### 3.2 Dennis Ritchie Systems Design: Orthogonal Tooling & Unix Philosophy
- Integrate Batch Tracking with the **Wastage Service** (`IWastageService`) and **Inventory Operations** (`IStoreOperationsService`) so batch adjustments automatically synchronize inventory movements and write-off logs.

---

## 4. Implementation Matrix — Batch Tracking vs Benchmarks

| Feature | Inventory Ops | Employees | Invoices | **Batch Tracking (Current)** | **Batch Tracking (Proposed)** |
|---|---|---|---|---|---|
| 4-Card KPI Banner | ✅ | ✅ | ✅ | ❌ None | ✅ 4 Color-Coded KPI Cards |
| Search & Filter Dock | ✅ | ✅ | ✅ | ❌ Basic Select | ✅ Filter Dock with SVG search |
| Shelf-Life Countdown Badge | N/A | N/A | N/A | ⚠️ Raw text | ✅ Color-coded countdown badges |
| Currency Standard (`XAF`) | ✅ | ✅ | ✅ | ❌ Hardcoded `GHS` | ✅ Standardized `XAF` |
| Direct Expired-to-Wastage Action | N/A | N/A | N/A | ❌ None | ✅ `⚡ Write-off to Wastage` |
| Live Batch Valuation Simulator | ✅ | N/A | ✅ | ❌ None | ✅ Live `Qty × Cost` calculator |
| CSV Audit Export | ✅ | ✅ | ✅ | ❌ None | ✅ `📥 Export CSV` button |
| Clean Application Manager Layer | ✅ | ✅ | ✅ | ❌ Fat Model | ✅ `IBatchManager` |

---

## 5. Prioritized Action Plan

### Phase 1: Application Architecture & Domain DTOs
1. **DTO Extensions** in `Store.Models/DTOs/Inventory/BatchDtos.cs`:
   - `BatchMetricsDto`: `TotalBatches`, `TotalExpiring30Days`, `TotalExpired`, `TotalTrackedUnits`, `TotalBatchValuationXaf`.
   - `BatchFilterRequest`: Extends `PagedRequest` with `Guid? ItemId`, `string? ExpiryStatus`, `DateTime? FromExpiry`, `DateTime? ToExpiry`.
   - Enrich `BatchDto` with `CategoryName`, `TotalValuation`, `Barcode`.
2. **Core Service & Controller**:
   - Implement `GetBatchMetricsAsync` in `BatchService.cs` and expose `[HttpGet("metrics")]` on `BatchesController.cs`.
   - Enhance `GetAllAsync` in `BatchService.cs` to support search query and date filtering.
3. **Application Orchestration Layer**:
   - Create `IBatchManager` and `BatchManager` in `Store.UI/Services/`.
   - Register in DI container (`Program.cs`).

### Phase 2: High-Density UI/UX Overhaul (`BatchTracking.cshtml`)
4. **KPI Grid**: 4 interactive cards (Expiring Soon, Expired, Active Units, Total Valuation).
5. **Toolbar & Filter Dock**: Search box, status filter pills (`All`, `Expiring`, `Expired`, `OK`), Export CSV, and primary CTA buttons.
6. **Batches Data Table**:
   - Composite batch number, item name, barcode, and category.
   - Shelf-life countdown status badge (`Expiring in 14d`, `Expired 3d ago`, `OK`).
   - Line valuation in `XAF`.
   - Action buttons: `✏️ Edit`, `⚡ Write-off`, `🗑️ Delete`.
7. **Interactive Modals**:
   - **Record Batch Modal**: Smart item lookup, cost prefill, dynamic line total calculator, expiry presets.
   - **Edit Batch Modal**: Live stock and date modification.
   - **Direct Write-off to Wastage Modal**: Seamless write-off of expired stock.

### Phase 3: Verification & Micro-Interactions
8. Verify clean compilation across the solution.
9. Verify interactive modals, shelf-life calculations, and CSV export.
