# ADR-005: Plan Quota Enforcement & Tenant Lifecycle State Machine

## Status
**Accepted** (2026-03-15)

## Context
As ClexAn transitioned into a multi-tenant commercial SaaS product, tenants were subscribed to distinct plan tiers (Starter, Professional, Enterprise) with resource constraints:
- Maximum active branch locations
- Maximum active cashier and manager user accounts
- Monthly invoice volume thresholds

We needed a standardized, non-intrusive mechanism to enforce plan limits on mutating endpoints without scattering repetitive `if (tenant.Branches.Count >= limit)` checks across controllers and services. Additionally, the system needed an explicit lifecycle state machine to govern tenant onboarding, temporary suspension (e.g. billing failure), resumption, and safe deprovisioning.

## Decision
We implemented a **declarative Action Filter for quota enforcement** combined with a **formal Tenant Lifecycle State Machine**:

1. **Declarative Quota Gate (`[EnforceTenantQuota]`)**:
   - Controller mutation actions declare quota gates:
     ```csharp
     [HttpPost]
     [EnforceTenantQuota(TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne)]
     public async Task<IActionResult> AddBranch(...)
     ```
   - If the tenant exceeds their tier limit, the filter short-circuits execution and returns **HTTP 402 Payment Required**:
     ```json
     {
       "code": "QuotaExceeded",
       "message": "Tenant has reached the maximum allowed Branches for their plan tier.",
       "traceId": "..."
     }
     ```
   - Decoupled via `IQuotaEnforcementHandler` in `Store.Models.Billing`, allowing domain code to remain free of direct ControlPlane project dependencies.

2. **Tenant Lifecycle State Machine**:
   - Each tenant entity maintains an explicit status:
     - **`Provisioning`**: Stack deployment initiated; databases and containers being created.
     - **`Active`**: Normal operation; all API/UI traffic routed.
     - **`Suspended`**: Operations paused due to billing arrears or security freeze. Ingress routes redirect to a payment/suspension notice.
     - **`Deprovisioned`**: Tenant stack torn down; data archived according to retention policy.
   - Transitions are enforced strictly through `Store.ControlPlane/Controllers/TenantsController.cs`.

3. **Master Key Cryptographic Verification**:
   - ControlPlane lifecycle endpoints are internal-only and require authentication via `X-Master-Key` validated against `ControlPlane:MasterEncryptionKey` using constant-time comparison.

## Alternatives Considered
- **Inline Service-Layer Verification**: Placing count checks inside each domain service method (`BranchService.CreateAsync`). Rejected because service methods shouldn't be responsible for HTTP 402 semantics, and testing requires deep entity mock setups.
- **Client-Only (UI) Enforcement**: Disabling the "+ New Branch" button on the UI. Rejected because API endpoints could still be invoked directly, bypassing plan tiers.

## Consequences
### Positive
- **Declarative & Centralized**: One attribute handles quota resolution, post-mutation projection, and standardized HTTP 402 error contracts.
- **Fail-Closed Security**: Unhandled quota overages immediately block mutations rather than silently degrading platform resources.
- **Clear Operational SOPs**: Support staff and automated billing webhooks interact with well-defined lifecycle endpoints (`/suspend`, `/resume`, `/deprovision`).

### Trade-offs
- Adding a new quota type requires adding an enum value to `TenantQuota` and updating the `PlanQuotaGate` definition matrix.
