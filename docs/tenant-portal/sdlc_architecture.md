# StoreProject: Multi-Tenant SDLC & Sandboxing Architecture

This document defines the architectural vision and workflow for giving tenants self-service capabilities over their Software Development Life Cycle (SDLC), including version upgrades, safe rollbacks, and parallel-running sandboxes.

## 1. Unified Version Management (The "Release Train")
To abstract away complex technical details (like container tags and schema versions), the platform will utilize **System Releases**. 

- **Concept:** A `SystemRelease` entity represents a bundled version of the entire StoreProject platform (e.g., `StoreOS v2.1`). It strictly binds the `store-ui:v2.1` image, the `store-api:v2.1` image, and the implicit EF Core migrations required.
- **Why?** It ensures that when a tenant updates, their frontend, backend, and database move exactly in lockstep, eliminating version mismatch bugs.

## 2. Tenant Self-Service SDLC (The "Update Center")
Tenants will have access to an **Update Center** inside their Tenant Portal.

### Safe Upgrades
When a new `SystemRelease` is published globally, tenants see a notification. They can initiate the update instantly or schedule it. The Control Plane re-writes their `docker-compose.yml` to the new tags and restarts their containers. EF Core runs database schema updates automatically on boot.

### Safe Rollbacks (Zero Data Loss)
If an update disrupts their workflow, they can rollback. Because EF Core schema changes can be destructive when downgrading code, the platform protects them via **Pre-flight Snapshots**:
1. Exactly before the upgrade, the Control Plane performs a rapid `mysqldump` of the tenant's database and saves it as a `TenantSnapshot`.
2. If the user clicks "Rollback", the Control Plane halts their containers, drops the broken database, restores the `TenantSnapshot`, and reverts the container tags back to the previous `SystemRelease`.

## 3. Parallel Running Sandboxes (The "Preview Environment")
For massive updates (e.g., major UI redesigns or new modules), tenants need a risk-free way to test the waters with their actual live data.

### The Sandbox Workflow
1. The tenant clicks **"Preview in Sandbox"** on a new release.
2. The Control Plane takes an instant `mysqldump` snapshot of their live database (e.g., `store_acmefoods`).
3. The system provisions a temporary tenant stack (e.g., `sandbox.acmefoods.store.localhost`).
4. Instead of mounting the clean production template, it provisions the stack using the live snapshot, but running the **New Version** containers.
5. **Outcome:** The tenant can log in, see the new features, train staff, and run test sales—all with their exact products, customers, and inventory, perfectly isolated from their live instance.
6. **Promotion:** Once satisfied, the tenant approves the upgrade on their live instance, and the sandbox is automatically torn down.
