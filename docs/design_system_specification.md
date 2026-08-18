# ClexAn Foods Operations Console — Master UI/UX Design System & Standardization Specification

---

## 1. Executive Summary & Design Philosophy

The **ClexAn Foods Operations Console** is an enterprise-grade retail, inventory, procurement, billing, loyalty, and customer relationship management (CRM) platform. 

The visual identity embodies:
1. **Emerald Organic Retail Heritage**: Deep forest greens (`#131c17` to `#1f2f25`), vibrant brand greens (`#019c01`), and crisp organic tints (`#e8f5e8`) reflecting fresh food operations.
2. **High-Density Information Architecture**: Clean, scannable, high-contrast typography, data-dense tables, 360-degree interactive flyout drawers (blades), and modular metric cards.
3. **Cohesive Micro-Interactions**: Predictable hover elevations (`translateY(-2px)`), glassmorphic backdrops, smooth drawer transitions (`cubic-bezier(0.2, 0.8, 0.2, 1)`), and consistent modal layers.

---

## 2. Screenshot-by-Screenshot Visual Audit & Analysis

Every image in `C:\Users\Rodern\Downloads\StoreProject Export & Artifacts\UI Screenshots` was thoroughly cataloged and inspected down to individual element styles.

| # | Screenshot Filename | Screen / Modal / Blade | Key Elements & Styles Identified | Discrepancies & Standardization Needed |
|---|---|---|---|---|
| 1 | `Screenshot_..._125817` | **Item Catalog & Inventory (Table View)** | 4-card KPI summary, search filter dock, category dropdown, sort dropdown, "+ New Product" green CTA, status pills, Table/Grid toggle, item rows with photo, barcode, category badge, bold green selling price, margin % badge, stock status pill, solid green action buttons ("Edit", "⚡ Stock"), floating scanner active FAB. | View toggle order (Table first, Grid second) was reversed compared to other screens. Table actions were solid green while other tables used outline buttons. |
| 2 | `Screenshot_..._125833` | **Item Catalog & Inventory (Grid View)** | 4-column product grid, top-left category badge, top-right stock/unit pill, white photo container, product title, barcode tag with icon, bold price + cost + margin badge pill. | Standardized card border-radius to `12px` and uniform height/padding across all modules. |
| 3 | `Screenshot_..._131013` | **Invoice Line-Item Return & Refund Modal (Top)** | Modal header with rotate icon, table of items with Unit Price, Qty Paid, Return Qty input boxes, Refund Subtotal calculation, Cancel button, "Process Return & Refund" CTA. | **Critical Inconsistency:** CTA button and accents used purple (`#7c3aed`/`#6366f1`) instead of brand palette and financial semantic colors. |
| 4 | `Screenshot_..._131029` | **Invoice Line-Item Return & Refund Modal (Bottom)** | Refund Method dropdown, "Restock Inventory" checkbox, "Reason for Return" input, "Total Refund to Customer" calculation highlight card. | Purple-tinted total box (`#f5f3ff`) and purple CTA button standardized to brand green/amber financial style. |
| 5 | `Screenshot_..._131134` | **Supplier 360 Hub & Procurement (Grid View)** | 5 KPI cards (Total Suppliers, Active Partners, Procurement Spend, Open POs, Due Deliveries), Search input, Sort dropdown, Grid/Table toggle, "Export CSV", "+ New Supplier" CTA, Supplier contact card with "SS" avatar, 360 pill button, PO & Edit mini buttons. | KPI grid wrapped as 3 + 2 cards. Standardized grid to responsive auto-fit. |
| 6 | `Screenshot_..._131150` | **Supplier 360 Hub & Procurement (Table View)** | KPI banner, toolbar, data table with SS avatar, Supplier Name, Reg #, Phone, Email, Location, Registered date, Outline Action buttons ("360", "PO", "Edit"). | Inconsistent action button styling between Catalog table (solid green) and Supplier table (gray outline). Standardized to clean outline pill buttons. |
| 7 | `Screenshot_..._13125` | **Product 360 Blade / Drawer** | Top status badges ("Active", "Product"), close button, product photo, title "Broli Milk", category & unit pills, 2x2 financial metric cards (Selling Price, Cost Price, Estimated Profit, Gross Margin), total valuation card, stock level progress bar, Barcode & SKU Studio with copy button and print label CTA, 4-button sticky bottom footer ("Edit", "⚡ Adjust Stock", "🛒 POS", "Deactivate"). | Standardized blade widths (`540px`), card spacing, and typography scale. |
| 8 | `Screenshot_..._131326` | **Register New Supplier Modal** | Modal header, Avatar upload tile, Company Name input, Tax ID, Notes / Payment Terms, repeatable Email, Phone, and Location field rows with "+ Add" pills, "Primary" checkmark, Cancel & "Create Supplier" CTA. | Form grid layout glitch where Notes textarea floated awkwardly beside the label instead of full-width stack. |
| 9 | `Screenshot_..._131421` | **Supplier 360 Drawer (Vendor ID Pass Tab)** | Avatar, Vendor name, Reg ID badge, quick communication pills (WhatsApp, Call, Email, Map), 3-stat summary (Total Spend, Total Orders, Open POs), drawer tab navigation, dark navy Authorised Vendor Pass card (`#0f172a`), barcode graphic, print badge button, drawer footer (Delete, Edit, Create PO). | Golden standard for 360 Flyout Passes; adopted as universal design blueprint for Customer and Loyalty passes. |
| 10 | `Screenshot_..._131434` | **Supplier 360 Drawer (Contacts & Locations Tab)** | Email Addresses card with green "Primary" pill, Phone Numbers card, Physical Locations card, Vendor Notes card. | Standardized contact card list styling across Supplier and Customer drawers. |
| 11 | `Screenshot_..._131450` | **Supplier 360 Drawer (Supplied Items Tab)** | Supplied item row with product name, barcode, unit cost, and Total Received badge. | Standardized itemized listing card. |
| 12 | `Screenshot_..._13145` | **Quick Stock Adjustment Modal (Layered on Blade)** | Lightning icon, product name subtitle, current on-hand stock box, adjustment qty input with helper text, reason code dropdown, Cancel & "Save Adjustment" buttons. | Cancel button had a solid green background instead of secondary neutral outline. |
| 13 | `Screenshot_..._13156` | **Supplier 360 Drawer (Purchase Orders Tab)** | PO summary card (`#PO-1`, date, item count, total spend, "Draft" status pill). | Standardized PO card pill colors and metadata rows. |
| 14 | `Screenshot_..._132055` | **Loyalty & Rewards 360 Hub (Grid View)** | 5 KPI cards (Enrolled Members, Points Liability, Points Earned MTD, Points Redeemed MTD, VIP Ratio), Search, Tier filter pills (Bronze, Silver, Gold), Sort dropdown, 3-way toggle (Grid, Table, Ledger), "Export CSV", "⭐ Manage Points & Rewards" CTA, Member cards with avatar, Tier badges (Gold/Bronze), lifetime points, currency equivalent value pill, Tier progress bar, Action buttons. | Rhoda Kah card was missing the secondary "Action" button. Standardized card action row consistency. |
| 15 | `Screenshot_..._132115` | **Loyalty & Rewards Hub (Ledger View)** | Store-Wide Points Transaction Stream table with Date & Time, Member link, Phone, Type, Points (+36,909 green bold), Invoice Ref link, Notes & Reason. | Standardized table typography and badge alignments. |
| 16 | `Screenshot_..._132135` | **Loyalty & Rewards Hub (Table View)** | Loyalty Member directory table with square icon buttons (`🔍` and `⚡`) for actions. | Standardized table action buttons to match suite-wide design system. |
| 17 | `Screenshot_..._13221` | **Loyalty 360 Drawer + Manage Points Modal (Earn Points Tab)** | Dual-layer UI: Background Loyalty 360 Drawer + foreground Manage Points Modal. Modal features Earn/Redeem/Adjust segmented tab, Member search, Points to Award, live Currency Value pill, Invoice Ref, Reason, "Award Points" CTA. | Coherent and clean. Verified z-index stacking (`z-index: 250` for blade, `z-index: 300` for layered modal). |
| 18 | `Screenshot_..._132221` | **Manage Points Modal (Redeem Reward Tab)** | Points to Redeem input, Live XAF value badge, Note input, "Redeem Reward" CTA. | Verified consistent form layout. |
| 19 | `Screenshot_..._132240` | **Manage Points Modal (Admin Adjust Tab)** | Points Adjustment (+ or -) input, Live value badge, Note input, "Apply Adjustment" CTA. | Verified consistent form layout. |
| 20 | `Screenshot_..._132258` | **Loyalty 360 Drawer (Transaction Ledger Tab)** | Individual member transaction history table (Date, Type, Points, Note). | Clean, responsive nested table layout. |
| 21 | `Screenshot_..._132310` | **Loyalty 360 Drawer (Active Campaigns Tab)** | Empty state message for applicable customer segment promotions. | Standardized empty state typography and padding. |
| 22 | `Screenshot_..._132330` | **Campaigns Hub (Grid View)** | 4 KPI cards (Live Campaigns, Scheduled, Targeted Reach, Peak Multiplier), Underline status tabs (All, Live Now, Scheduled, Completed, Paused) with count badges, Filter dock, Campaign cards with title, status badge, reward pill (`$+12 Bonus`, `1.0x Points`), target segment, date range, action icon buttons (Play/Pause, Clone, Edit, Delete). | Status filters used underline tabs while other pages used pill buttons. Standardized when to use Underline Tabs vs Filter Pills. |
| 23 | `Screenshot_..._132339` | **Campaigns Hub (Table View)** | Data table with Campaign Name, Reward badge, Audience, Dates, Status pill, Play/Pause and Edit outline icon buttons. | Clean table structure matching system design. |
| 24 | `Screenshot_..._132358` | **New Campaign Modal with Simulator** | Quick Presets pills, Campaign Name, Description textarea, Type and Segment dropdowns, Multiplier input, Date pickers, Live Points Simulator card with sample spend calculation, "Active immediately" checkbox, Cancel & Create button. | Cancel button was unstyled plain text instead of `.btn-secondary`. |
| 25 | `Screenshot_..._13235` | **Edit Product Modal (with Live Margins)** | Product Name, Description, Selling Price, Cost Price, Live Unit Profit / Gross Margin / Markup metrics banner, Category, Unit, Stock Qty, Reorder Level, Barcode input with "🎲 Generate" button, Photo upload, Cancel & "Save Product" CTA. | Cancel button had a solid green background instead of secondary neutral outline. |
| 26 | `Screenshot_..._132426` | **Campaigns Hub (Live Now Empty State)** | Filtered tab showing dashed empty state container, green compass icon with halo, title "No campaigns found", subtitle, "Create Campaign" CTA. | Golden standard for In-Page Empty States. |
| 27 | `Screenshot_..._132443` | **Campaigns Hub (Scheduled Tab Filtered)** | Filtered list showing Black Friday campaign with Purple "Scheduled" badge. | Verified status badge palette (`#7c3aed` with `rgba(139,92,246,0.12)` background). |
| 28 | `Screenshot_..._132452` | **Campaigns Hub (Completed Tab Empty State)** | Filtered tab showing dashed empty state container. | Consistent with Live Now empty state. |
| 29 | `Screenshot_..._132526` | **Delete Campaign Confirmation Modal** | Danger alert icon (red exclamation triangle in soft red circle), confirmation title, bold entity name, Cancel & solid red "Delete" CTA. | Cancel button was styled as green text instead of `.btn-secondary`. |
| 30 | `Screenshot_..._132556` | **Clone Campaign Modal + Alert Banner** | Info circular icon, confirmation message, Cancel & green "Confirm" CTA. Background shows green toast alert notification banner. | Toast banner height and placement verified. |
| 31 | `Screenshot_..._13257` | **Campaigns Hub (Paused Tab Filtered)** | Filtered list showing Easter campaign with Amber "Paused" badge. | Verified status badge palette (`#d97706` with `rgba(245,158,11,0.12)` background). |
| 32 | `Screenshot_..._13443` | **Customer 360 CRM Hub (Grid View)** | 4 KPI cards (Total Registered, Tiered Accounts, Active Loyalty Points, Outstanding Balance), Segment filter pills (All, Standard, Wholesale, VIP), Loyalty Tier dropdown, "With Debt Only" checkbox, Sort dropdown, Grid/Table toggle, "+ New Customer" CTA, Customer Cards with avatar, Gold/Bronze tier badge, segment pill, lifetime spend, balance/debt status, "POS Sale" and "View 360" outline buttons. | Standardized customer card layout and button alignment. |
| 33 | `Screenshot_..._13545` | **Edit Customer Profile Modal** | First/Last Name (2 cols), Middle/Gender (2 cols), Segment dropdown, Phone/Email (2 cols), Internal CRM Notes textarea, Photo upload, Cancel & "Save Changes" CTA. | Form grid spacing and input focus states verified. |
| 34 | `Screenshot_..._1358` | **Customer 360 CRM Drawer (Invoices Tab + Loyalty Barcode Pass)** | Customer avatar, Name "Rhoda Bei Kah", VIP & Bronze Tier badges, WhatsApp/Call/Email contact pills, 3-metric financial summary (Lifetime Spend, Total Invoices, Unpaid Balance), dark Store Rewards Pass card with barcode, Loyalty Tier Progress unlock bar with "⭐ Adjust Points" button, Drawer tabs (Invoices, Loyalty Ledger, Profile & Notes), Invoices list items, Sticky footer (Delete, Edit Profile, Start POS Sale). | Complete, cohesive Customer 360 implementation. |
| 35 | `Screenshot_..._13635` | **Invoices 360 Financial & Billing Hub (Main View)** | Invoices & Billing Hub inner header card with recorded count, "Export CSV", "+ New POS Sale" CTA, 4 KPI cards (Gross Invoiced, Total Collected, Outstanding Debt, Refunds & Returns), Time range segmented tab (All Time, Today, Yesterday, Last 7d, Last 30d, This Month, Custom), Secondary filter bar (Statuses, Payment Methods, Date Sort, Filter, Reset), Table with copyable Invoice ID pill, Date/Time, Customer link, Cashier, Payment Method badge, Total Amount bold, Status badge (`✓ PAID`), Actions (`👁️ Details`, thermal receipt print, tax invoice pdf). | Standardized table typography, copyable ID button, and action icons. |
| 36 | `Screenshot_..._13724` | **Invoice Details Blade (Header & Itemized Bill)** | Header "Invoice #d22653b7", Close button, 2-column Metadata grid (Date, Customer link, Cashier, Branch, Status, Payment Mode), Itemized bill table (Item, Unit Price, Qty, Discount, Line Total), Sticky footer (Thermal Receipt, A4 Tax Invoice, Refund & Return Items, Void). | Cohesive invoice drawer layout. |
| 37 | `Screenshot_..._13749` | **Invoice Details Blade (Totals, Payment Tender, Footer)** | Scrolled view showing Grand Total financial summary card (Subtotal, Grand Total bold, Amount Tendered green, Change Given), Payment Tender Breakdown table (Method, Amount Applied, Reference, Recorded), Sticky action footer. | Clear financial calculation hierarchy and contrasting colors. |
| 38 | `Screenshot_..._13945` | **Void Invoice Confirmation Modal** | Red `⊗` icon, warning description, "Reason for Voiding" input, Cancel & solid red "Confirm & Void Invoice" CTA. | Clean danger confirmation modal. |

---

## 3. Universal Design Tokens & Color Palette

### 3.1 Core Color System

```css
:root {
    /* ── Primary Brand Green (Organic Retail Identity) ── */
    --brand:                #019c01;  /* Canonical primary action green */
    --brand-dark:           #006300;  /* Primary hover / active state */
    --brand-darker:         #004700;  /* Deep emphasis green */
    --brand-soft:           #e8f5e8;  /* Subtle tinted backgrounds */
    --brand-soft-hover:     #daf0da;  /* Hover state for soft items */
    --brand-border:         #b7d9b7;  /* Accent borders */
    --brand-glow:           rgba(1, 156, 1, 0.20); /* Focus rings and button glows */

    /* ── Dark Navigation & Shell Surface ── */
    --sidebar-grad-start:   #1f2f25;  /* Deep forest green top */
    --sidebar-grad-end:     #131c17;  /* Dark obsidian forest bottom */
    --sidebar-border:       rgba(255, 255, 255, 0.08);
    --sidebar-link:         #dbe7df;  /* Muted pale mint link text */
    --sidebar-muted:        #8da394;  /* Muted sidebar section labels */
    --sidebar-active-bg:    rgba(1, 156, 1, 0.25);

    /* ── Canvas, Surfaces & Cards ── */
    --canvas:               #f0f3f1;  /* Modern cool tinted page background */
    --canvas-tint-soft:     #f7fff7;  /* Radial gradient top-right highlight */
    --canvas-tint-alt:      #e8ece8;  /* Radial gradient bottom shade */
    --surface:              #ffffff;  /* Card, modal, and blade background */
    --surface-alt:          #f8fafc;  /* Inset boxes, secondary toolbars */
    --surface-hover:        #f1f5f9;  /* List item and table row hover */

    /* ── Typography & Ink ── */
    --text-primary:         #0f172a;  /* High contrast primary slate ink */
    --text-secondary:       #64748b;  /* Subtitles, labels, metadata */
    --text-muted:           #94a3b8;  /* Disabled, placeholders, tertiary text */
    --text-on-brand:        #ffffff;  /* Text on solid brand buttons */

    /* ── Borders & Separators ── */
    --border:               #e2e8f0;  /* Canonical border color */
    --border-strong:        #cbd5e1;  /* High-emphasis borders / inputs */
    --border-subtle:        #edf2f7;  /* Table row separators */
    --border-hover:         #94a3b8;  /* Input hover border */

    /* ── Semantic Colors & Status Badges ── */
    /* Success / Active / In Stock / Paid */
    --success:              #059669;
    --success-dark:         #047857;
    --success-soft:         rgba(5, 150, 105, 0.10);
    --success-border:       rgba(5, 150, 105, 0.25);

    /* Danger / Out of Stock / Void / Delete / Error */
    --danger:               #dc2626;
    --danger-dark:          #b91c1c;
    --danger-soft:          #fef2f2;
    --danger-border:        #fecaca;

    /* Warning / Low Stock / Paused / Draft / Bronze Tier */
    --warning:              #d97706;
    --warning-dark:         #b45309;
    --warning-soft:         #fffbeb;
    --warning-border:       #fde68a;

    /* Scheduled / Info / VIP Points / Special Promotion */
    --info-purple:          #7c3aed;
    --info-purple-dark:     #6d28d9;
    --info-purple-soft:     rgba(139, 92, 246, 0.12);
    --info-purple-border:   rgba(139, 92, 246, 0.25);

    /* Cold Info / Cyan / Stock Balance / Processing */
    --info-teal:            #0d9488;
    --info-teal-soft:       rgba(20, 184, 166, 0.12);

    /* Metallic Gold / VIP Tier */
    --gold-tier:            #d97706;
    --gold-tier-soft:       rgba(245, 158, 11, 0.15);
    --gold-tier-border:     rgba(245, 158, 11, 0.35);

    /* ── Elevation & Shadows ── */
    --shadow-xs:            0 1px 2px rgba(0, 0, 0, 0.04);
    --shadow-sm:            0 1px 3px rgba(0, 0, 0, 0.06), 0 1px 2px rgba(0, 0, 0, 0.04);
    --shadow-md:            0 4px 12px rgba(0, 0, 0, 0.08), 0 2px 4px rgba(0, 0, 0, 0.04);
    --shadow-lg:            0 10px 25px rgba(0, 0, 0, 0.10), 0 4px 8px rgba(0, 0, 0, 0.05);
    --shadow-xl:            0 20px 40px rgba(0, 0, 0, 0.18), 0 8px 16px rgba(0, 0, 0, 0.08);
    --shadow-blade:         -6px 0 30px rgba(0, 0, 0, 0.15);

    /* ── Spacing & Radii ── */
    --radius-xs:            4px;
    --radius-sm:            6px;
    --radius-md:            8px;
    --radius-lg:            12px;
    --radius-xl:            16px;
    --radius-full:          9999px;

    --space-1:              4px;
    --space-2:              8px;
    --space-3:              12px;
    --space-4:              16px;
    --space-5:              20px;
    --space-6:              24px;
    --space-8:              32px;
}
```

---

## 4. Typography Scale & Specifications

| Role | Font Family | Size | Line Height | Weight | Tracking | Color |
|---|---|---|---|---|---|---|
| **Page Header (H1)** | Segoe UI Variable, system-ui | `1.65rem` (26px) | 1.2 | 700 | `-0.02em` | `--text-primary` (`#0f172a`) |
| **Section Header (H2)** | Segoe UI Variable, system-ui | `1.35rem` (21px) | 1.3 | 700 | `-0.01em` | `--text-primary` (`#0f172a`) |
| **Card / Modal Title (H3)** | Segoe UI Variable, system-ui | `1.15rem` (18px) | 1.3 | 600 | `0` | `--text-primary` (`#0f172a`) |
| **Subtitle / Subheader** | Segoe UI Variable, system-ui | `0.88rem` (14px) | 1.4 | 400 | `0` | `--text-secondary` (`#64748b`) |
| **KPI Metric Value** | Segoe UI Variable, system-ui | `1.50rem` (24px) | 1.1 | 700 | `-0.02em` | `--text-primary` (`#0f172a`) |
| **KPI Overline Label** | Segoe UI Variable, system-ui | `0.78rem` (12px) | 1.2 | 700 | `0.06em` | `--text-secondary` (UPPERCASE) |
| **Table Header (TH)** | Segoe UI Variable, system-ui | `0.75rem` (12px) | 1.2 | 700 | `0.05em` | `--text-secondary` (UPPERCASE) |
| **Table Body (TD)** | Segoe UI Variable, system-ui | `0.88rem` (14px) | 1.4 | 400 / 500 | `0` | `--text-primary` (`#0f172a`) |
| **Primary Price / Valuation**| Segoe UI Variable, system-ui | `1.05rem` (16px) | 1.2 | 700 | `0` | `--brand` / `--text-primary` |
| **Barcode / SKU / Codes** | Cascadia Code, Consolas, monospace | `0.80rem` (13px) | 1.2 | 600 | `0.04em` | `--text-primary` |
| **Badge / Pill Text** | Segoe UI Variable, system-ui | `0.72rem` (11.5px)| 1.2 | 700 | `0.03em` | Varies by status |

---

## 5. Component Standardization Catalog

### 5.1 KPI Summary Metric Cards

```html
<div class="kpi-grid">
    <div class="kpi-card">
        <div class="kpi-icon kpi-icon-green">
            <!-- 24x24 SVG Icon -->
        </div>
        <div class="kpi-info">
            <span class="kpi-label">Active Products</span>
            <span class="kpi-val">19</span>
            <span class="kpi-sub">Total catalog items</span>
        </div>
    </div>
</div>
```

**Standardized Card CSS:**
```css
.kpi-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;
    margin-bottom: 20px;
}

.kpi-card {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: 16px 20px;
    display: flex;
    align-items: center;
    gap: 16px;
    box-shadow: var(--shadow-sm);
    transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
    text-decoration: none;
    color: inherit;
}

.kpi-card:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
    border-color: var(--brand-border);
}

.kpi-card.active {
    border-color: var(--brand);
    background: var(--brand-soft);
}

.kpi-icon {
    width: 48px;
    height: 48px;
    border-radius: var(--radius-lg);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.4rem;
    flex-shrink: 0;
}

.kpi-icon-green   { background: var(--brand-soft); color: var(--brand); }
.kpi-icon-amber   { background: var(--warning-soft); color: var(--warning); }
.kpi-icon-red     { background: var(--danger-soft); color: var(--danger); }
.kpi-icon-purple  { background: var(--info-purple-soft); color: var(--info-purple); }
.kpi-icon-teal    { background: var(--info-teal-soft); color: var(--info-teal); }
```

---

### 5.2 Filter Docks & Toolbars

The Toolbar pattern standardizes:
1. **Search Box with Icon & Clear Action**: Left side, flexible min-width (`240px`).
2. **Filter Controls (Dropdowns, Segment Pills, Date Pickers)**: Middle flex row.
3. **View Switcher (Grid / Table / Ledger)**: Segmented button group. Canonical order is always `Grid` first, `Table` second, `Ledger` third (when present).
4. **Action Buttons**: Export button (Secondary) + "+ New Entity" button (Primary Brand Green).

```css
.toolbar {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: var(--radius-lg);
    padding: 14px 18px;
    margin-bottom: 20px;
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
    align-items: center;
    justify-content: space-between;
    box-shadow: var(--shadow-sm);
}

.search-input-wrap {
    position: relative;
    flex: 1;
    min-width: 240px;
}

.search-input-wrap input {
    width: 100%;
    padding: 9px 14px 9px 38px;
    border-radius: var(--radius-md);
    border: 1px solid var(--border);
    background: var(--surface-alt);
    color: var(--text-primary);
    font-size: 0.9rem;
    outline: none;
    transition: all 0.2s ease;
}

.search-input-wrap input:focus {
    background: var(--surface);
    border-color: var(--brand);
    box-shadow: 0 0 0 3px var(--brand-glow);
}

.search-icon {
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    color: var(--text-secondary);
    pointer-events: none;
}

.view-toggle {
    display: inline-flex;
    background: var(--canvas);
    border: 1px solid var(--border);
    border-radius: var(--radius-md);
    padding: 2px;
}

.view-btn {
    border: none;
    background: transparent;
    color: var(--text-secondary);
    padding: 6px 12px;
    border-radius: var(--radius-sm);
    cursor: pointer;
    font-size: 0.85rem;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 6px;
    transition: all 0.15s ease;
}

.view-btn.active {
    background: var(--surface);
    color: var(--brand);
    box-shadow: var(--shadow-xs);
}
```

---

### 5.3 Buttons Specification

| Variant | Class Name | Background | Text Color | Border | Shadow / Hover |
|---|---|---|---|---|---|
| **Primary CTA** | `.btn-primary` | `--brand` (`#019c01`) | `#ffffff` | `1px solid var(--brand)` | `0 2px 6px rgba(1,156,1,0.2)` -> hover `--brand-dark` |
| **Secondary / Outline** | `.btn-secondary` | `--surface` (`#ffffff`) | `--text-primary` | `1px solid var(--border)` | hover `--surface-alt` & `--border-strong` |
| **Danger CTA** | `.btn-danger` | `--danger` (`#dc2626`) | `#ffffff` | `1px solid var(--danger)` | hover `--danger-dark` |
| **Danger Outline** | `.btn-danger-outline`| `--surface` (`#ffffff`) | `--danger` | `1px solid var(--danger-border)`| hover `--danger-soft` |
| **Icon Button** | `.icon-btn` | `--surface` | `--text-secondary`| `1px solid var(--border)` | `32px x 32px`, hover `--brand-soft` |
| **Action Pill (Table)** | `.btn-table-action` | `--surface` | `--text-primary` | `1px solid var(--border)` | `padding: 4px 10px; font-size: 0.8rem;` |

---

### 5.4 360-Degree Flyout Drawers (Blades)

The 360 Flyout Panel provides complete, in-context entity inspection without navigating away from the grid/table.

```
┌────────────────────────────────────────────────────────┐
│ [Avatar] Entity Name [Status Badge] [Tier Badge]  [✕] │ ← Header
├────────────────────────────────────────────────────────┤
│ [💬 WhatsApp]  [📞 Call]  [✉️ Email]  [📍 Map]         │ ← Quick Actions
├────────────────────────────────────────────────────────┤
│ ┌──────────────┬──────────────┬──────────────────────┐ │
│ │ Metric 1     │ Metric 2     │ Metric 3             │ │ ← KPI Grid
│ └──────────────┴──────────────┴──────────────────────┘ │
├────────────────────────────────────────────────────────┤
│ ╔════════════════════════════════════════════════════╗ │
│ ║  AUTHORIZED DIGITAL 360 PASS                       ║ │ ← Digital Pass
│ ║  Entity Title                       [ACTIVE BADGE] ║ │   (Dark Navy)
│ ║  ||||||||||||||||||||||||||||||||||||||||||||||||| ║ │
│ ║  CODE: ENT-98472910                                ║ │
│ ║                                      [Print Badge] ║ │
│ ╚════════════════════════════════════════════════════╝ │
├────────────────────────────────────────────────────────┤
│ [Tab 1: Overview]  [Tab 2: Ledger]  [Tab 3: Sub-Items] │ ← Underline Tabs
├────────────────────────────────────────────────────────┤
│                                                        │
│                  Active Tab Content                    │ ← Scrollable Body
│                                                        │
├────────────────────────────────────────────────────────┤
│ [🗑️ Delete]               [Edit Profile] [Primary CTA]│ ← Sticky Footer
└────────────────────────────────────────────────────────┘
```

**Standardized Blade CSS:**
```css
.blade-overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.45);
    backdrop-filter: blur(3px);
    z-index: 250;
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.25s ease, visibility 0.25s ease;
}

.blade-overlay.open {
    opacity: 1;
    visibility: visible;
}

.blade {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    width: min(540px, 100vw);
    background: var(--surface);
    box-shadow: var(--shadow-blade);
    z-index: 260;
    transform: translateX(100%);
    transition: transform 0.28s cubic-bezier(0.2, 0.8, 0.2, 1);
    display: flex;
    flex-direction: column;
}

.blade.open {
    transform: translateX(0);
}

.blade-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 18px 24px;
    border-bottom: 1px solid var(--border);
}

.blade-body {
    padding: 20px 24px;
    overflow-y: auto;
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.blade-footer {
    padding: 16px 24px;
    border-top: 1px solid var(--border);
    background: var(--surface-alt);
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 12px;
}
```

---

### 5.5 Modal System Standardization

Modals must strictly obey:
1. **Backdrop**: `rgba(15, 23, 42, 0.5)` with blur.
2. **Animation**: Scale and slide `transform: scale(0.96) translateY(12px) -> scale(1) translateY(0)`.
3. **Cancel Buttons**: ALWAYS use `.btn-secondary` (white background, gray border, dark text). NEVER use solid green or bare text links for Cancel actions.
4. **Form Controls**: Full width, `8px` radius, focus ring with brand glow.
5. **Layering with Blades**: When a modal is triggered from inside a 360 blade, modal `z-index` is `300` and overlay `z-index` is `290` so it rests naturally above the blade (`z-index: 260`).

```css
.modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.5);
    backdrop-filter: blur(3px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 300;
    padding: 16px;
}

.modal {
    background: var(--surface);
    border-radius: var(--radius-xl);
    padding: 24px;
    width: min(580px, 94vw);
    max-height: 88vh;
    overflow-y: auto;
    box-shadow: var(--shadow-xl);
    border: 1px solid var(--border);
    animation: modalPop 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modalPop {
    from { opacity: 0; transform: scale(0.96) translateY(8px); }
    to { opacity: 1; transform: scale(1) translateY(0); }
}

.modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border);
}

.modal-footer {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    gap: 12px;
    margin-top: 24px;
    padding-top: 16px;
    border-top: 1px solid var(--border);
}
```

---

## 6. Comprehensive Component Standardization Matrix

| Component | Standard Specifications | Rules & Prohibitions |
|---|---|---|
| **Buttons** | `border-radius: 8px; font-weight: 600; padding: 8px 16px; font-size: 0.88rem;` | 🚫 Never use unstyled raw text for cancel. 🚫 Never make Cancel buttons solid green. |
| **Inputs & Selects** | `border-radius: 8px; border: 1px solid var(--border); padding: 9px 12px; font-size: 0.9rem;` | 🚫 Never use plain browser default outlines. Always use focus ring `0 0 0 3px var(--brand-glow)`. |
| **Status Badges** | `border-radius: 9999px; font-size: 0.72rem; font-weight: 700; padding: 2px 8px;` | 🚫 Do not mix raw hardcoded hex codes. Use standard semantic soft tint background + dark text. |
| **Digital Passes** | `background: #0f172a; color: #ffffff; border-radius: 12px; padding: 18px; box-shadow: var(--shadow-md);` | 🚫 Must contain clear high-contrast white barcode card container with monospace ID and print CTA. |
| **Grid Cards** | `border-radius: 12px; border: 1px solid var(--border); padding: 16px; background: var(--surface);` | 🚫 Always maintain uniform card heights and bottom-aligned action button rows. |
| **Data Tables** | `border-collapse: collapse; font-size: 0.88rem; th { text-transform: uppercase; font-size: 0.75rem; letter-spacing: 0.05em; }` | 🚫 Always wrap in `.table-wrap` card container with subtle hover row background. |
| **Flyout Blades** | `width: min(540px, 100vw); z-index: 260; backdrop-filter: blur(3px); transform transition 0.28s;` | 🚫 Must contain Sticky Header, Scrollable Body, and Sticky Bottom Action Footer. |
| **Confirmation Modals** | `max-width: 440px; text-align: center for icon and warning copy; flex-end actions;` | 🚫 Always place Cancel on left/neutral and Destructive action on right in solid red. |

---

## 7. Implementation Roadmap & Verification

1. **Tokens Integration**: All semantic tokens, radii, shadows, and status colors consolidated into `tokens.css` and `components.css`.
2. **Elimination of Rogue Styles**:
   - Replaced purple button styles in refund modal with standardized financial action buttons.
   - Fixed Cancel buttons across modals to use uniform `.btn-secondary`.
   - Harmonized Table Action buttons between Catalog, Suppliers, Loyalty, and Invoices.
   - Unified View Toggle ordering (`Grid` -> `Table` -> `Ledger`).
   - Fixed Supplier Registration Form notes textarea layout.
3. **Responsive Breakpoints**:
   - `Desktop (> 1024px)`: Multi-column grid, full flyout blade (`540px`), full table columns.
   - `Tablet (768px - 1024px)`: 2-column grid, compact blade (`480px`), horizontally scrollable table wrapper.
   - `Mobile (< 768px)`: 1-column stack, full-screen blade (`100vw`), condensed cards.

---
*ClexAn Foods Design System — Operations Console UI/UX Engineering Specification*
