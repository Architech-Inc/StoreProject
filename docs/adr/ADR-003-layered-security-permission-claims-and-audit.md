# ADR-003: Layered Security: JWT Bearer, Permission Policies & Structured Audit Logging

## Status
**Accepted** (2025-10-10, updated 2026-09-30)

## Context
Retail point-of-sale and ERP platforms manage sensitive commercial assets: pricing strategies, financial ledger balances, cash drawers, employee payroll, and supplier terms. Traditional coarse-grained role-based authorization (e.g., `[Authorize(Roles = "Admin,Manager")]`) is too rigid for retail environments where cashiers may require temporary inventory count permissions, branch managers may have restricted access to central payroll, and auditor accounts require read-only access across all modules.

Furthermore, compliance standards (OHADA, GDPR/local data protection) require comprehensive audit trails of financial alterations, cash reconciliations, and user credential modifications.

## Decision
We implemented a **defense-in-depth, layered runtime security model**:

1. **JWT Bearer Authentication & Security Stamp Validation**:
   - Access tokens contain standard subject claims (`uid`), username, and individual permission claims (`perm`).
   - Every authenticated request verifies the user's `SecurityStamp` in the database. When credentials change, sessions are revoked, or an account is suspended, all existing access tokens are instantly invalidated without waiting for token expiry.
   - Refresh tokens are cryptographically rotated on every refresh request (`SEC-14`).

2. **Automated Permission Policy Registration**:
   - Every constant defined in `PermissionKeys.All` (in `Store.Models`) is automatically registered in `Store.API/Program.cs` as an authorization policy:
     ```csharp
     builder.Services.AddPolicy(key, p => p.RequireClaim("perm", key));
     ```
   - Endpoints declare fine-grained policy requirements: `[Authorize(Policy = PermissionKeys.InventoryWrite)]`.
   - Adding a new permission key to `PermissionKeys.All` automatically creates its corresponding policy without manual `Program.cs` edits.

3. **Strict Anonymous Allowlisting**:
   - Only explicitly vetted public endpoints (e.g. login, refresh, QR receipt lookup) declare `[AllowAnonymous]`.
   - Automated reflection unit tests (`EndpointAuthorizationSecurityTests.cs`) fail the build if any new unapproved action is decorated with `[AllowAnonymous]` or lacks authorization.

4. **Dual Audit Logging (`[Audit]`)**:
   - Non-default and security-critical mutating endpoints declare `[Audit("Action Description", Category = "Category")]`.
   - `AuditLoggingMiddleware` captures structured JSON logs via `ILogger` and persists security-relevant audit records to `IAuditLogService` asynchronously.

5. **Rate Limiting Policies**:
   - Endpoints are protected by fixed-window rate limiters: `auth` (strict login/recovery throttling), `financial` (heavy invoice/cash operations), and `general`.

6. **Cryptographic Callbacks**:
   - Payment webhooks (e.g. MTN MoMo, Orange Money) validate `X-Callback-Signature: sha256=<hex>` over the raw request payload using constant-time HMAC comparison.

## Alternatives Considered
- **Coarse Role Strings (`[Authorize(Roles = "...")]`)**: Rejected due to inflexibility when tailoring user capabilities across differing branch hierarchies.
- **Dynamic Database-Driven Policy Resolution on Every Request**: Rejected due to latency and database overhead; embedding granted permission claims in the JWT token with security-stamp checks provides low latency with instantaneous revocation capabilities.

## Consequences
### Positive
- **Fine-Grained Access Control**: Roles map to sets of permissions; users can be granted specific operational rights without elevation to full administrative roles.
- **Zero-Bypass Guardrails**: Continuous reflection tests prevent accidental public endpoint exposure during rapid development.
- **Comprehensive Audit Trail**: Complete forensic history of who performed what action, when, and from what IP address.

### Trade-offs
- Slightly larger JWT token payload size due to multiple `perm` claims.
- Developers must explicitly declare both `[Authorize(Policy = "...")]` and `[Audit]` on new endpoints.
