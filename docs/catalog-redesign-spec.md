# Modern Item Catalog & Inventory Management Suite Specification

## 1. Executive Summary & Objective

The **Item Catalog** is the central operational hub of the ClexAn Foods Retail & Operations system. It bridges product discovery, inventory management, pricing and profit margin calculation, barcode label generation, and point-of-sale dispatch.

This specification outlines the comprehensive overhaul of the Catalog module, transforming it from a basic static table into an interactive, high-performance **Catalog & Inventory Suite**.

---

## 2. Current State & Gap Analysis

```
┌───────────────────────────────────────────────────────────────────────────────────┐
│                                CURRENT GAPS                                       │
├─────────────────────────┬─────────────────────────┬───────────────────────────────┤
│ ❌ Item Click Inertness │ ❌ No Live Search/Filter│ ❌ No Barcode Studio / Labels │
│ Rows and cards are      │ No search input, no     │ Cannot generate barcodes or   │
│ non-interactive; no     │ category filter, no     │ print thermal/shelf price     │
│ detail drawer or view.  │ stock status chips.     │ tags for physical items.      │
├─────────────────────────┼─────────────────────────┼───────────────────────────────┤
│ ❌ No Margin Analytics  │ ❌ Stock Lockout        │ ❌ Single View Mode           │
│ Lacks live profit,      │ Stock editing disabled  │ No visual card grid for fast  │
│ markup, and inventory   │ without a quick stock   │ browsing on touchscreens and  │
│ valuation metrics.      │ correction modal.       │ POS tablets.                  │
└─────────────────────────┴─────────────────────────┴───────────────────────────────┘
```

---

## 3. System Architecture & Layout Wireframe

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│  [📦 Total SKUs: 1,240]  [💰 Stock Value: 18.4M XAF]  [⚠️ Low Stock: 14]  [🚫 Out: 3]   │  <-- KPI Metric Cards
├────────────────────────────────────────────────────────────────────────────────────────┤
│  🔍 [Search by name, barcode, SKU...]   [📁 Category ▾]   [🏷️ Status: All|Low|Out]  [▦|☰]│  <-- Search & Filter Dock
├────────────────────────────────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────────────────────────────────┐  │
│  │ 🏷️ PRODUCT CATALOG (Table or Grid View)                                          │  │
│  │  Photo  │ Name / Barcode  │ Category │ Price (XAF) │ Cost (XAF) │ Stock │ Action │  │
│  │  [🖼️]   │ Whole Milk 1L   │ Dairy    │ 1,200       │ 950        │ 45 🟢 │ [···]  │  │  <-- Clickable Row/Card
│  │  [🖼️]   │ Basmati Rice 5kg│ Grains   │ 6,500       │ 5,200      │  3 🔴 │ [···]  │  │
│  └──────────────────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────────────────────────┘
                                      │ (On Click Item)
                                      ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ 🗂️ SLIDE-OVER ITEM DETAIL DRAWER                                                       │
│ ┌───────────────────────────────────────┬────────────────────────────────────────────┐ │
│ │  [ High-Res Photo / Gallery ]         │  Whole Milk 1L                             │ │
│ │                                       │  Category: Dairy | Unit: Litre (L)         │ │
│ │  Barcode: 614141000036                │  Status: Active 🟢                         │ │
│ │  |||| |||||||| |||||||| |||||         │                                            │ │
│ │  [🖨️ Print Label] [📋 Copy Barcode]   │  Selling Price:  1,200 XAF                 │ │
│ ├───────────────────────────────────────┤  Cost Price:       950 XAF                 │ │
│ │ 📊 Financial & Margin Analytics       │  Unit Profit:     +250 XAF                 │ │
│ │  • Profit Margin: 20.83%              │  Profit Markup:    26.32%                  │ │
│ │  • Stock Valuation: 54,000 XAF        │  Total Stock Value: 54,000 XAF             │ │
│ ├───────────────────────────────────────┴────────────────────────────────────────────┤ │
│ │ 📦 Stock & Batch Status                                                            │ │
│ │  In Stock: 45 Litres  |  Reorder Level: 10 Litres                                  │ │
│ │  Batches:                                                                          │ │
│ │   • Batch #B-2026-08 (Qty: 25, Exp: 2026-11-15 🟢)                                │ │
│ │   • Batch #B-2026-09 (Qty: 20, Exp: 2026-12-01 🟢)                                │ │
│ ├────────────────────────────────────────────────────────────────────────────────────┤ │
│ │ ⚡ Quick Actions: [✏️ Edit Item] [⚡ Quick Stock Adjustment] [🛒 Open in POS]        │ │
│ └────────────────────────────────────────────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Key Functional Modules & Specifications

### 4.1. Interactive Slide-Over Item Detail Drawer
- **Trigger**: Clicking on any item table row, product card, or deep-linking via URL query `?item={itemId}`.
- **Components**:
  1. **Visual Hero**: High-res cropped image preview with zoom modal trigger, category tag, unit badge, and active status indicator.
  2. **Financial Breakdown**:
     $$\text{Unit Profit} = \text{Selling Price} - \text{Cost Price}$$
     $$\text{Profit Margin (\%)} = \left(\frac{\text{Selling Price} - \text{Cost Price}}{\text{Selling Price}}\right) \times 100$$
     $$\text{Markup (\%)} = \left(\frac{\text{Selling Price} - \text{Cost Price}}{\text{Cost Price}}\right) \times 100$$
     $$\text{Stock Valuation (Cost)} = \text{Cost Price} \times \text{InStock}$$
  3. **Barcode Studio Widget**: Renders standard barcode (Code128/EAN-13/QR) using SVG canvas with 1-click **"Print Label"** and **"Copy Barcode"**.
  4. **Stock Level Meter**: Visual progress bar indicating stock relative to reorder threshold.
  5. **Batch & Expiry Ledger**: Displays batches associated with the item, expiration dates, and remaining quantities.
  6. **Quick Command Hub**: Instant buttons for *Edit*, *Adjust Stock*, *Add to POS Cart*, *Deactivate/Activate*.

### 4.2. Live Search, Filter Dock & KPI Summary Cards
- **Top Metric Cards**:
  - **Total Products**: Active SKU count.
  - **Inventory Valuation**: Total monetary value held in stock (Retail & Cost).
  - **Low Stock Alerts**: Count of items where $\text{InStock} \le \text{ReorderLevel}$ (Clickable filter shortcut).
  - **Out of Stock**: Count of depleted items where $\text{InStock} \le 0$ (Clickable filter shortcut).
- **Search Dock**:
  - Instant debounced search querying `Name`, `Barcode`, `Description`, and `Category`.
  - Category dropdown filter and Unit dropdown filter.
  - Quick filter pills: `All`, `In Stock`, `Low Stock ⚠️`, `Out of Stock 🚫`, `Active`, `Inactive`.
  - View switcher: Toggle between **Compact Data Table** and **Visual Product Grid Cards**.

### 4.3. Barcode Studio & Label Printing Engine
- **Auto-Generator**: 1-click generation of store-standard 12/13 digit EAN-13 or internal SKU barcodes (`CLX-XXXXXX`).
- **Print Modes**:
  - **Single Item Thermal Sticker**: Standard 50mm × 25mm barcode label with Product Name, Price (XAF), and Barcode.
  - **A4 Multi-Label Sheet**: Standard 3 × 8 (24 labels per sheet) print layout for bulk labeling.
  - **Shelf Price Tag**: Shelf-edge label with bold price, unit, and scannable barcode.

### 4.4. Quick Stock Adjustment & Audit Trail
- Modal accessible from drawer or catalog row:
  - Adjustment Type: `Add Stock (+)` or `Deduct Stock (-)`.
  - Quantity: Integer adjustment.
  - Reason Code: `Initial Count Correction`, `Damaged / Spoiled`, `Found Stock`, `Supplier Return`, `Internal Store Use`.
  - Automatically records `StockMovement` audit record with user ID and timestamp.

### 4.5. Live Profit & Margin Calculator in Create/Edit Form
- In the Create/Edit modal, as the user types `Selling Price` and `Cost Price`, live calculation chips dynamically show:
  - **Estimated Profit per Unit**
  - **Gross Margin %**
  - **Markup %**

### 4.6. Data Portability & Bulk Operations
- **CSV / Excel Export**: One-click export of filtered or full catalog data.
- **Bulk Selection**: Checkbox multi-select to:
  - Bulk Deactivate / Activate
  - Bulk Category Update
  - Bulk Print Barcode Sheets

---

## 5. Technical Design & Data Flow

### 5.1. Backend Updates (`Catalog.cshtml.cs` & `ItemService.cs`)
```csharp
public class CatalogQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? StockStatus { get; set; } // "all" | "low" | "out" | "inactive"
    public string? SortBy { get; set; }     // "name" | "price_asc" | "price_desc" | "stock" | "created"
}
```

### 5.2. Client-Side State Management (`catalog.js`)
- `CatalogState`: Maintains current page, active filters, selected items, active view mode (grid/table), and drawer state.
- `BarcodeRenderer`: Lightweight client-side Code128 / QR SVG renderer.
- `LabelPrinter`: Generates print-ready CSS stylesheets for thermal and A4 sticker sheets.

---

## 6. Phased Implementation Plan

| Phase | Deliverables | Status |
| :--- | :--- | :--- |
| **Phase 1: Drawer & Clickable Rows** | Item Detail Slide-Over Drawer, clickable rows/cards, financial margin calculator, barcode rendering. | Planned |
| **Phase 2: Search, Filters & KPIs** | Top KPI summary cards, debounced live search, category & stock status filters, grid/table view toggle. | Planned |
| **Phase 3: Quick Stock Adjustment** | Stock adjustment modal with reason codes and `StockMovement` audit logging. | Planned |
| **Phase 4: Barcode Print Studio** | Thermal sticker, A4 multi-label sheet, and shelf tag printing templates. | Planned |
| **Phase 5: Bulk Actions & CSV Export** | Multi-item selection, bulk status toggle, and CSV catalog exporter. | Planned |

---

## 7. Verification & Quality Acceptance Criteria

1. **Item Drawer Interaction**: Clicking any row or card opens the detail drawer within $<100\text{ms}$ with zero layout shift.
2. **Search Performance**: Instant debounced search responsive within $<150\text{ms}$ without clearing user page context.
3. **Financial Accuracy**: Profit, margin %, and inventory valuation accurately calculate across integer and decimal price points.
4. **Barcode Compatibility**: Generated barcodes scan instantly with 1D/2D hardware scanners and mobile camera scanners.
5. **Print Fidelity**: Printed barcode labels align accurately on 50x25mm thermal rolls and standard A4 24-label sheets.
