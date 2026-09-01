# 02 — Store.TenantPortal UX & Design Specification

**Status:** Draft
**Version:** 1.1 — Updated to match reference screenshot
**Date:** September 2026

> **Design Language:** All color tokens, component specifications, animation rules, and
> layout decisions are defined in **[00b-design-language.md](00b-design-language.md)**.
> This document references that canonical source and applies it page-by-page.

---

## 1. Design Language Summary

The portal follows **ClexAn Portal 1.0** — a premium dark SaaS aesthetic matching the
existing ClexAn Foods landing page design:

| Token | Value | Use |
|:---|:---|:---|
| Page background | `#050a05` | Near-black with green tint |
| Primary accent | `#22c55e` | CTAs, active states, badges — **GREEN, not blue** |
| Hero glow | `rgba(34,197,94,0.18)` radial gradient | Landing page hero only |
| Card surface | `rgba(255,255,255,0.04)` | Glass-effect cards |
| Hero heading weight | **900** | Extra-bold / black weight |
| CTA shape | `border-radius: 999px` | Full pill buttons |

See [00b-design-language.md](00b-design-language.md) for complete CSS tokens, component specs,
animation definitions, and cross-document consistency rules.

---

## 2. Page-by-Page Specifications

---

### 2.1 Landing Page (`/`)

**Purpose:** Public marketing page — convert visitors into registered portal users.

**Visual Atmosphere:**
- Full-height hero with `#050a05` background and a radial emerald glow centred above the headline
- Glow fades completely to black before the feature card section
- The glow is **exclusive to this page** — all other pages are flat black

**Navbar:**
```
[🏪 ClexAn Foods]         Features   About               [Sign In →]
```
- Transparent background on load → `blur(12px)` glass on scroll
- `[Sign In →]` = secondary pill button (outlined, no fill)
- No `[Get Started]` button in the navbar — CTA lives in the hero only

**Hero Section:**
```
                    ┌──────────────────────────┐
                    │  NEXT-GEN PLATFORM        │  ← green pill badge
                    └──────────────────────────┘

          Elevate Your Store
              Operations                         ← font-weight: 900, clamp(3.5rem→6rem)

   Experience a fluid, real-time store management system.
    Unify your Point of Sale, Inventory, and Analytics
                  into one premium platform.             ← --p-text-muted, max-width 580px

         [Get Started →]    [Discover Features]          ← pill CTAs, centred
         ▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔   ▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔
         solid green pill     outlined dark pill
```

**Feature Cards Section (below hero):**
```
                    Built for Scale & Speed                ← h2, font-weight: 700
         Everything you need to run your retail locations
                   smoothly and efficiently.               ← --p-text-muted

  ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐
  │  ┌──────┐            │  │  ┌──────┐            │  │  ┌──────┐            │
  │  │  🏪  │ green chip │  │  │  📦  │ teal chip  │  │  │  📊  │ purple chip│
  │  └──────┘            │  │  └──────┘            │  │  └──────┘            │
  │                      │  │                      │  │                      │
  │  Intelligent POS      │  │  Inventory Control   │  │  Live Analytics      │
  │  font-weight: 700     │  │  font-weight: 700    │  │  font-weight: 700    │
  │                      │  │                      │  │                      │
  │  A lightning-fast    │  │  Real-time stock     │  │  Make data-driven    │
  │  point of sale...    │  │  tracking across...  │  │  decisions with...   │
  └─────────────────────┘  └─────────────────────┘  └─────────────────────┘
  glass surface, hover → green-tinted border
```

**Feature card copy (matches the reference image):**
- **Intelligent POS** — "A lightning-fast point of sale system designed for high volume environments. Process transactions, manage discounts, and handle returns with ease."
- **Inventory Control** — "Real-time stock tracking across multiple branches. Automate purchase orders, manage stock transfers, and minimize wastage."
- **Live Analytics** — "Make data-driven decisions with real-time dashboards. Track cash variances, promotion effectiveness, and store performance at a glance."

**Footer:**
```
© 2026 ClexAn Foods. All rights reserved.              Privacy Policy   Terms of Service
```
Minimal, single line. `--p-text-subtle`.

**Interactions:**
- `[Get Started →]` → `/register`
- `[Discover Features]` → smooth scroll to `#features`
- Navbar becomes glass on scroll (JS `IntersectionObserver` watching hero bottom edge)
- Hero headline and badge animate in with `fadeUp` on page load

---

### 2.2 Register Page (`/register`)

**Layout:** Centered card, max-width `480px`, flat `#050a05` background (no glow).

```
                    [🏪 ClexAn Foods]               ← minimal header

       Create your portal account
       ─────────────────────────────

       Full Name          [                         ]
       Email Address      [                         ]
       Password           [                         ] [👁]
       Confirm Password   [                         ] [👁]

       Password strength:  ████░░░░  Fair             ← animated bar
                           red → orange → yellow → green

       [✓] I agree to the Terms of Service and Privacy Policy

                              [Create Account →]       ← primary green pill

       Already have an account?  Sign In
```

- All inputs styled with `--p-surface-alt` background, green focus ring
- Password strength bar colour transitions through `--p-danger` → `--p-warning` → `--p-green`
- `[Create Account →]` disabled (50% opacity) until all fields valid + checkbox checked

---

### 2.3 Login Page (`/login`)

```
                    [🏪 ClexAn Foods]

       Sign in to your portal
       ──────────────────────

       Email              [                         ]
       Password           [                         ] [👁]

       Forgot password?

                              [Sign In →]              ← primary green pill

       Don't have an account?  Register
```

- Invalid credentials → red inline error below Password (not above — never reveals email validity)
- `[Sign In →]` shows green spinner + disabled on submit

---

### 2.4 Onboarding Wizard (`/onboarding`)

**Step indicator (top of page):**
```
●──────○──────○──────○
①      ②      ③      ④
Acct  Store  Domain Launch
```

Active step = filled green circle. Completed = green with checkmark. Upcoming = outlined muted.

**Step 1 — Account:**
```
  Welcome, Alice!

  Set your store admin credentials:
  ─────────────────────────────────
  Admin Username  [admin                   ]
  Admin Password  [                        ] [👁]
  Confirm         [                        ] [👁]

  These are the credentials to log into your store, separate from your portal account.

                                      [Next →]   ← green pill
```

**Step 2 — Your Store:**
```
  Name your store
  ───────────────
  Store Name     [Acme Foods Ltd              ]
  Store Slug     [acme-foods].store.domain    ← live avail. check, green ✓ / red ✗
  Currency       [XAF ▾]
  Plan
    (●) Starter         (○) Professional       (○) Enterprise
         Free                $29/mo                  $99/mo

               [← Back]                 [Next →]
```

Slug availability: debounced 500ms AJAX, inline badge replaces label:
- `✓ Available` — `--p-green` text
- `✗ Already taken` — `--p-danger` text

**Step 3 — Domain:**
```
  Choose your store URL
  ─────────────────────
  (●) Use ClexAn platform subdomain
      Store: https://acme-foods.store.domain
      Branches: https://[branch].acme-foods.store.domain

  (○) Use my own domain
      Domain: [acme-foods.com             ]
      → After saving:
        ┌──────────────────────────────────────────┐
        │  Add this DNS TXT record:                │
        │  Name:   _clexan-verify.acme-foods.com  │
        │  Value:  clxv_4a8f...      [Copy]        │
        │  TTL:    300                             │
        └──────────────────────────────────────────┘
        [⟳ Check DNS Now]

  You can always change this from the Domains page.

               [← Back]                 [Next →]
```

DNS instructions box: `--p-surface`, `border: 1px solid --p-border-accent` (green-tinted border).

**Step 4 — Confirm & Launch:**
```
  Review your setup
  ─────────────────
  ┌─────────────────────────────────────────────┐
  │  Store Name:   Acme Foods Ltd               │
  │  Slug:         acme-foods                   │  ← JetBrains Mono
  │  Plan:         Professional                 │
  │  Currency:     XAF                          │
  │  Store URL:    https://acme-foods.store.dom │  ← JetBrains Mono
  └─────────────────────────────────────────────┘

               [← Back]       [🚀 Launch My Store]    ← primary green pill

  On launch → button becomes spinner → animated checklist:
  ✓  Request validated
  ✓  Secrets generated
  ✓  Compose blueprint created
  ⟳  Deploying containers...          ← pulsing green dot
  ○  Health check pending
  ○  Store ready
```

---

### 2.5 Dashboard (`/dashboard`)

**Top Nav (authenticated):**
```
[🏪 ClexAn Foods]  Dashboard · Environment · Domains · Branches · Backups  [● Healthy] [⊕]
```
- Active page link = `--p-green` text + `2px` green bottom border
- `[● Healthy]` = status dot + `--p-text-muted` text, right side

**Content:**
```
  Welcome back, Admin                                              ● Silo Healthy
  Acme Foods Ltd  ·  Professional  ·  acme-foods

  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
  │  ┌──────┐    │  │  ┌──────┐    │  │  ┌──────┐    │  │  ┌──────┐    │
  │  │  🗄️  │ G  │  │  │  🗄️  │ T  │  │  │  ⚡  │ G  │  │  │  🖥️  │ G  │
  │  └──────┘    │  │  └──────┘    │  │  └──────┘    │  │  └──────┘    │
  │  MySQL        │  │  MongoDB     │  │  API          │  │  UI          │
  │  ● Healthy    │  │  ● Healthy   │  │  ● Healthy    │  │  ● Healthy   │
  │  5 min ago    │  │  5 min ago   │  │  5 min ago    │  │  5 min ago   │
  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘
  (G=green chip, T=teal chip — using --chip-* tokens)

  Quick Actions
  [Open Store ↗]   [Open API ↗]   [Force Health Check]

  ┌────────────────────────────────┐  ┌────────────────────────────────┐
  │  Domain Summary                │  │  Backup Summary                │
  │  Platform:  acme.store...  ✓   │  │  Google Drive  ● Active        │
  │  Custom:    acme-foods.com ✓   │  │  Last backup: 2h ago           │
  │  Branches:  3 configured       │  │  [Manage Backups →]            │
  │  [Manage Domains →]            │  └────────────────────────────────┘
  └────────────────────────────────┘

  Health auto-refresh: every 60s via fetch → updates card dots and timestamps
```

---

### 2.6 Environment Control Panel (`/environment`)

```
  Environment Control Panel
  Acme Foods Ltd  ·  acme-foods  ·  ● Healthy

  ┌───────────────────────────────────────┐  ┌──────────────────────────┐
  │  CONTAINER STATUS                     │  │  ACTIONS                 │
  │                                       │  │                          │
  │  ● MySQL    acme-mysql    ✓ Healthy   │  │  [↺ Restart All]         │
  │    Last: 2026-09-01 10:01             │  │  [↺ Restart API]         │
  │                                       │  │  [↺ Restart UI]          │
  │  ● MongoDB  acme-mongodb  ✓ Healthy   │  │  [⚡ Force Health Check]  │
  │    Last: 2026-09-01 10:01             │  └──────────────────────────┘
  │                                       │
  │  ● API      acme-api      ✓ Healthy   │  ┌──────────────────────────┐
  │    api.acme.store.domain              │  │  SILO LINKS              │
  │    Last: 2026-09-01 10:01             │  │  Store UI  [Open ↗]      │
  │                                       │  │  API       [Open ↗]      │
  │  ● UI       acme-ui       ✓ Healthy   │  │  API Docs  [Open ↗]      │
  │    acme.store.domain                  │  └──────────────────────────┘
  │    Last: 2026-09-01 10:01             │
  └───────────────────────────────────────┘  ┌──────────────────────────┐
                                             │  DANGER ZONE             │
                                             │  border: --p-danger      │
                                             │  [⏸ Suspend Silo]        │
                                             └──────────────────────────┘

  PROVISIONING LOG TIMELINE
  ─────────────────────────
  ● 2026-09-01 09:00  ─── Validation         — ✓ Request validated
  ● 2026-09-01 09:00  ─── SecretGeneration   — ✓ Secrets generated
  ● 2026-09-01 09:00  ─── ComposeGeneration  — ✓ Blueprint written
  ● 2026-09-01 09:01  ─── DockerDeployment   — ✓ Containers started
  ● 2026-09-01 09:02  ─── HealthCheck        — ✓ All services healthy
  (dots are --p-green, line connecting them is --p-border)
```

---

### 2.7 Domain Manager (`/domains`)

```
  Domain Management

  PLATFORM SUBDOMAIN (always active)
  ┌──────────────────────────────────────────────────────────────┐
  │  ● Store URL:  https://acme-foods.store.domain               │
  │  ● API URL:    https://api.acme-foods.store.domain           │
  │  [Copy Store URL]   [Open Store ↗]                           │
  └──────────────────────────────────────────────────────────────┘

  CUSTOM DOMAIN
  ┌──────────────────────────────────────────────────────────────┐
  │  Status: ● Verified                                           │  ← green dot
  │  Domain: acme-foods.com                    JetBrains Mono    │
  │  Verified: 2026-09-01 09:15 UTC                              │
  │  [Remove Custom Domain]  ← red outlined pill                 │
  └──────────────────────────────────────────────────────────────┘

  ──── OR (pending) ────
  ┌──────────────────────────────────────────────────────────────┐
  │  Status: ⚠ Pending Verification           ← --p-warning dot  │
  │                                                               │
  │  Add this DNS TXT record at your domain registrar:            │
  │  ┌──────────────────────────────────────────────────────┐    │
  │  │  Type:   TXT                                         │    │
  │  │  Name:   _clexan-verify.acme-foods.com   [Copy]      │    │  ← JetBrains Mono
  │  │  Value:  clxv_4a8f3bd9...c72e            [Copy]      │    │  ← JetBrains Mono
  │  │  TTL:    300                                         │    │
  │  └──────────────────────────────────────────────────────┘    │
  │  border: --p-border-accent (green-tinted)                    │
  │                                                               │
  │  [⟳ Check DNS Now]   [Cancel]                                │
  └──────────────────────────────────────────────────────────────┘
```

`[Check DNS Now]` response:
- Spinner → ✓ slide-down green banner: "Domain verified! Traefik routing updated."
- Spinner → ✗ red card: "Not found. Expected: `clxv_4a8f...` Found: (none)"

---

### 2.8 Branch Manager (`/branches`)

```
  Branch Subdomain Management
  Map your store branches to their own URLs.

  [+ Add Branch]  ← green pill button

  ┌────────────────────────────────────────────────────────────────────┐
  │ Branch      │ URL                                 │ Status  │ Acts │
  ├─────────────┼─────────────────────────────────────┼─────────┼──────│
  │ HQ          │ hq.acme-foods.store.domain          │ ● Live  │ [✕]  │
  │ Northgate   │ northgate.acme-foods.store.domain   │ ● Live  │ [✕]  │
  │ Mfoundi     │ mfoundi.acme-foods.com              │ ⚠ Verify│ [▶✕] │
  └────────────────────────────────────────────────────────────────────┘
  Alternating rows: --p-surface / --p-surface-alt

  ADD BRANCH DRAWER (slides in from right, glass panel):
  ┌────────────────────────────────────────────┐
  │  Add Branch Mapping                      ✕ │
  │  ─────────────────────────────────────────  │
  │  Branch Name:   [HQ                      ] │
  │  Branch Slug:   [hq                      ] │
  │                                            │
  │  Subdomain:                                │
  │  (●) Platform  hq.[slug].store.domain      │
  │  (○) Custom    [hq.acme-foods.com        ] │
  │      (Requires CNAME pointing to          │
  │       acme-foods.store.domain)            │
  │                                            │
  │         [Cancel]          [Add Branch]     │
  └────────────────────────────────────────────┘
```

---

### 2.9 Backup Configuration (`/backups`)

```
  Backup Configuration

  PROVIDER CARDS (3-column grid, same card style as landing)

  ┌─────────────────────────┐  ┌─────────────────────────┐  ┌─────────────────────────┐
  │  ┌──────┐               │  │  ┌──────┐               │  │  ┌──────┐               │
  │  │  ☁  │ blue chip     │  │  │  ▲  │ teal chip     │  │  │  📦  │ green chip    │
  │  └──────┘               │  │  └──────┘               │  │  └──────┘               │
  │  OneDrive                │  │  Google Drive            │  │  S3 / MinIO             │
  │  Status: Disconnected    │  │  Status: ● Connected     │  │  Status: Configured     │
  │  [Connect OneDrive →]    │  │  Folder: /ClexAn...     │  │  Bucket: store-backups  │
  │  ← green pill            │  │  Last: 2h ago ✓         │  │  [Edit]   [Backup Now]  │
  │                          │  │  [Disconnect] [Backup↑] │  │                         │
  └─────────────────────────┘  └─────────────────────────┘  └─────────────────────────┘

  SCHEDULE
  ┌──────────────────────────────────────────────────────────────────────┐
  │  Run backups: [Every night at ▾]  [02:00 UTC ▾]                      │
  │  Retention:   [7 days ▾]                                             │
  │  Covers: MySQL + MongoDB (both databases per run)                    │
  │                                                   [Save Schedule]    │
  └──────────────────────────────────────────────────────────────────────┘

  BACKUP HISTORY
  ┌──────────────────────────────────────────────────────────────────┐
  │  Date                  │ DB      │ Provider     │ Size  │ Status │
  │  2026-09-01 02:00 UTC  │ MySQL   │ Google Drive │ 450MB │ ✓      │
  │  2026-09-01 02:30 UTC  │ MongoDB │ Google Drive │ 120MB │ ✓      │
  └──────────────────────────────────────────────────────────────────┘
```

`[Connect OneDrive →]` opens Microsoft OAuth in centred popup (800×600). On success,
popup sends `postMessage` to parent and closes. Parent refreshes card via `fetch`.

---

### 2.10 Settings (`/settings`)

```
  Account Settings
  ────────────────────────────────────────────────────────

  Full Name       [Admin User              ]  [Save]
  Email           [admin@acme-foods.com    ]  (contact support to change)

  Change Password
  ───────────────────────────────
  Current         [                        ]
  New             [                        ]
  Confirm         [                        ]
                                              [Update Password]

  PLAN
  ───────────────────────────────
  Current Plan:  Professional ($29/mo)
  Currency:      XAF (FCFA)
  [Upgrade to Enterprise]  ← green pill

  DANGER ZONE
  ───────────────────────────────
  ┌─────────────────────────────────────────────────────────────────┐
  │  ⚠ Permanently delete this store                                │
  │  All data, databases, uploads, and backups will be lost.        │
  │  border: 1px solid --p-danger                                   │
  │                                                                 │
  │  Type store name to confirm:  [                    ]            │
  │                                             [Delete Store]      │
  │                                      ← red pill, enabled only   │
  │                                        when name matches exactly│
  └─────────────────────────────────────────────────────────────────┘
```

---

## 3. Micro-Interactions & Feedback Patterns

| Interaction | Pattern | Token/Color |
|:---|:---|:---|
| Form submit | Button → green spinner + disabled; re-enables on response | `--p-green` |
| Slug availability | Inline `✓ Available` / `✗ Taken` badge, 500ms debounce | `--p-green` / `--p-danger` |
| Password strength | Animated bar: 4 segments, colour transitions red→green | `--p-danger→--p-green` |
| DNS check result | Slide-down card below button; green ✓ or red ✗ with found/expected diff | `--p-green` / `--p-danger` |
| Health dot (healthy) | Pulsing green glow — `box-shadow: 0 0 6px --p-green` | `--p-green` |
| Health dot (checking) | Scale 1→1.4→1, 1.5s loop | `--p-warning` |
| Provision progress | Step-by-step checklist; completed=green ✓, current=pulsing dot, pending=grey | `--p-green` |
| Copy button | Text changes "Copy" → "Copied!" for 2s with green check → reverts | `--p-green` |
| Danger delete | `[Delete]` disabled until input exactly matches store name; red pill | `--p-danger` |
| Toast success | Slide from top-right, green left border, 5s auto-dismiss | `--p-green` |
| Toast error | Slide from top-right, red left border, stays until dismissed | `--p-danger` |
| OAuth popup | `window.open(url, '_blank', 'width=800,height=600,centered')` | — |
| Nav scroll | `backdrop-filter: blur(12px)` fades in on first scroll | `IntersectionObserver` |
| Card hover | `border-color` → `--p-border-accent`, smooth 0.2s | `--p-border-accent` |
| CTA hover | Lift `translateY(-1px)`, green darkens to `--p-green-hover` | `--p-green-hover` |
