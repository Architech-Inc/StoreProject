# 02 — Store.TenantPortal UX & Design Specification

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

---

## 1. Design Language

The portal uses **ClexAn Portal 1.0** — a lighter, public-facing variant of ClexAn Fluent 2.0
used in the main Store.UI. It shares color tokens but has a different layout optimised for
a marketing-to-onboarding journey.

### 1.1 Color Tokens

```css
/* Portal design tokens */
--portal-bg:           #0d1117;     /* Deep dark background */
--portal-surface:      #161b22;     /* Card / panel surface */
--portal-surface-alt:  #21262d;     /* Secondary surface, table rows */
--portal-border:       #30363d;     /* Subtle dividers */
--portal-accent:       #2f81f7;     /* Primary blue — CTAs, active states */
--portal-accent-hover: #388bfd;
--portal-success:      #3fb950;
--portal-warning:      #d29922;
--portal-danger:       #f85149;
--portal-text:         #e6edf3;     /* Primary text */
--portal-text-muted:   #8b949e;     /* Metadata, labels */
--portal-text-subtle:  #484f58;     /* Disabled / placeholder */
```

### 1.2 Typography

- **Font family:** `Inter` (Google Fonts, loaded via `<link>` in `_Layout.cshtml`)
- **Headings:** `font-weight: 600`, `letter-spacing: -0.02em`
- **Body:** `font-size: 0.9rem`, `line-height: 1.6`
- **Code/slugs:** `font-family: 'JetBrains Mono', monospace` — used for slugs, DNS records, URLs

### 1.3 Iconography

SVG inline icons throughout — no icon font dependencies. Icon set matches the existing
Store.UI SVG standard (18×18 viewport, `stroke-width: 1.5`, `currentColor`).

---

## 2. Page-by-Page Specifications

---

### 2.1 Landing Page (`/`)

**Purpose:** Public marketing page — convert visitors into registered portal users.

**Layout:**
```
+------------------------------------------------------------------+
|  [Logo]  ClexAn Foods Portal          [Login]  [Get Started]    |  <- Navbar (sticky)
+------------------------------------------------------------------+
|                                                                  |
|   HERO SECTION                                                   |
|   ─────────────                                                  |
|   Headline:   "Your Own Store. Fully Managed."                   |
|   Subline:    "Launch a complete multi-branch ERP in minutes.    |
|                Your data. Your domain. Your cloud backups."      |
|                                                                  |
|   [Get Started Free]   [Learn More ↓]                           |
|                                                                  |
|   Hero graphic: animated isometric illustration of 4 containers |
|   spinning up (SVG animation)                                    |
|                                                                  |
+------------------------------------------------------------------+
|  FEATURE CARDS (3-column grid)                                   |
|  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐ |
|  │ ⚡ 2-Minute Setup │ │ 🌐 Custom Domains │ │ ☁ Cloud Backups  │ |
|  │ Provision a full │ │ Use your own     │ │ OneDrive, Google │ |
|  │ isolated silo in │ │ domain or our    │ │ Drive, or S3.    │ |
|  │ minutes.         │ │ subdomain.       │ │ Automated nightly│ |
|  └──────────────────┘ └──────────────────┘ └──────────────────┘ |
+------------------------------------------------------------------+
|  PLAN TIER COMPARISON TABLE                                      |
|  Starter / Professional / Enterprise                             |
|  [Get Started Free]                                              |
+------------------------------------------------------------------+
|  FOOTER                                                          |
|  © ClexAn Foods 2026  |  Privacy  |  Terms  |  Contact          |
+------------------------------------------------------------------+
```

**Interactions:**
- `[Get Started]` → `/register`
- `[Login]` → `/login`
- Sticky nav changes background opacity on scroll (CSS `backdrop-filter`)
- Hero headline uses a CSS text gradient animation

---

### 2.2 Register Page (`/register`)

**Purpose:** Create a portal account (not yet provisioning a silo — that is `/onboarding`).

```
+------------------------------------------------------------------+
|  [← Back to Home]                                                |
|                                                                  |
|  Create your portal account                                      |
|  ─────────────────────────────                                   |
|                                                                  |
|  Full Name          [                              ]             |
|  Email Address      [                              ]             |
|  Password           [                              ] [👁]         |
|  Confirm Password   [                              ] [👁]         |
|                                                                  |
|  Password strength meter (animated bar)                          |
|                                                                  |
|  [ ] I agree to the Terms of Service and Privacy Policy          |
|                                                                  |
|                          [Create Account →]                      |
|                                                                  |
|  Already have an account? [Sign In]                              |
+------------------------------------------------------------------+
```

**Validation (real-time, inline):**
- Email format check (regex) on blur
- Password: min 8 chars, 1 uppercase, 1 number, 1 symbol
- Password strength meter: Weak / Fair / Good / Strong with colour transition
- `[Create Account]` disabled until all fields valid and checkbox checked

**On success:** Redirect to `/onboarding` with session cookie set.

---

### 2.3 Login Page (`/login`)

```
+------------------------------------------------------------------+
|                                                                  |
|  Sign in to your portal                                          |
|  ────────────────────────                                        |
|                                                                  |
|  Email           [                              ]                |
|  Password        [                              ] [👁]            |
|                                                                  |
|  [Forgot password?]                                              |
|                                                                  |
|                          [Sign In →]                             |
|                                                                  |
|  Don't have an account? [Register]                               |
+------------------------------------------------------------------+
```

- `[Sign In]` shows button spinner on submit; disables to prevent double-submit
- Invalid credentials: red inline error beneath password field (no enumeration — same message for wrong email or wrong password)
- Successful login → redirect to `/dashboard` (or `returnUrl` if set)

---

### 2.4 Onboarding Wizard (`/onboarding`)

**Purpose:** Guide a newly registered user through provisioning their first store silo.

**Layout:** Full-page wizard with animated step indicator at top.

```
Step 1          Step 2          Step 3          Step 4
[Account] ──── [Your Store] ── [Domain] ──── [Confirm & Launch]
  ●               ○               ○                 ○
```

**Step 1 — Account (pre-filled, read-only):**
```
  Welcome, [Full Name]!
  Email: [user@example.com]  (from session)

  Admin credentials for your store:
  Admin Username  [admin                 ]
  Admin Password  [                      ] [👁]
  Confirm         [                      ] [👁]

  These are the credentials you will use to log into your store.

                                    [Next →]
```

**Step 2 — Your Store:**
```
  Store Name     [Acme Foods Ltd                ]
  Store Slug     [acme-foods  ] .store.domain     ← live validation (green/red indicator)
                                                    shows availability as-you-type (debounced 500ms)
  Currency       [XAF ▾]
  Plan           (●) Starter   (○) Professional   (○) Enterprise
                     Free            $29/mo              $99/mo

                     [← Back]              [Next →]
```

Slug real-time availability: POST `/api/control/slugs/check` (new lightweight endpoint)
Returns `{ available: true }` or `{ available: false, reason: "already taken" }`

**Step 3 — Domain:**
```
  How would you like to reach your store?

  (●) Use ClexAn platform subdomain   (zero setup)
      Store:    https://acme-foods.store.domain
      Branches: https://[branch].acme-foods.store.domain

  (○) Use my own domain
      Domain:   [                              ]
                → Instructions shown after slug is saved:
                  Add TXT record:
                  Name:  _clexan-verify.acme-foods.com
                  Value: clxv_4a8f3b...  [Copy]
                  TTL:   300

                  [Check DNS Now]   (live DNS lookup, shows spinner → ✓ or ✗)

  Note: You can always add a custom domain later from the Domains page.

                     [← Back]              [Next →]
```

**Step 4 — Confirm & Launch:**
```
  Review your setup

  ┌─────────────────────────────────────────────────────┐
  │  Store Name:      Acme Foods Ltd                    │
  │  Slug:            acme-foods                        │
  │  Admin:           admin@acme-foods.com              │
  │  Plan:            Professional                      │
  │  Currency:        XAF                               │
  │  Store URL:       https://acme-foods.store.domain   │
  │  API URL:         https://api.acme-foods.store...   │
  └─────────────────────────────────────────────────────┘

  By clicking Launch, your isolated store environment will be
  provisioned. This takes approximately 60–120 seconds.

                     [← Back]     [🚀 Launch My Store]
```

On click → submit button becomes spinner → shows animated provisioning progress:
```
  Provisioning your store...

  ✓  Request validated
  ✓  Secrets generated
  ✓  Compose blueprint created
  ⟳  Deploying containers...     (animated pulse)
  ○  Health check pending
  ○  Store ready
```

On success → redirect to `/dashboard` with welcome banner.

---

### 2.5 Dashboard (`/dashboard`)

```
+------------------------------------------------------------------+
|  [Logo] Acme Foods Portal      [Environment] [Domains] [Backups] [Settings] [Sign Out]
+------------------------------------------------------------------+
|                                                                  |
|  Welcome back, Admin!                            ● Healthy        |
|  Acme Foods Ltd · Professional Plan                              |
|                                                                  |
|  SILO HEALTH CARDS                                               |
|  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐         |
|  │  MySQL   │  │ MongoDB  │  │   API    │  │    UI    │         |
|  │  ● OK    │  │  ● OK    │  │  ● OK    │  │  ● OK    │         |
|  │  5m ago  │  │  5m ago  │  │  5m ago  │  │  5m ago  │         |
|  └──────────┘  └──────────┘  └──────────┘  └──────────┘         |
|                                                                  |
|  QUICK ACTIONS                                                   |
|  [Open Store ↗]  [Open API Docs ↗]  [Force Health Check]        |
|                                                                  |
|  DOMAIN SUMMARY                              BACKUP SUMMARY      |
|  ┌─────────────────────────────┐  ┌──────────────────────────┐  |
|  │ Platform:  acme.store...  ✓ │  │ Google Drive    ● Active  │  |
|  │ Custom:    acme-foods.com ✓ │  │ S3              ● Active  │  |
|  │ Branches:  3 configured     │  │ Last backup: 2h ago       │  |
|  │ [Manage Domains →]          │  │ [Manage Backups →]        │  |
|  └─────────────────────────────┘  └──────────────────────────┘  |
+------------------------------------------------------------------+
```

**Health card auto-refresh:** `setInterval` every 60 seconds calls `/api/portal/health/refresh`
(a thin portal-side endpoint that proxies to the Control Plane health check and returns JSON).

---

### 2.6 Environment Control Panel (`/environment`)

```
+------------------------------------------------------------------+
|  Environment Control Panel                                       |
|  Acme Foods Ltd  · acme-foods  · ● Healthy                      |
+------------------------------------------------------------------+
|                                                                  |
|  CONTAINER STATUS                          ACTIONS               |
|  ┌──────────────────────────────┐  ┌─────────────────────────┐  |
|  │  ● MySQL      acme-mysql  ✓  │  │  [↺ Restart All]        │  |
|  │    Last: 2026-09-01 10:01    │  │  [↺ Restart API]        │  |
|  │                              │  │  [↺ Restart UI]         │  |
|  │  ● MongoDB    acme-mongodb ✓ │  │  [⚡ Force Health Check] │  |
|  │    Last: 2026-09-01 10:01    │  └─────────────────────────┘  |
|  │                              │                               |
|  │  ● API        acme-api    ✓  │  SILO LINKS                  |
|  │    http://api.acme...        │  Store UI  [Open ↗]          |
|  │    Last: 2026-09-01 10:01    │  API       [Open ↗]          |
|  │                              │  API Docs  [Open ↗]          |
|  │  ● UI         acme-ui     ✓  │                               |
|  │    http://acme.store...      │  DANGER ZONE                  |
|  │    Last: 2026-09-01 10:01    │  [⏸ Suspend Silo]            |
|  └──────────────────────────────┘  [▶ Resume Silo]              |
|                                                                  |
|  PROVISIONING LOG TIMELINE                                       |
|  ─────────────────────────                                       |
|  ✓  2026-09-01 09:00  Validation — Request validated             |
|  ✓  2026-09-01 09:00  SecretGeneration — Secrets created         |
|  ✓  2026-09-01 09:00  ComposeGeneration — Blueprint written      |
|  ✓  2026-09-01 09:01  DockerDeployment — Containers started      |
|  ✓  2026-09-01 09:02  HealthCheck — All services healthy         |
+------------------------------------------------------------------+
```

**Restart actions:** POST to Control Plane API → button enters spinner state → result shown as toast.
**Suspend/Resume:** Confirmation dialog required (AppDialog pattern). Suspend shows warning that
the store will be unreachable until resumed.

---

### 2.7 Domain Manager (`/domains`)

```
+------------------------------------------------------------------+
|  Domain Management                                               |
+------------------------------------------------------------------+
|                                                                  |
|  PLATFORM SUBDOMAIN (always active)                              |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Store URL:   https://acme-foods.store.domain    ● Active  │  |
|  │  API URL:     https://api.acme-foods.store.domain ● Active  │  |
|  │  [Copy Store URL]  [Open Store ↗]                          │  |
|  └──────────────────────────────────────────────────────────┘   |
|                                                                  |
|  CUSTOM DOMAIN                                                   |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Status:   ● Active                                        │  |
|  │  Domain:   acme-foods.com                                  │  |
|  │  Verified: 2026-09-01 09:15 UTC                            │  |
|  │  [Remove Custom Domain]                                    │  |
|  └──────────────────────────────────────────────────────────┘   |
|                              ──── OR (if not configured) ────    |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Status:   ○ Not Configured                                │  |
|  │                                                            │  |
|  │  Add your domain:  [acme-foods.com        ]  [Save]        │  |
|  └──────────────────────────────────────────────────────────┘   |
|                              ──── OR (if pending) ────           |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Status:   ⚠ Pending Verification                          │  |
|  │  Domain:   acme-foods.com                                  │  |
|  │                                                            │  |
|  │  Add this DNS TXT record at your domain registrar:         │  |
|  │  ┌──────────────────────────────────────────────────┐     │  |
|  │  │  Type:   TXT                                     │     │  |
|  │  │  Name:   _clexan-verify.acme-foods.com           │     │  |
|  │  │  Value:  clxv_4a8f3bd9...c72e  [Copy]            │     │  |
|  │  │  TTL:    300                                     │     │  |
|  │  └──────────────────────────────────────────────────┘     │  |
|  │                                                            │  |
|  │  [⟳ Check DNS Now]    [Cancel]                             │  |
|  └──────────────────────────────────────────────────────────┘   |
+------------------------------------------------------------------+
```

**`[Check DNS Now]`:** AJAX POST → Portal proxies to Control Plane `/domains/verify`.
Shows spinner → then either:
- ✓ green banner "Domain verified! Your store is now reachable at acme-foods.com"
- ✗ red error card showing: `Expected TXT value: clxv_4a8f...  Found: (not found)`

---

### 2.8 Branch Manager (`/branches`)

```
+------------------------------------------------------------------+
|  Branch Subdomain Management                                     |
|  Map branch subdomains to your store locations.                  |
+------------------------------------------------------------------+
|                                                                  |
|  [+ Add Branch]                                                  |
|                                                                  |
|  Branch       │ URL                                  │ Status   │
|  ─────────────┼──────────────────────────────────────┼──────────│
|  HQ           │ hq.acme-foods.store.domain           │ ● Active │ [Edit][✕]
|  Northgate    │ northgate.acme-foods.store.domain    │ ● Active │ [Edit][✕]
|  Mfoundi      │ mfoundi.acme-foods.com               │ ⚠ Verify │ [Verify DNS][✕]
|                                                                  |
+------------------------------------------------------------------+

ADD BRANCH DRAWER (slides in from right):
┌────────────────────────────────────────┐
│  Add Branch Mapping                   ✕│
│  ────────────────────────────────────  │
│  Branch Name:   [HQ                  ] │
│  Branch Slug:   [hq                  ] │
│                                        │
│  Subdomain type:                       │
│  (●) Platform  hq.acme-foods.store.dom │
│  (○) Custom    [hq.acme-foods.com    ] │
│      (requires CNAME: acme-foods.store.│
│       domain → your DNS)              │
│                                        │
│           [Cancel]   [Add Branch]      │
└────────────────────────────────────────┘
```

---

### 2.9 Backup Configuration (`/backups`)

```
+------------------------------------------------------------------+
|  Backup Configuration                                            |
+------------------------------------------------------------------+
|                                                                  |
|  PROVIDER CARDS (horizontal scroll on mobile)                    |
|                                                                  |
|  ┌─────────────────────┐ ┌─────────────────────┐ ┌───────────┐  |
|  │  OneDrive            │ │  Google Drive        │ │  S3/MinIO │  |
|  │  ─────────────────   │ │  ─────────────────   │ │  ──────── │  |
|  │  Status: Disconnected│ │  Status: Connected   │ │  Status:  │  |
|  │                      │ │  Folder:             │ │  Not conf.│  |
|  │  [Connect OneDrive]  │ │  /ClexAn Backups/    │ │           │  |
|  │                      │ │  acme-foods/         │ │  [Config] │  |
|  │                      │ │  Last: 2h ago ✓      │ │           │  |
|  │                      │ │  [Disconnect]        │ │           │  |
|  │                      │ │  [Backup Now]        │ │           │  |
|  └─────────────────────┘ └─────────────────────┘ └───────────┘  |
|                                                                  |
|  SCHEDULE                                                        |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Run backups:  [Every night at ▾]  [02:00 UTC ▾]          │  |
|  │  Retention:    [7 days ▾]                                  │  |
|  │  Backup both MySQL and MongoDB                             │  |
|  │                                              [Save Schedule]│  |
|  └──────────────────────────────────────────────────────────┘   |
|                                                                  |
|  BACKUP HISTORY                                                  |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  Date                │ Database │ Provider     │ Size │ OK│   |
|  │  2026-09-01 02:00    │ MySQL    │ Google Drive │ 450M │ ✓ │   |
|  │  2026-09-01 02:30    │ MongoDB  │ Google Drive │ 120M │ ✓ │   |
|  │  2026-08-31 02:00    │ MySQL    │ S3           │ 448M │ ✓ │   |
|  └──────────────────────────────────────────────────────────┘   |
+------------------------------------------------------------------+
```

**`[Connect OneDrive]`:** Opens Microsoft OAuth consent in a popup window (800×600).
On completion, popup closes and parent page refreshes provider card state.

**`[Backup Now]`:** Triggers manual backup job. Button becomes spinner. Toast notification
on completion: "Backup completed — 450 MB uploaded to Google Drive".

---

### 2.10 Settings (`/settings`)

```
+------------------------------------------------------------------+
|  Account Settings                                                |
|  ─────────────────────────────────────────────────────────────  |
|  Full Name       [Admin User               ]  [Save]            |
|  Email           [admin@acme-foods.com     ]  (contact support) |
|                                                                  |
|  Change Password                                                 |
|  Current Password  [                       ]                     |
|  New Password      [                       ]                     |
|  Confirm           [                       ]                     |
|                                                          [Save]  |
|                                                                  |
|  PLAN & BILLING                                                  |
|  Current Plan:  Professional ($29/mo)                           |
|  Currency:      XAF (FCFA)                                      |
|  [Upgrade to Enterprise]  [Downgrade]                           |
|                                                                  |
|  DANGER ZONE                                                     |
|  ┌──────────────────────────────────────────────────────────┐   |
|  │  ⚠ Permanently delete this store                          │  |
|  │  All data, databases, uploads, and backups will be lost.  │  |
|  │  This action cannot be undone.                            │  |
|  │  Type store name to confirm: [              ]  [Delete]   │  |
|  └──────────────────────────────────────────────────────────┘   |
+------------------------------------------------------------------+
```

---

## 3. Shared Layout (`_Layout.cshtml`)

### 3.1 Authenticated Layout (used by Dashboard onwards)

```
+------------------------------------------------------------------+
│  [ClexAn Logo]                                         [Avatar]  │  <- Top nav
│  Acme Foods · ● Healthy                                          │
+------------------------------------------------------------------+
│  [Dashboard] [Environment] [Domains] [Branches] [Backups] [⚙]  │  <- Horizontal nav
+------------------------------------------------------------------+
│                                                                  │
│  PAGE CONTENT                                                    │
│                                                                  │
+------------------------------------------------------------------+
│  © ClexAn Foods 2026  ·  Status  ·  Support                     │  <- Footer
+------------------------------------------------------------------+
```

### 3.2 Guest Layout (Landing, Login, Register)

Minimal top nav: Logo on left, `[Login]` + `[Get Started]` on right.
Full-width content area. No sidebar.

---

## 4. Micro-interactions & Feedback Patterns

| Interaction | Pattern |
|:---|:---|
| Form submit | Button → spinner + disabled, re-enables on response |
| Slug availability check | Inline `✓ Available` / `✗ Taken` badge with 500ms debounce |
| Password strength meter | Animated bar: red→orange→yellow→green with label change |
| DNS check result | Slide-down card: green ✓ or red ✗ with found vs expected diff |
| Health card status | Pulsing dot: green (healthy), amber (checking), red (unhealthy) |
| Provision progress | Animated step-by-step checklist with live SSE or polling |
| Copy button | Clipboard copy → button text changes to "Copied!" for 2s then reverts |
| Danger zone delete | Input-to-confirm pattern; `[Delete]` disabled until name matches exactly |
| Toast notifications | Slide-in from top-right; success=green, error=red; auto-dismiss 5s |
| OAuth popup | `window.open` 800×600 centred; parent listens for `postMessage` on close |
