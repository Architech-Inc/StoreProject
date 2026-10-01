# ADR-002: Per-Tenant Isolated Container Stacks vs Shared Database

## Status
**Accepted** (2025-09-01)

## Context
ClexAn Foods serves diverse retail operations across Africa, from single-outlet food markets to multi-branch supermarket chains. Retail operations require uncompromising financial confidentiality (sales revenue, supplier contracts, customer debt, margins), strict compliance with OHADA accounting standards, offline local POS syncing, and distinct maintenance schedules.

We evaluated three primary multi-tenancy architectural models:
1. **Shared Everything (Row-Level Security)**: Single application instance, single database, all tables partitioned by a `TenantId` column with global EF Core query filters.
2. **Shared Application, Separate Databases**: Single shared API and UI layer, but separate MySQL database per tenant routed dynamically by tenant connection string resolvers.
3. **Isolated Container Stacks (Virtual Private Stack)**: Each tenant runs their own isolated Docker container stack (API, UI, dedicated MySQL 8, dedicated MongoDB 6+, ClamAV sidecar) behind an edge proxy (Traefik 2.10) managed by an orchestration service (`Store.ControlPlane`).

## Decision
We chose **Isolated Container Stacks (Option 3)** as the primary multi-tenancy production model, orchestrated by `Store.ControlPlane` and fronted by `Traefik 2.10`:

1. **Stack Isolation**:
   - Each tenant receives their own network-isolated compose stack (`store-{slug}-api`, `store-{slug}-ui`, `store-{slug}-mysql`, `store-{slug}-mongodb`).
   - Tenant data never co-mingles at rest, in memory, or on disk.
   - Accidental omission of a `WHERE tenant_id = @id` clause in a query is physically incapable of leaking other tenants' financial records.

2. **Edge Routing**:
   - Traefik automatically routes requests based on Host header rules (e.g. `bastos-market.store.example.com` or custom domains).
   - TLS certificates and wildcard domains (`store.127.0.0.1.nip.io` in dev, `store.example.com` in prod) terminate at the proxy.

3. **Centralized Orchestration**:
   - `Store.ControlPlane` provides an authenticated, internal-only control interface for tenant lifecycle (provision, suspend, resume, backup, restore, deprovision).
   - `Store.TenantPortal` acts as the public SaaS self-service signup front-end communicating securely with `Store.ControlPlane`.

## Alternatives Considered
- **Shared Database with Row-Level Filtering (Option 1)**: Rejected as the primary model due to security blast radius concerns in retail finance. A single SQL injection or un-scoped LINQ query could expose competitor pricing or customer accounts. Furthermore, taking a backup or restoring a single tenant's database after accidental corruption requires complex table-level filtering rather than a standard `mysqldump`.
- **Kubernetes Namespaces per Tenant**: Evaluated for enterprise scale. Deferred because Docker Compose stacks provide significantly lower operational overhead and memory footprint on standard low-cost Linux VPS instances in African data centers.

## Consequences
### Positive
- **Guaranteed Isolation**: Zero risk of cross-tenant data leakage or connection pollution.
- **Granular Backup/Restore**: Each tenant can be snapshotted (`mysqldump` / `mongodump`) and restored independently without touching other tenants.
- **Independent Maintenance & Resource Allocation**: Resource-heavy tenants (e.g., peak Black Friday sales) do not degrade responsiveness for smaller tenants.
- **Custom Domain & Offline Flexibility**: Straightforward mapping of custom tenant domains and dedicated local caches.

### Trade-offs
- Higher memory footprint per tenant compared to row-level sharing (requires roughly 250MB–400MB base RAM per tenant container stack).
- Requires orchestration tooling (`Store.ControlPlane` and `provision-tenant` scripts) to manage container deployments and network lifecycle.
