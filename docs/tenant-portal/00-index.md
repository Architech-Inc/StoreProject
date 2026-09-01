# Store.TenantPortal — Documentation Index

> All specification documents for the Store.TenantPortal self-service portal.
> Review and approve all documents before implementation begins.

> [!IMPORTANT]
> **[00b-design-language.md](00b-design-language.md) is the canonical design reference.**
> All color tokens, component CSS, and animation rules come from that document.
> When in doubt about any visual detail in any other spec document, defer to `00b`.

| # | Document | Description |
|:--|:---|:---|
| 00 | [Index](00-index.md) | This file |
| 00b | [Design Language Reference](00b-design-language.md) | **Canonical** — color tokens, component CSS, animations, layout rules |
| 01 | [Technical Specification](01-technical-spec.md) | Project structure, data models, service layer, DI, configuration |
| 02 | [UX & Design Specification](02-ux-design-spec.md) | All page layouts, wireframes, component inventory, interaction patterns |
| 03 | [Domain & Routing Specification](03-domain-routing-spec.md) | DNS verification, Traefik dynamic config, branch subdomain architecture |
| 04 | [Backup & Cloud Storage Specification](04-backup-spec.md) | OAuth flows, provider integration, encryption, scheduling |
| 05 | [API Contract](05-api-contract.md) | Full REST API contract for all new Control Plane endpoints |
| 06 | [Security Specification](06-security-spec.md) | Threat model, auth design, data protection, audit |

---

## Implementation Plans (Phases 1 — 4)

| Phase | Implementation Plan | Status |
|:---|:---|:---:|
| **Phase 1** | [Phase 1: Foundation, Auth, Onboarding & Dashboard](plans/phase-1-foundation-and-onboarding.md) | Completed & Committed (`1a15387`) |
| **Phase 2** | [Phase 2: Environment Control, Custom Domains & Branch Routing](plans/phase-2-environment-domains-branches.md) | Completed & Committed (`8ae87a3`) |
| **Phase 3** | [Phase 3: Automated Cloud Backups & OAuth2 Integration](plans/phase-3-cloud-backups-oauth2.md) | Completed & Committed (`357edfa`) |
| **Phase 4** | [Phase 4: Security Hardening, Rate Limiting & Audit Logging](plans/phase-4-security-rate-limiting-audit.md) | Completed & Committed (`4598386`) |

---

## Document Status Tracker

| Document | Status | Reviewed |
|:---|:---:|:---:|
| 01-technical-spec.md | Approved | Yes |
| 02-ux-design-spec.md | Approved | Yes |
| 03-domain-routing-spec.md | Approved | Yes |
| 04-backup-spec.md | Approved | Yes |
| 05-api-contract.md | Approved | Yes |
| 06-security-spec.md | Approved | Yes |
