# Implementation Plan — Phase 2: Environment Control, Custom Domain Verification & Branch Routing

**Phase:** 2  
**Status:** Completed & Verified  
**Component:** `Store.ControlPlane` & `Store.TenantPortal`  
**Date:** September 2026  

---

## 1. Goal Description

Implement the **Environment Control Panel (`/environment`)**, **Custom Domain Verification (`/domains`)**, and **Branch Subdomain Mapping (`/branches`)** in `Store.TenantPortal` and the supporting backend orchestration in `Store.ControlPlane`.

This enables tenants to:
1. Externally monitor and control their isolated 4-container stack (MySQL, MongoDB, Store.API, Store.UI) with live status indicators, per-container restarts, global restarts, and silo suspension/resumption.
2. Connect their own custom domain (e.g. `acmefoods.com`) using automated DNS TXT verification (`_clexan-verify.{domain}` records via `DnsClient`) and automatic dynamic Traefik reverse-proxy rule updates (`traefik/dynamic/{slug}.yml`).
3. Map physical branches to dedicated subdomains (`[branch].[slug].store.clexan.com` or `[branch].[customdomain]`) with real-time URL previews.

---

## 2. Architectural Design & Principles

- **Dennis Ritchie Systems Design**: Minimal ambient state, atomic configuration generation (`.yml` written to temp file before move), explicit interfaces, and zero hidden assumptions.
- **Uncle Bob Clean Architecture**: `Store.TenantPortal` communicates purely over HTTP with `Store.ControlPlane` via typed `IControlPlaneClient`. DTO contracts are self-contained.
- **Security & Integrity**: Cryptographic random token generation (`clxv_{24_bytes_hex}`), zero-cache DNS lookups against public authoritative resolvers (Cloudflare `1.1.1.1` and Google `8.8.8.8`), and input validation regex on domain/branch slugs.
- **ClexAn Design System**: Dark `#050906` aesthetic, `#019c01` primary glow, `Outfit` headings, `Inter` body text, glassmorphic cards (`backdrop-filter: blur(12px)`), and `.btn-primary-glow` CTAs.

---

## 3. Implementation Details

### 3.1 `Store.ControlPlane` Infrastructure & Domain Engine

1. **Models (`Store.ControlPlane/Models/Tenant.cs`)**:
   - `TenantDomainConfig`: `CustomDomain`, `DomainStatus Status`, `VerificationToken`, `VerificationRecordName`, `VerifiedAt`, `LastCheckedAt`, `LastErrorMessage`.
   - `TenantBranchMapping`: `BranchId`, `BranchName`, `BranchSlug`, `BranchDomainType DomainType`, `CustomSubdomain`, `ResolvedUrl`, `VerificationStatus`, `VerificationRecordName`, `VerificationRecordValue`, `DateCreated`.
2. **DTOs (`Store.ControlPlane/Models/DTOs/DomainAndBranchDtos.cs`)**:
   - `TenantDomainDto`, `SetCustomDomainRequest`, `VerifyDomainResponse`.
   - `BranchDto`, `CreateBranchRequest`.
   - `EnvironmentStatusDto`, `ContainerStatusDto`.
3. **DNS Verification Service (`IDomainVerificationService.cs` & `DomainVerificationService.cs`)**:
   - Generates tokens: `clxv_{32_hex_chars}`.
   - Formats verification host: `_clexan-verify.{cleanDomain}`.
   - Queries TXT records live with `DnsClient` `LookupClient` (timeout: 5s, cache disabled).
4. **Dynamic Traefik Configuration Writer (`ITraefikConfigWriter.cs` & `TraefikConfigWriter.cs`)**:
   - Generates `traefik/dynamic/{slug}.yml` with router rules for UI (`{slug}-ui`) and API (`{slug}-api`).
   - Automatically hot-reloads router rules when custom domains and branch subdomains are verified.
5. **Tenant Orchestrator Extensions (`ITenantOrchestrator.cs` & `TenantOrchestrator.cs`)**:
   - `RestartContainerAsync`, `RestartAllContainersAsync`, `GetEnvironmentStatusAsync`.
   - `SetCustomDomainAsync`, `VerifyCustomDomainAsync`, `RemoveCustomDomainAsync`.
   - `GetBranchesAsync`, `AddBranchAsync`, `VerifyBranchAsync`, `RemoveBranchAsync`.
6. **Controllers**:
   - `EnvironmentController.cs`: `GET /api/control/tenants/{id}/environment`, `POST .../restart/{service}`, `POST .../suspend`, `POST .../resume`.
   - `DomainsController.cs`: `GET /api/control/tenants/{id}/domains`, `POST .../custom`, `POST .../verify`, `DELETE .../custom`.
   - `BranchesController.cs`: `GET /api/control/tenants/{id}/branches`, `POST ...`, `POST .../{branchId}/verify`, `DELETE .../{branchId}`.

---

### 3.2 `Store.TenantPortal` Service Layer & UI

1. **Typed Client (`IControlPlaneClient.cs` & `ControlPlaneClient.cs`)**:
   - Implements all Environment, Domain, and Branch operations under Polly retry resilience policies.
2. **Environment Control Panel (`/Environment`)**:
   - `Environment.cshtml` & `Environment.cshtml.cs`:
   - 4 Container cards: MySQL 8.0, MongoDB 7.0, Store.API, Store.UI.
   - Per-service restart buttons + Global `[Restart All Containers]` action with instant feedback.
   - Danger Zone: Silo suspension / resumption.
3. **Custom Domains Manager (`/Domains`)**:
   - `Domains.cshtml` & `Domains.cshtml.cs`:
   - Platform subdomain card with copyable URLs.
   - BYOD custom domain card:
     - Unconfigured: Domain input form.
     - Pending: Diagnostic DNS instructions box (Type `TXT`, Host `_clexan-verify.{domain}`, Value `clxv_...`, TTL `300`) with one-click copy buttons and live `[⟳ Check DNS Now]` trigger.
     - Verified: Active status badge, connected HTTPS URL, and `[Disconnect Domain]` button.
4. **Branch Subdomain Routing (`/Branches`)**:
   - `Branches.cshtml` & `Branches.cshtml.cs`:
   - High-density table of configured store branches (HQ, Northgate, Downtown, etc.).
   - Slide-over glass drawer to map platform (`[branch].[slug].store.clexan.com`) or custom (`[branch].[customdomain]`) subdomains with live preview.
   - Action buttons to verify DNS and delete branch mappings.

---

## 4. Verification & Validation

- `dotnet build Store.ControlPlane/Store.ControlPlane.csproj` &rarr; `0 Errors, 0 Warnings`
- `dotnet build Store.TenantPortal/Store.TenantPortal.csproj` &rarr; `0 Errors, 0 Warnings`
- **Git Commit**: `8ae87a3`
