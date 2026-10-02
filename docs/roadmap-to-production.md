# Concrete Roadmap to Production → Multi-Tenant → Global — ClexAn Foods

> **Reconciled Roadmap & Audit Tracker Cross-Reference**  
> Last Updated: 2026-10-01 (Reconciled with Audit Tracker Waves 1–40)  
> This roadmap documents the architectural progression of ClexAn Foods (StoreProject), mapping strategic delivery phases directly to finding IDs in [`docs/audit-tracker.md`](audit-tracker.md).

---

## Strategic Phase Overview

```
Phase 1: Production Hardening ───────────────► [ 95% COMPLETE ] (Waves 1–40)
Phase 2: Multi-Tenant Enablement ────────────► [ 85% COMPLETE ] (Waves 13, 20, 36)
Phase 3: Regional Compliance & Africa-First ──► [ 70% COMPLETE ] (Waves 16, 21, 38)
Phase 4: Enterprise SaaS Maturity ───────────► [ 40% IN PROGRESS ]
```

---

## Phase 1 — Production Hardening & Operational Safety

### Goal
Transform the internal business engine into an observable, secure, zero-warning, production-grade platform.

### Status: **95% Complete** (Validated by Waves 1–40)

| Finding ID | Scope & Milestone | Status | Resolution Detail |
|---|---|---|---|
| `SEC-01` | MySQL root password empty fallback in compose | `[x]` | Stripped fallback; enforced `${VAR:?VAR is required}` in compose. |
| `SEC-02` | Placeholder JWT key startup crash guard | `[x]` | `Program.cs` throws fatal startup exception if key is placeholder or <32 chars. |
| `SEC-03` | Master Encryption Key placeholder guard | `[x]` | `SecretEncryptionService` enforces 32-character high-entropy master key. |
| `SEC-04` | MoMo payment callback HMAC signature | `[x]` | Migrated from static key to `X-Callback-Signature: sha256=<hex>` constant-time compare. |
| `SEC-05` | CSRF protection across Razor Pages | `[x]` | Enforced `AutoValidateAntiforgeryToken` globally. |
| `SEC-06` | Rate limiting on authentication & recovery | `[x]` | Configured ASP.NET Core rate limiters (`auth`, `password-recovery`, `financial`). |
| `SEC-08` | Structured audit logging | `[x]` | `AuditLoggingMiddleware` captures structured logs + database audit records via `[Audit]`. |
| `SEC-11` | ClamAV antivirus file boundary inspection | `[x]` | `IVirusScanner` streams bytes to ClamAV sidecar; `NoOp` refused in prod (`ADR-004`). |
| `SEC-12` | Path traversal & MIME type spoofing | `[x]` | Enforced magic-number inspection and UUID filenames in `FilesController`. |
| `SEC-14` | Refresh token cryptographic rotation | `[x]` | Enforced single-use refresh token rotation in `AuthenticationService`. |
| `SEC-20` | Controller authorization policy audit | `[x]` | Hardened policies across all 39 controllers; 13 security reflection tests. |
| `OPS-01` | Production compose secrets externalization | `[x]` | Eliminated empty fallbacks across all docker-compose files. |
| `OPS-02` | Healthchecks and readiness probes | `[x]` | Added `/health` and `/ready` probes to API and ControlPlane. |
| `OPS-03` | Traefik Edge Proxy & Dashboard security | `[x]` | Gated Traefik dashboard behind local dev flag; exposed via `docker-compose.traefik.yml`. |
| `PROC-08` | Developer onboarding documentation | `[x]` | Published comprehensive `docs/onboarding.md` guide. |
| `PROC-10` | Unified CLI task runners | `[x]` | Delivered root `Makefile` and native `tasks.ps1` PowerShell runner. |
| `PROC-11` | Automated CI/CD build matrix | `[x]` | Configured 6-leg parallel GitHub Actions workflow with build caching. |
| `OPS-05` | Automated Let's Encrypt TLS (DNS-01 / HTTP-01) | `[ ]` | Wire ACME automated wildcard resolver for production Traefik. |
| `OPS-06` | Automated container updates (Watchtower) | `[ ]` | Add Watchtower daemon with cleanup schedule. |
| `OPS-07` | Centralized log shipping (Loki / Seq) | `[ ]` | Provision Promtail/Loki sidecar for structured log aggregation. |

---

## Phase 2 — Multi-Tenant Enablement & Orchestration

### Goal
Provide isolated retail operations per tenant with zero cross-tenant data leakage, automated provisioning, and plan-tier enforcement.

### Status: **95% Complete**

| Finding ID | Scope & Milestone | Status | Resolution Detail |
|---|---|---|---|
| `MT-01` | Isolated container stack per tenant | `[x]` | Architected virtual private stack per tenant fronted by Traefik (`ADR-002`). |
| `MT-02` | Plan quota enforcement action filter | `[x]` | Built `[EnforceTenantQuota]` returning `HTTP 402 QuotaExceeded` (`ADR-005`). |
| `MT-03` | Centralized tenant lifecycle orchestration | `[x]` | Delivered `Store.ControlPlane` (provision, suspend, resume, deprovision). |
| `MT-05` | Public SaaS customer signup portal | `[x]` | Delivered `Store.TenantPortal` for self-service tenant registration. |
| `MT-06` | Tenant provisioning automation scripts | `[x]` | Authored `scripts/provision-tenant.ps1` and `scripts/provision-tenant.sh`. |
| `MT-07` | Multi-tenant database backup snapshotting | `[x]` | Delivered `scripts/backup-now.ps1` and `scripts/restore-database.ps1`. |
| `DOC-05` | Per-tenant operations runbook | `[x]` | Authored `docs/tenant_operations_runbook.md` with complete SOPs. |
| `MT-04` | Per-tenant SMTP credentials rotation | `[x]` | Delivered `ITenantSmtpService`, AES-256 encrypted relay config, portal UI, live test dispatch, and Enterprise plan feature gate. |
| `PROC-05` | Smoke test suite for ControlPlane / TenantPortal| `[x]` | Built `Store.ControlPlane.Tests` (51 tests) and `Store.TenantPortal.Tests` (27 tests) in Wave 43/47. |

---

## Phase 3 — Regional Compliance & Africa-First Retail

### Goal
Enable seamless multi-currency, multi-branch operations tailored for Cameroon, Central/West Africa, and OHADA regulatory accounting.

### Status: **70% Complete**

| Finding ID | Scope & Milestone | Status | Resolution Detail |
|---|---|---|---|
| `GAP-01` | Thermal receipt printer & A4 tax invoice export | `[x]` | Standardized ESC/POS thermal receipt formatting and OHADA tax invoice layouts. |
| `GAP-04` | Batch tracking, lot numbers & expiry alerts | `[x]` | FIFO/FEFO inventory batch tracking with automatic expiry degradation. |
| `GAP-07` | Barcode scanner studio & live lookups | `[x]` | Smart scanner hub with floating FAB and instant barcode assignment. |
| `GAP-21` | Mobile Money payment integration | `[x]` | MTN MoMo and Orange Money webhook validation with HMAC signatures. |
| `GAP-23` | Offline-capable POS with local ledger | `[x]` | Service Worker caching (`sw.js`) and background sync for network outages. |
| `UX-01` | Progressive Web App (PWA) manifest | `[x]` | Installed PWA manifest with mobile-friendly splash and offline cache. |
| `UX-04` | Global keyboard shortcuts cheatsheet (`?`) | `[x]` | Built accessible shortcut modal with live search filter and 3D keycaps. |
| `UX-02` | Mobile-first layout & touch-first POS register | `[x]` | Delivered mobile segmented view switcher, sticky bottom cart summary, and WCAG touch targets (Wave 42). |
| `SEC-24` | Password hashing upgrade to Argon2id | `[ ]` | Migrate from BCrypt (cost 12) to Argon2id. |

---

## Phase 4 — Enterprise SaaS Commercialization

### Goal
Scale to commercial SaaS operations with automated billing, self-service tier upgrades, and enterprise governance.

### Status: **40% In Progress**

| Milestone | Deliverable | Status | Dependencies |
|---|---|---|---|
| **Automated Billing Webhooks** | Stripe / PayDunya subscription webhooks in ControlPlane | Planned | `MT-02` (Done), `MT-05` (Done) |
| **Tenant White-Labeling** | Custom logo, portal CSS themes, and branded receipts | In Progress | `SOP-02` (Done) |
| **Real-Time Push Alerts** | SignalR live hub for low stock, cash variance, and order approvals | Planned | `Store.API` WebSockets |
| **API Versioning (`/api/v1/`)** | Asp.Versioning with ETag caching middleware | Planned | `GAP-15`, `GAP-16` |
| **Architecture Records (ADR)** | Formal ADR documentation of system design decisions | `[x]` | `PROC-07` (`docs/adr/`) |

---

## Operational Maturity Index

| Metric | Target | Current Status | Notes |
|---|---|---|---|
| **Build Warnings** | 0 warnings | **0 warnings** | Release mode strictly enforced across all 9 projects. |
| **Unit Test Suite** | >400 tests | **499 passed (100%)** | 421 API tests + 51 ControlPlane tests + 27 TenantPortal tests. |
| **Build Time** | <90s | **~25s** | Release build with incremental Roslyn analyzer caching. |
| **Tenant Isolation** | Virtual Private Stack | **100% Isolated** | Dedicated containers and database per tenant (`ADR-002`). |
| **Documentation Coverage**| Comprehensive | **High** | Onboarding, Runbook, Security, ADRs, Design System. |
