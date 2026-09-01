# 00b — Store.TenantPortal: Design Language Reference

**Status:** Canonical (all other docs must reference this file)
**Version:** 1.0
**Date:** September 2026

> This document is the **single source of truth** for all visual and interaction design decisions
> in `Store.TenantPortal`. Every spec doc (01 through 06) defers to this file on any design question.

---

## 1. Visual Identity

The portal design language is **ClexAn Portal 1.0** — inspired directly by the existing
ClexAn Foods marketing experience (see: `/Store.UI` landing page). The core aesthetic is:

> **Premium dark-mode SaaS** — deep black background with a signature emerald-green accent,
> bold heavy type, and generous whitespace. Feels authoritative and trustworthy at first glance.

### 1.1 Background & Atmosphere

- **Page background:** `#050a05` — near-true black with a very subtle green tint
- **Hero radial glow:** `radial-gradient(ellipse 80% 60% at 50% -10%, rgba(34, 197, 94, 0.18), transparent)` — centred above the hero heading, bleeds into pure black
- **Glow is ONLY on the landing page hero section.** All other pages use a flat `#050a05` background.
- **Card surfaces:** `rgba(255, 255, 255, 0.04)` — barely-there glass. No heavy borders.

### 1.2 Color Tokens

```css
/* === ClexAn Portal 1.0 — Design Tokens === */

/* Backgrounds */
--p-bg:              #050a05;              /* Page background — near-black, green-tinted */
--p-surface:         rgba(255,255,255,0.04); /* Card/panel surface — glass effect */
--p-surface-hover:   rgba(255,255,255,0.07); /* Surface on hover */
--p-surface-alt:     rgba(255,255,255,0.02); /* Table alternating rows, input backgrounds */

/* Borders */
--p-border:          rgba(255,255,255,0.08); /* Subtle divider/card border */
--p-border-accent:   rgba(34,197,94,0.3);    /* Green-tinted border for active/focus states */

/* BRAND — Emerald Green (primary accent) */
--p-green:           #22c55e;              /* Primary CTA, active indicators, badges */
--p-green-hover:     #16a34a;             /* Hover state — deeper green */
--p-green-glow:      rgba(34,197,94,0.18);  /* Used for hero radial glow */
--p-green-chip:      rgba(34,197,94,0.15);  /* Icon chip background (feature cards) */

/* Status Colors */
--p-success:         #22c55e;              /* Same as --p-green */
--p-warning:         #f59e0b;
--p-danger:          #ef4444;
--p-info:            #3b82f6;

/* Typography */
--p-text:            #f1f5f9;              /* Primary text — off-white */
--p-text-muted:      #94a3b8;             /* Secondary text, metadata, captions */
--p-text-subtle:     #475569;             /* Disabled, placeholder text */

/* Feature Card Icon Chips — each feature card uses a different chip color */
--chip-green:        rgba(34,197,94,0.15);   /* POS, Environment, Domains */
--chip-teal:         rgba(20,184,166,0.15);  /* Inventory, Branches */
--chip-purple:       rgba(168,85,247,0.15);  /* Analytics, Backups */
```

**IMPORTANT:** There is **no blue accent** in this design. All primary interactive elements,
active states, badges, and CTA buttons use `--p-green` (`#22c55e`). Blue is reserved exclusively
for the `--p-info` status color (informational alerts only).

### 1.3 Typography

| Element | Font | Weight | Size | Notes |
|:---|:---|:---:|:---|:---|
| Hero headline | `Inter` | **900** | `clamp(3.5rem, 8vw, 6rem)` | Extra-bold, tight tracking `-0.03em` |
| Section heading (h2) | `Inter` | **700** | `clamp(2rem, 4vw, 3rem)` | Bold, tracking `-0.02em` |
| Card heading (h3) | `Inter` | **700** | `1.25rem` | |
| Body text | `Inter` | 400 | `1rem` | Line height `1.75` |
| Nav links | `Inter` | 500 | `0.9rem` | Muted color |
| Badge/label | `Inter` | 600 | `0.7rem` | Uppercase, `letter-spacing: 0.08em` |
| Code / slug / DNS | `JetBrains Mono` | 400 | `0.875rem` | Used for slugs, DNS records, URLs |

**Google Fonts import (in `_Layout.cshtml`):**
```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;900&family=JetBrains+Mono&display=swap" rel="stylesheet">
```

### 1.4 Iconography

- **Style:** Outline SVG icons, `stroke-width: 1.5`, `currentColor`
- **Size:** 20×20 (nav/body), 24×24 (feature card icons)
- **Source:** Consistent with Store.UI icon set — no icon font (no FontAwesome, no Bootstrap Icons)
- **Icon chips (feature cards):** Icon inside a `48×48` rounded square (`border-radius: 12px`),
  background set to the appropriate `--chip-*` color token

---

## 2. Component Specifications

### 2.1 Navbar

```
[🏪 ClexAn Foods]     Features   About                [Sign In →]
```

| Property | Value |
|:---|:---|
| Background | `transparent` → `rgba(5,10,5,0.85) backdrop-filter: blur(12px)` on scroll |
| Position | `fixed` top |
| Height | `64px` |
| Logo | Icon + wordmark, `font-weight: 700`, `color: --p-text` |
| Nav links | `color: --p-text-muted`, hover → `color: --p-text`, no underline |
| Sign In button | Pill shape, `border: 1px solid rgba(255,255,255,0.15)`, transparent bg, `→` icon |
| Get Started (landing only) | Solid `--p-green` pill button |

### 2.2 Pill Badge (Hero label)

```html
<span class="portal-badge">NEXT-GEN PLATFORM</span>
```

```css
.portal-badge {
  display: inline-block;
  padding: 4px 14px;
  border: 1px solid var(--p-green);
  border-radius: 999px;
  color: var(--p-green);
  font-size: 0.7rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}
```

### 2.3 Primary CTA Button (`btn-primary`)

Used for: `[Get Started →]`, `[Launch My Store →]`, `[Backup Now]`

```css
.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  background: var(--p-green);
  color: #000;                          /* Black text on green */
  font-weight: 700;
  font-size: 1rem;
  border: none;
  border-radius: 999px;                  /* Full pill */
  cursor: pointer;
  transition: background 0.2s, transform 0.15s;
}
.btn-primary:hover {
  background: var(--p-green-hover);
  transform: translateY(-1px);
}
```

### 2.4 Secondary CTA Button (`btn-secondary`)

Used for: `[Discover Features]`, `[Sign In →]`, `[Cancel]`

```css
.btn-secondary {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 14px 28px;
  background: transparent;
  color: var(--p-text);
  font-weight: 600;
  border: 1px solid rgba(255,255,255,0.15);
  border-radius: 999px;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
}
.btn-secondary:hover {
  border-color: rgba(255,255,255,0.35);
  background: rgba(255,255,255,0.05);
}
```

### 2.5 Feature / Info Card

Used on landing page (3-column grid) and Dashboard (health cards).

```css
.portal-card {
  background: var(--p-surface);
  border: 1px solid var(--p-border);
  border-radius: 16px;
  padding: 28px;
  transition: border-color 0.2s, background 0.2s;
}
.portal-card:hover {
  border-color: var(--p-border-accent);
  background: var(--p-surface-hover);
}
```

**Icon chip inside card:**
```css
.card-icon-chip {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 20px;
  /* Background set per-card using chip color tokens */
}
```

Card heading is `font-weight: 700`, body is `color: var(--p-text-muted)`.

### 2.6 Form Input

```css
.portal-input {
  width: 100%;
  padding: 12px 16px;
  background: var(--p-surface-alt);
  border: 1px solid var(--p-border);
  border-radius: 10px;
  color: var(--p-text);
  font-size: 0.95rem;
  transition: border-color 0.2s;
}
.portal-input:focus {
  outline: none;
  border-color: var(--p-green);
  box-shadow: 0 0 0 3px var(--p-green-glow);
}
```

### 2.7 Status Indicator Dot

Used on health cards, domain status, branch status:

```css
.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  display: inline-block;
}
.status-dot.healthy  { background: var(--p-green); box-shadow: 0 0 6px var(--p-green); }
.status-dot.warning  { background: var(--p-warning); }
.status-dot.error    { background: var(--p-danger); }
.status-dot.checking { background: var(--p-warning); animation: pulse 1.5s infinite; }
```

### 2.8 Section Divider Pattern

Between major page sections, use a subtle gradient fade:
```css
.section-divider {
  height: 1px;
  background: linear-gradient(to right, transparent, var(--p-border), transparent);
  margin: 80px 0;
}
```

---

## 3. Hero Section (Landing Page Only)

```html
<!-- Hero background with radial glow -->
<section class="hero">
  <div class="hero-glow"></div>  <!-- Positioned radial gradient overlay -->
  <span class="portal-badge">NEXT-GEN PLATFORM</span>
  <h1>Elevate Your Store<br>Operations</h1>
  <p class="hero-sub">Experience a fluid, real-time store management system...</p>
  <div class="hero-ctas">
    <a href="/register" class="btn-primary">Get Started →</a>
    <a href="#features" class="btn-secondary">Discover Features</a>
  </div>
</section>
```

```css
.hero {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 120px 24px 80px;
  position: relative;
  overflow: hidden;
}
.hero-glow {
  position: absolute;
  top: -20%;
  left: 50%;
  transform: translateX(-50%);
  width: 80%;
  height: 60%;
  background: radial-gradient(ellipse, rgba(34,197,94,0.18) 0%, transparent 70%);
  pointer-events: none;
}
```

---

## 4. Page Layout — Authenticated Pages (Dashboard onwards)

Authenticated pages (Dashboard, Environment, Domains, Branches, Backups, Settings)
use a **flat top nav + content area** layout — no sidebar. The hero glow is absent.

```
┌──────────────────────────────────────────────────────────────────┐
│  [🏪 ClexAn]  Dashboard · Environment · Domains · Branches ·     │  (64px top nav)
│               Backups                          [● Healthy] [⊕]   │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  PAGE CONTENT  (max-width: 1200px, centred, 48px horizontal pad) │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

- **Active nav link:** `color: --p-green`, left `2px` green underline
- **Health indicator in nav (top right):** `● Healthy` — green dot + muted text
- **No sidebar** — all authenticated pages are single-column content areas

---

## 5. Animation & Motion

| Animation | Description | CSS |
|:---|:---|:---|
| Hero headline entrance | Fade up 20px, 0.6s ease-out, 0.1s delay | `@keyframes fadeUp` |
| Badge entrance | Fade in, 0.4s, no delay | `opacity 0→1` |
| CTA button hover | Lift `-2px`, 0.15s | `transform: translateY(-2px)` |
| Card hover | Border color → green-tinted, 0.2s | `border-color` transition |
| Input focus | Green border + soft green glow, 0.2s | `box-shadow` transition |
| Status dot pulse (checking) | Scale 1→1.4→1, 1.5s loop | `@keyframes pulse` |
| Page-level nav scroll | Nav bg fades from transparent to `blur(12px)` | `IntersectionObserver` JS |
| Form submit spinner | Button icon rotates, text hidden | `@keyframes spin` |
| Toast notification | Slide in from right 0.3s, auto-dismiss 5s | `@keyframes slideInRight` |

**NO abrupt transitions.** All state changes must feel smooth. Minimum `transition: 0.2s ease`.

---

## 6. Responsive Breakpoints

| Breakpoint | Width | Changes |
|:---|:---|:---|
| Mobile | < 640px | Single column layouts, hero font smaller, nav collapses to hamburger |
| Tablet | 640–1024px | 2-column grids, reduced padding |
| Desktop | > 1024px | Full 3-column grids, max-width 1200px centred |

---

## 7. Cross-Document Consistency Rules

All 6 specification documents must conform to the following rules:

| Rule | Detail |
|:---|:---|
| **Primary accent = green** | `#22c55e`. Zero blue (`#2f81f7`) for interactive elements. Blue only for `--p-info` alerts. |
| **Background = near-black** | `#050a05`. Not GitHub dark blue (`#0d1117`). The green tint distinguishes the portal. |
| **CTA = pill shape** | `border-radius: 999px`. No square buttons. |
| **Card borders = glass** | `rgba(255,255,255,0.08)`. No solid 1px grey borders like Store.UI. |
| **Icon chips** | Always in a 48×48 rounded chip, chip BG from `--chip-*` token. Never raw icons. |
| **Nav = transparent + blur** | Not a solid dark bar. Glass-blur effect on scroll. |
| **Heading weight = 900** | Hero and section headings use `font-weight: 900` (black). Not 600 or 700. |
| **No external CDN scripts** | All JS bundled in `wwwroot/js/`. Eliminates SRI issues. |
