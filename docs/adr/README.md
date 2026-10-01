# Architecture Decision Records (ADRs) — ClexAn Foods

> Architecture Decision Records (ADRs) capture important architectural and structural decisions made in the ClexAn Foods (StoreProject) codebase, along with their context, rationale, alternatives considered, and consequences.

---

## Index of Architecture Decision Records

| ADR # | Title | Status | Date | Area |
|---|---|---|---|---|
| [**ADR-001**](ADR-001-clean-architecture-application-dispatchers.md) | Clean Architecture & Request Dispatcher Pattern | **Accepted** | 2025-08-15 | Core Architecture |
| [**ADR-002**](ADR-002-per-tenant-isolated-container-stacks.md) | Per-Tenant Isolated Container Stacks vs Shared Database | **Accepted** | 2025-09-01 | Multi-Tenancy & Infrastructure |
| [**ADR-003**](ADR-003-layered-security-permission-claims-and-audit.md) | Layered Security: JWT Bearer, Permission Policies & Structured Audit Logging | **Accepted** | 2025-10-10 | Security & Governance |
| [**ADR-004**](ADR-004-antivirus-file-boundary-inspection.md) | Anti-Virus File Inspection with ClamAV Sidecar & Dev Fallback | **Accepted** | 2026-01-20 | Security & File Pipeline |
| [**ADR-005**](ADR-005-plan-quota-and-tenant-lifecycle-enforcement.md) | Plan Quota Enforcement Attributes & Tenant Lifecycle State Machine | **Accepted** | 2026-03-15 | SaaS & Monetization |

---

## What is an ADR?

An Architecture Decision Record (ADR) is a short text document describing an architectural decision made on a project. Each record follows the standard Michael Nygard structure:

1. **Title**: Number and descriptive title.
2. **Status**: Proposed, Accepted, Deprecated, or Superseded.
3. **Context**: What is the problem or circumstance requiring a decision?
4. **Decision**: What did we choose to do and how is it implemented?
5. **Alternatives Considered**: Other approaches evaluated and why they were not chosen.
6. **Consequences**: Positive effects, trade-offs, and resulting operational impacts.

When proposing a new ADR, copy the structure above, assign the next sequential number, and submit via a pull request.
