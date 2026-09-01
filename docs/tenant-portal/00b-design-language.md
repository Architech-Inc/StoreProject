# 00b — Store.TenantPortal: Design Language Reference

**Status:** Canonical (all other docs must reference this file)
**Version:** 1.1 — Updated to match exact tokens from Store.UI landing.css
**Date:** September 2026

> This document is the **single source of truth** for all visual and interaction design decisions
> in `Store.TenantPortal`. Every spec doc (01 through 06) defers to this file on any design question.

---

## 1. Visual Identity

The portal design language is **ClexAn Portal 1.0** — inspired directly by the existing
ClexAn Foods marketing experience (see: `/Store.UI` landing page). The core aesthetic is:

> **Premium dark-mode SaaS** — deep black background with a signature ClexAn green accent,
> glassmorphism elements, `Outfit` font for headings, and generous whitespace. Feels authoritative 
> and trustworthy at first glance.

### 1.1 Background & Atmosphere

- **Page background:** `#050906` — near-true black with a very subtle green tint
- **Hero radial glow:** `radial-gradient(circle, rgba(1, 156, 1, 0.4) 0%, rgba(1, 156, 1, 0) 60%)` — centred above the hero heading, bleeds into pure black with `blur(80px)` and an 8s pulsing animation.
- **Glow is ONLY on the landing page hero section.** All other pages use a flat `#050906` background.
- **Card surfaces:** `rgba(255, 255, 255, 0.03)` — barely-there glass with a `blur(12px)` backdrop filter.

### 1.2 Color Tokens

```css
/* === ClexAn Portal 1.0 — Design Tokens === */

/* Backgrounds */
--p-bg:              #050906;              /* Page background */
--p-surface:         rgba(255, 255, 255, 0.03); /* Card/panel surface — glass effect */
--p-surface-hover:   rgba(255, 255, 255, 0.05); /* Surface on hover */
--p-surface-alt:     rgba(255, 255, 255, 0.02); /* Table alternating rows, input backgrounds */

/* Borders */
--p-border:          rgba(255, 255, 255, 0.08); /* Subtle divider/card border */
--p-border-accent:   rgba(1, 156, 1, 0.3);    /* Green-tinted border for active/focus states */
--p-border-hover:    rgba(255, 255, 255, 0.15); /* Border on card hover */

/* BRAND — ClexAn Green (primary accent) */
--p-green:           #019c01;              /* Primary CTA, active indicators, badges */
--p-green-hover:     #02b802;             /* Hover state */
--p-green-glow:      rgba(1, 156, 1, 0.4);  /* Used for hero radial glow and button box-shadow */
--p-green-chip:      linear-gradient(135deg, rgba(1, 156, 1, 0.2), rgba(1, 156, 1, 0.05)); /* Icon chip background */

/* Status Colors */
--p-success:         #019c01;              /* Same as --p-green */
--p-warning:         #f59e0b;
--p-danger:          #d13438;              /* From existing landing.css accent */
--p-info:            #3b82f6;

/* Typography */
--p-text:            #ffffff;              /* Primary text */
--p-text-muted:      #a1b0a6;             /* Secondary text, metadata, captions */
--p-text-subtle:     #475569;             /* Disabled, placeholder text */

/* Feature Card Icon Chips — each feature card uses a different chip color */
--chip-green:        linear-gradient(135deg, rgba(1, 156, 1, 0.2), rgba(1, 156, 1, 0.05));
--chip-teal:         linear-gradient(135deg, rgba(20,184,166,0.2), rgba(20,184,166,0.05));
--chip-purple:       linear-gradient(135deg, rgba(168,85,247,0.2), rgba(168,85,247,0.05));
```

**IMPORTANT:** All primary interactive elements, active states, badges, and CTA buttons use `--p-green` (`#019c01`). Blue is reserved exclusively for the `--p-info` status color.

### 1.3 Typography

**Two distinct font families are used (imported from Google Fonts):**
- **Headings & Logos:** `Outfit`
- **Body & UI Elements:** `Inter`

| Element | Font | Weight | Size | Notes |
|:---|:---|:---:|:---|:---|
| Hero headline | `Outfit` | **800** | `clamp(3rem, 6vw, 5.5rem)` | Gradient fill: `#fff` to `#a1b0a6` |
| Section heading (h2) | `Outfit` | **800** | `2.5rem` | |
| Card heading (h3) | `Outfit` | **600** | `1.4rem` | |
| Body text | `Inter` | 400 | `0.95rem` | Line height `1.6` |
| Nav links | `Inter` | 500 | `15px` | Muted color |
| Badge/label | `Inter` | 600 | `13px` | Uppercase, `letter-spacing: 0.5px` |
| Code / slug / DNS | `JetBrains Mono` | 400 | `0.875rem` | Used for slugs, DNS records, URLs |

**Google Fonts import:**
```css
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;600;800&family=Inter:wght@400;500;600&display=swap');
```

### 1.4 Iconography

- **Style:** Outline SVG icons, `stroke-width: 1.5`, `currentColor`
- **Size:** 20×20 (nav/body), 24×24 (feature card icons)
- **Source:** Consistent with Store.UI icon set
- **Icon chips (feature cards):** Icon inside a `56×56` rounded square (`border-radius: 16px`), background set to `--chip-*` gradient, with `1px solid` matching border color (e.g. `rgba(1, 156, 1, 0.3)`).

---

## 2. Component Specifications

### 2.1 Navbar

```
[🏪 ClexAn Foods]     Features   About                [Sign In →]
```

| Property | Value |
|:---|:---|
| Background | `rgba(5, 9, 6, 0.6)` |
| Filter | `-webkit-backdrop-filter: blur(16px); backdrop-filter: blur(16px);` |
| Position | `fixed` top, `border-bottom: 1px solid var(--p-border)` |
| Height | `64px` |
| Logo | `Outfit`, `font-weight: 800`, gradient text fill |
| Nav links | `Inter`, `color: var(--p-text-muted)`, hover → `color: #fff` |
| Sign In button | Pill shape (`.btn-glass`), `backdrop-filter: blur(10px)` |

### 2.2 Pill Badge (Hero label)

```html
<div class="hero-pill">NEXT-GEN PLATFORM</div>
```

```css
.hero-pill {
  display: inline-block;
  padding: 6px 16px;
  border-radius: 99px;
  background: rgba(1, 156, 1, 0.1);
  border: 1px solid rgba(1, 156, 1, 0.2);
  color: #4ade80;
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}
```

### 2.3 Primary CTA Button (`.btn-primary-glow`)

```css
.btn-primary-glow {
  padding: 14px 32px;
  border-radius: 99px;
  background: var(--p-green);
  color: #fff;
  font-family: 'Inter', sans-serif;
  font-weight: 600;
  font-size: 16px;
  border: none;
  box-shadow: 0 4px 20px var(--p-green-glow);
  transition: all 0.3s ease;
}
.btn-primary-glow:hover {
  background: var(--p-green-hover);
  transform: translateY(-2px);
  box-shadow: 0 8px 30px var(--p-green-glow);
}
```

### 2.4 Secondary CTA Button (`.btn-glass`)

```css
.btn-glass {
  padding: 10px 24px;
  border-radius: 99px;
  background: var(--p-surface);
  border: 1px solid var(--p-border);
  color: #fff;
  font-family: 'Inter', sans-serif;
  font-weight: 600;
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
}
.btn-glass:hover {
  background: var(--p-surface-hover);
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  border-color: rgba(255, 255, 255, 0.2);
}
```

### 2.5 Feature / Info Card

```css
.feature-card {
  background: var(--p-surface);
  border: 1px solid var(--p-border);
  border-radius: 24px;
  padding: 32px;
  backdrop-filter: blur(12px);
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.feature-card:hover {
  transform: translateY(-8px);
  background: rgba(255, 255, 255, 0.05);
  border-color: var(--p-border-hover);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
}
```

---

## 3. Hero Section (Landing Page Only)

```html
<section class="hero-section">
  <div class="landing-bg-glow"></div>
  <div class="hero-pill">NEXT-GEN PLATFORM</div>
  <h1 class="hero-title">Elevate Your Store<br>Operations</h1>
  <p class="hero-subtitle">Experience a fluid, real-time store management system...</p>
  <div class="hero-actions">
    <a href="/register" class="btn-primary-glow">Get Started →</a>
    <a href="#features" class="btn-glass">Discover Features</a>
  </div>
</section>
```

---

## 4. Cross-Document Consistency Rules

All 6 specification documents must conform to the following rules:

| Rule | Detail |
|:---|:---|
| **Primary accent = ClexAn Green** | `#019c01`. Zero blue. Blue only for `--p-info`. |
| **Background = near-black** | `#050906`. Not GitHub dark blue (`#0d1117`). |
| **CTA = pill shape** | `border-radius: 99px`. |
| **Card borders = glass** | `rgba(255,255,255,0.08)`. Hover uses shadow and `-8px` lift. |
| **Heading Font = Outfit** | Headings use `Outfit`, weight `800` for hero/h2, `600` for h3. |
| **Body Font = Inter** | Body text uses `Inter`. |
| **Nav = glass blur** | `rgba(5, 9, 6, 0.6)` with `blur(16px)`. |
