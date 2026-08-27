# Inventory Operations Hub — Deep Dive Analysis & Systems Design Report
### Cross-referenced against the Design System Specification, Clean Architecture, and Suite-Wide Benchmarks
---

## 1. Executive Summary & Current State

The **Inventory Operations Hub** (`/InventoryOps`) is a core operational console within ClexAn Foods, intended for warehouse receiving (GRN), customer/supplier stock returns, manual stock adjustments (shrinkage, damage, count variance), low-stock reorder monitoring, and stock movement auditing.

### Current Health Score: ~25%
The current page is an early prototype implementation that severely violates design system standards and Clean Architecture principles:
- **Layout & Design**: Uses deprecated `<div class="ops-page">` and anchor jump links (`<nav class="page-jump">`).
- **Visuals**: Zero KPI cards, unstyled raw inputs, unstyled buttons, static forms squeezed into a 3-column card grid, raw tables with no pagination or sorting.
- **Architecture**: `InventoryOpsModel` combines presentation, HTTP client operations, direct API calls, and business validation (violating SRP).
- **Missing Workflows**: No multi-item GRN receipt, no live stock simulation in adjustments, no reorder-to-PO conversion, and no movement audit export.

---

## 2. UI/UX & Design System Gaps (vs. `docs/design_system_specification.md`)

### 2.1 Missing Standard KPI Banner (Design Spec §5.1)
**Current state:** Zero metric cards. The user lands directly on jump links and raw tables without operational visibility.

**Required 4-Card KPI Banner:**
1. **Low Stock Alerts** (Amber/Warning icon): Total SKUs at or below reorder threshold.
2. **Stock Movements (24h / MTD)** (Teal/Info icon): Total stock transaction volume.
3. **Goods Inward (GRN)** (Brand Green icon): Total received units / valuation.
4. **Returns & Adjustments** (Purple/Red icon): Net variance and returned units.

*Each card must feature the standard `.kpi-icon` + `.kpi-info` (`.kpi-label`, `.kpi-val`, `.kpi-sub`) structure with interactive filter clickability.*

---

### 2.2 Broken Action Paradigm ("Stock Actions" 3-Column Static Grid)
**Current state:** Lines 49–97 of `InventoryOps.cshtml` render 3 static form cards side-by-side (`Goods Receipt`, `Stock Return`, `Stock Adjustment`).
- Forms take up excessive vertical canvas space.
- No live on-hand stock indicators.
- No dynamic price/valuation calculation.
- Cluttered interface for everyday auditing.

**Design System Benchmark Solution:**
Replace static forms with **Modern Action Modals / Slide-Over Blades**:
1. **Goods Receipt Note (GRN) Modal/Blade**: Multi-line item entry, Supplier selection, PO reference code, Unit cost, Batch number (optional), live subtotal valuation.
2. **Quick Stock Adjustment Modal** *(matching screenshot benchmark `Screenshot_..._13145`)*:
   - Live Current On-Hand Stock banner (`120 bottle`).
   - Adjustment Quantity input (`+5` or `-2`).
   - Reason Code dropdown (`Physical Count Correction`, `Damaged / Broken`, `Expired Product`, `Internal Transfer / Sampling`, `Theft / Loss`, `Data Entry Correction`).
   - Live New Stock Level simulation.
   - Cancel button (`.btn-secondary`) + Save Adjustment CTA (`.btn-primary`).
3. **Stock Return Modal**:
   - Item search with live stock preview.
   - Invoice Reference link.
   - Return Quantity + Reason selector.

---

### 2.3 Low Stock Reorder Monitoring — Actionability Gaps
**Current state:** A static HTML table listing items with low stock. No visual indicators, no urgency highlighting, and no actions.

**Gaps vs Design System:**
1. **Stock Progress Bar**: Visual gauge showing `Current Stock` vs `Reorder Level`.
2. **Urgency Status Badges**: `.badge-danger` for Out of Stock (`0` units), `.badge-warning` for Low Stock.
3. **Action Buttons**:
   - `⚡ Quick PO`: Pre-populates a Purchase Order draft with the suggested order quantity.
   - `⚡ Adjust`: Opens the Quick Adjustment modal.
   - `👁️ 360`: Opens the Product 360 blade.

---

### 2.4 Stock Movement Audit Stream — Data Dense & Ergonomic Gaps
**Current state:** An unpaginated table dumping raw movements with unstyled headers and raw ISO UTC timestamps.

**Gaps vs Invoices / Loyalty Stream Benchmarks:**
1. **Semantic Movement Type Badges**:
   - `Receive / GRN`: `.badge-success` (`+20 GRN`)
   - `Return`: `.badge-teal` (`+5 Return`)
   - `Adjustment (+)`: `.badge-success` (`+10 Audit`)
   - `Adjustment (-)`: `.badge-danger` (`-4 Spoilage`)
   - `Transfer`: `.badge-neutral` (`Transfer`)
   - `Wastage / Scrap`: `.badge-danger` (`Wastage`)
2. **Delta & Stock Change Visualization**:
   - Green bold for positive deltas (`+25`), Red bold for negative deltas (`-5`).
   - `Before ➔ After` stock column pill (`120 ➔ 145`).
3. **User Identity**: Avatar/Initials pill for `PerformedByUserName`.
4. **Toolbar & Filter Controls**:
   - Search box by item name, reference code, or reason.
   - Movement Type filter pills (`All`, `Receipts`, `Adjustments`, `Returns`, `Transfers`).
   - Date range selector / quick presets (`Today`, `Yesterday`, `Last 7d`, `This Month`).
   - `📥 Export CSV` button for audit compliance.
5. **Pagination**: Standard `.pagination` controls.

---

## 3. Systems Design & Clean Architecture (Dennis Ritchie & Uncle Bob)

### 3.1 Principle of Orthogonality & SRP (Uncle Bob)
- **Violation**: `InventoryOpsModel` acts as a fat controller. It directly constructs raw JSON request bodies, sets HTTP tokens, makes un-encapsulated POST requests, and manages UI state.
- **Solution**: Introduce `IInventoryOpsManager` and `InventoryOpsManager` in `Store.UI/Services/`:
  - Encapsulates `ReceiveGoodsAsync`, `ProcessReturnAsync`, `AdjustStockAsync`, `GetReorderSuggestionsAsync`, `GetMovementsAsync`, and `GetInventoryMetricsAsync`.
  - Encapsulates user ID extraction, validation, and error translation.
  - Keeps `InventoryOpsModel` razor page model thin (~60 lines) strictly responsible for HTTP binding and view data.

### 3.2 Dennis Ritchie Systems Design: Do One Thing & Do It Well
- **Simplicity**: Move stock actions into modal interactors so the main canvas remains a unified, high-density dashboard (KPIs ➔ Reorder Alerts ➔ Audit Stream).
- **Extensibility**: All stock movements are immutably logged with before/after snapshots, acting as an event-sourced audit ledger for inventory integrity.

---

## 4. Implementation Matrix — Inventory Ops vs Benchmarks

| Feature | Catalog | Invoices | Suppliers | **Inventory Ops (Current)** | **Inventory Ops (Proposed)** |
|---|---|---|---|---|---|
| 4-Card KPI Banner | ✅ | ✅ | ✅ | ❌ None | ✅ 4 Color-Coded KPI Cards |
| Search & Filter Dock | ✅ | ✅ | ✅ | ❌ None | ✅ Filter Dock with SVG icon |
| Movement Type Filter Pills | N/A | ✅ | N/A | ❌ None | ✅ All / GRN / Adjust / Return / Transfer |
| Quick Action Buttons in Toolbar | ✅ | ✅ | ✅ | ❌ Static Forms | ✅ + Receive GRN / ⚡ Adjust / ↩️ Return |
| Live Stock Simulation in Adjustments | ⚠️ | N/A | N/A | ❌ None | ✅ Live Current ➔ New Stock Simulator |
| Reorder to PO Generation | ❌ | N/A | ⚠️ | ❌ None | ✅ Quick PO Action per Low-Stock Item |
| Movement Audit Trail Badges | N/A | ✅ | N/A | ❌ Raw text | ✅ Color-coded semantic pills |
| CSV Audit Export | ❌ | ✅ | ✅ | ❌ None | ✅ Export CSV Button & Handler |
| Standard Pagination | ✅ | ✅ | ✅ | ❌ None | ✅ Standard `.pagination` |
| Empty State Handling | ✅ | ✅ | ✅ | ❌ Plain text | ✅ `.empty-state` with SVG icons |
| Clean Application Manager Layer | ⚠️ | ⚠️ | ⚠️ | ❌ Fat Model | ✅ `IInventoryOpsManager` |

---

## 5. Prioritized Action Plan

### Phase 1: Application Architecture & Domain DTOs
1. **DTO Extensions** in `Store.Models/DTOs/Operations/InventoryDtos.cs`:
   - `InventoryMetricsDto`: `LowStockCount`, `OutOfStockCount`, `MovementsTodayCount`, `TotalUnitsReceivedMtd`, `TotalAdjustmentVarianceMtd`.
   - `StockMovementFilterRequest`: Extends `PagedRequest` with `StockMovementType?`, `FromDate`, `ToDate`, `ItemId`.
2. **Core Service & Controller**:
   - Implement `GetInventoryMetricsAsync` in `StoreOperationsService.cs` and expose `[HttpGet("metrics")]` on `InventoryOperationsController.cs`.
   - Enhance `GetStockMovementsAsync` to support search, date range, and pagination.
3. **Application Orchestration Layer**:
   - Create `IInventoryOpsManager` and `InventoryOpsManager` in `Store.UI/Services/`.
   - Register in DI container (`Program.cs`).

### Phase 2: High-Density UI/UX Overhaul (`InventoryOps.cshtml`)
4. **KPI Grid**: 4 interactive cards with SVG icons and active filter states.
5. **Toolbar & Filter Dock**: Search box, movement type pills, date filters, Export CSV, and primary CTA buttons.
6. **Low Stock Reorder Monitor Section**: Card container with stock progress bars, estimated cost, and `⚡ Quick PO` trigger.
7. **Stock Movement Audit Trail**: Dense table with semantic badges (`.badge-success`, `.badge-danger`, `.badge-teal`), before/after pills, user avatars, and pagination.
8. **Interactive Modals**:
   - **Quick Stock Adjustment Modal**: With live stock level indicator and reason code dropdown.
   - **Goods Receipt (GRN) Modal**: Multi-line item receiving with live subtotal calculation.
   - **Stock Return Modal**: Customer/supplier return processing with invoice link.

### Phase 3: Verification & Micro-Interactions
9. Test end-to-end GRN posting, stock adjustment, return processing, and CSV export.
10. Verify compile and clean architecture boundaries.
