# Design Consistency Report — StoreProject UI

Generated: 2026-08-24 13:39:00 +01:00
Based on: docs/design_system_specification.md and a repo-wide scan of Store.UI (tokens, components, JS hooks)

---

## Executive summary

This report audits the Store.UI Razor Pages against the design system specification (tokens, components, motion/micro-interactions) and identifies where pages are consistent, partially migrated, or still missing required classes and interaction hooks. The goal is a review-only checklist: no source files were modified.

High-level findings
- Core design tokens and component CSS files exist: `tokens.css`, `components.css`, `operations.css` and are referenced from the layout.
- JS hooks for blades and toasts exist (`site.js`), but usage is inconsistent across pages.
- Many pages have been refactored to use the new component classes (kpi-grid, kpi-card, blade), but several important hubs still use legacy or mixed patterns (Bootstrap classes, ad-hoc styles).
- Micro-interactions (hover elevation, focus states, toast timing, copy feedback) are partially implemented in JS and CSS but are missing on many input controls and action buttons.

Scope & methodology
- Scanned: all files under `Store.UI/Pages` and `Store.UI/wwwroot/{css,js}` for tokens and hooks.
- Patterns searched: button tokens (btn-primary / button-primary), blade hooks (openBlade/closeBlade), kpi and empty-state classes, showToast, data-tooltip/aria-label usage.
- Produced per-page checklist for the most relevant pages (recently edited and key operational hubs).

---

## Global token/component coverage (summary)
- tokens.css: PRESENT and contains semantic color variables and some component tokens.
- components.css: PRESENT with definitions for `.btn`, `.blade`, `.kpi-card`, `.empty-state` (partial coverage).
- operations.css: PRESENT with hub layouts and `.kpi-grid` styles.
- site.js: PRESENT and contains blade open/close and showToast hooks.

Observed gaps (global):
- Not all pages reference semantic `.btn-*` tokens; many still use `btn btn-primary` (Bootstrap) or custom inline classes.
- Tooltip attributes and aria-labels are inconsistently applied.
- Some pages used CSS classes that were replaced by tokens but tokens not yet defined in components.css (e.g., `text-info`, `status error`).

---

## Per-page checklist (present / missing / notes)

Legend: Present = uses token/hook; Partial = some elements use it; Missing = no usage found; Recommend = suggested micro-interaction or accessibility addition.

Notes: file links point to repo files.

### App-wide / Layout
- [Store.UI/Pages/Shared/_AppLayout.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Shared/_AppLayout.cshtml)
  - Buttons: Partial (bootstrap classes still present in many nav items)
  - Blade container & toast mounting point: Present
  - Accessibility: Partial — skip links and aria landmarks present; ensure focus management when blades open
  - Recommend: global keyboard shortcut to close blades (Esc), focus trapping inside blades, and visible focus ring styles for keyboard users

### Index / Home
- [Store.UI/Pages/Index.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Index.cshtml)
  - KPI grid: Partial (uses `.kpi-card` in some sections)
  - Empty states: Missing (where data empty)
  - Micro-interactions: Recommend hover elevation on KPI cards and aria-live for dynamic counters

### POS (Point of Sale)
- [Store.UI/Pages/Pos.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Pos.cshtml)
  - Buttons: Partial (some `.button-primary` present, some legacy `btn` remain)
  - JS hooks (amountTendered, receipt open/close): Present but note previously observed duplicate variable error — requires dedupe
  - Blade & receipt: Present (blade pattern used for receipts)
  - Recommend micro-interactions:
    - Debounced quantity input + focused numeric keypad animation
    - Real-time change calculation with highlighted update animation (fade-in + bg-pulse)
    - Accessible ARIA updates for the total and tender fields (aria-live="polite")

### Catalog / Items
- [Store.UI/Pages/Catalog.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Catalog.cshtml)
  - KPI & filter-dock: Present (uses `.kpi-grid`, `.filter-dock`)
  - Card/grid view tokens: Partial (card tokens present but some inline heights/paddings differ)
  - Image thumbnails: Missing lazy-loading attribute (recommend `loading="lazy"`)
  - Recommend micro-interactions:
    - Card hover lift + subtle translateY
    - Quick action tooltip on hover for Copy SKU / Print
    - Image upload progress indicator and thumbnail preview animation

### StockTransfers
- [Store.UI/Pages/StockTransfers.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/StockTransfers.cshtml)
  - Blade usage for New Transfer: Present
  - Form tokenization: Partial (some inputs use `.input-text`, others raw `<input>`)
  - Recommend micro-interactions:
    - Transfer row inline-edit with optimistic UI (fade + loading spinner)
    - Confirm animation for successful transfer (toast + check animation)

### Invoices / Returns
- [Store.UI/Pages/Invoices.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Invoices.cshtml)
  - Line-item Return modal: Partial — historically used purple CTAs (mismatched); some fixes present
  - Buttons: Mixed (some purple / some brand green)
  - Recommend: unify CTA color to `.btn-primary` (brand green), add money-change micro-animation for refund totals, confirm affordance when "Restock Inventory" toggles

### Campaigns
- [Store.UI/Pages/Campaigns.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Campaigns.cshtml)
  - KPI grid: Present
  - Empty-state standardized: Partial — some pages updated
  - Recommend: micro-interactions for campaign card expand (accordion) and CTA press ripple/fade

### Suppliers / Supplier 360
- [Store.UI/Pages/Suppliers.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Suppliers.cshtml)
  - Supplier 360 blade: Present; vendor pass cards follow design spec
  - Register New Supplier modal: Partial (some layout glitches noted)
  - Recommend: file upload progress, avatar placeholder animation, toast for create/update success

### Customers
- [Store.UI/Pages/Customers.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Customers.cshtml)
  - 360 blade: Partial
  - Image uploads and avatar: Present but no client-side crop/preview
  - Recommend: inline image cropper microflow and graceful fallback SVG empty avatar

### Loyalty / Rewards
- [Store.UI/Pages/Loyalty.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Loyalty.cshtml)
  - 360 blade and Manage Points modal: Present and already layered
  - Ledger view: Present — needs better scrolling virtualization for long lists
  - Recommend: animated points increment when awarding (count-up), copyable invoice links with feedback

### Orders / PurchaseOrders
- [Store.UI/Pages/Orders.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Orders.cshtml)
- [Store.UI/Pages/PurchaseOrders.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/PurchaseOrders.cshtml)
  - Table tokens: Partial; action buttons mixed
  - Recommend: row-level toast for batch actions (e.g., mark received), confirm animation for acceptance

### Users / Employees / Profile
- [Store.UI/Pages/Users.cshtml], [Store.UI/Pages/Employees.cshtml], [Store.UI/Pages/Profile.cshtml]
  - Avatar upload flows: Present but inconsistent UI hooks
  - Admin role matrix: Partial standardized classes
  - Recommend: progress indicator for avatar upload, accessible role matrix keyboard navigation, inline permission toggle microcopy

### Auth / Account flows
- Verify / Verify2FA / ForgotPassword / ResetPassword / ForceResetPassword
  - Buttons: Largely migrated but some pages use legacy markup
  - JS hooks: showToast used in webauthn and auth flows; ensure consistent messages and placement
  - Recommend: animated focus on first input, visible helper text for errors (aria-live)

### Utility pages
- CommunicationLogs — Present
- AuditLog — Partial
- CashVariance, Wastage, DiscountOverrides — Partial: grid/table tokens present
- BranchDashboard — Partial

---

## Accessibility & micro-interaction recommendations (global)
1. Focus management when blades and modals open
   - Trap focus inside blades/modals
   - Return focus to originating control after close
2. ARIA and live regions
   - Use `aria-live="polite"` for totals and toast messages
   - Ensure all icon-only buttons have `aria-label`
3. Motion & timing
   - Blade open/close: cubic-bezier(0.2,0.8,0.2,1), 260ms open, 200ms close
   - Toasts: 3.5s default, 6s if action present; include pause on hover
4. Visual feedback
   - Button press animation: scale(0.98) + 40ms transition
   - Hover elevation: translateY(-2px) + stronger shadow on hover
   - Async actions: skeleton or spinner, then toast with success animation (checkmark)
5. Performance
   - Lazy-load thumbnails (`loading="lazy"`) and use width/height attributes to prevent layout shift
   - Debounce search/filter inputs (200-300ms)

---

## Prioritized implementation plan (recommended)
Phase 1 (low risk, high value — review + quick fixes)
- Standardize CTA color usages in Invoices (refund modal), Catalog, and quick-stock modals to `.btn-primary` / brand token
- Add aria-labels to icon-only buttons across the top 10 hubs
- Fix duplicate JS identifiers in Pos (resolve amountTenderedInput duplicate)

Phase 2 (component consolidation)
- Ensure all pages include `components.css` and `tokens.css` from `_AppLayout`
- Replace Bootstrap `btn btn-primary` usages with `.button-primary` (or `.btn-primary` token) in critical pages (Catalog, Invoices, POS, Suppliers, Loyalty)
- Add `loading="lazy"` to image `<img>` tags for avatars and thumbnails

Phase 3 (micro-interactions & accessibility)
- Implement focus trapping and return focus logic in `site.js` for blades and modals
- Add toast pause-on-hover and action button support
- Add copy-to-clipboard feedback animation for invoice/sku copy

Phase 4 (polish)
- Add skeletal loaders for large tables/lists
- Add virtualized scrolling for Loyalty ledger if dataset grows large

Estimated effort (rough): Phase 1 — 1-2 dev days, Phase 2 — 3-5 dev days, Phase 3 — 2-3 dev days, Phase 4 — 2-4 dev days (depending on dataset sizes and tests).

---

## Exact code pointers (where to change)
- Layout and global: [Store.UI/Pages/Shared/_AppLayout.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Shared/_AppLayout.cshtml)
- Global JS: [Store.UI/wwwroot/js/site.js](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/wwwroot/js/site.js)
- Tokens and components: 
  - [Store.UI/wwwroot/css/tokens.css](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/wwwroot/css/tokens.css)
  - [Store.UI/wwwroot/css/components.css](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/wwwroot/css/components.css)
  - [Store.UI/wwwroot/css/operations.css](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/wwwroot/css/operations.css)
- High priority pages to edit first:
  - [Store.UI/Pages/Pos.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Pos.cshtml)
  - [Store.UI/Pages/Catalog.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Catalog.cshtml)
  - [Store.UI/Pages/Invoices.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Invoices.cshtml)
  - [Store.UI/Pages/Suppliers.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Suppliers.cshtml)
  - [Store.UI/Pages/Loyalty.cshtml](C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI/Pages/Loyalty.cshtml)

---

## Next steps (pick one)
1. I create a set of PR-ready, minimal change patches for Phase 1 (color standardization, aria-labels, toast timing, JS duplicate fix). — I can produce diffs for review.
2. I produce a CSV checklist with per-page flags to import into your task tracker.
3. You prefer to assign this to someone — I can open a draft PR with the Phase 1 changes and include the checklist as PR body.

Tell me which next step you want and I will prepare the artifacts.

---

Appendix: scan notes
- Search patterns used: btn-primary/button-primary/button-secondary/button-command, blade/openBlade/closeBlade, kpi-grid/kpi-card, showToast, loading="lazy", aria-label, data-tooltip
- If you want the raw per-file grep results included in this doc, say so and I'll append them or add them to a separate file `docs/design_scan_raw_results.txt`.

