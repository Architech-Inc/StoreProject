# Implementation Plan — Phase 4: Security Hardening, Rate Limiting, Audit Logging & End-to-End Polish

**Phase:** 4  
**Status:** Completed & Verified  
**Component:** `Store.ControlPlane` & `Store.TenantPortal`  
**Date:** September 2026  

---

## 1. Goal Description

Harden security, implement rate limiting policies, build the immutable **Tenant Audit Trail & Activity Engine**, enforce **IDOR isolation**, validate signed **HMAC-SHA256 OAuth states**, and perform final end-to-end integration polish.

---

## 2. Implementation Details

### 2.1 `Store.ControlPlane` Security & Audit Engine
- **ASP.NET Core Rate Limiting (`System.Threading.RateLimiting`)**:
  - `PortalAuth` Policy: 10 requests per 15 minutes per IP on auth endpoints.
  - `BackupTrigger` Policy: 5 requests per 10 minutes per IP on backup trigger endpoints.
- **Audit Logging Engine (`AuditService.cs` & `AuditController.cs`)**:
  - `TenantAuditRecord` model storing `AuditId`, `TenantId`, `Timestamp`, `ActorEmail`, `ActionType`, `Details`, `IpAddress`.
  - Automatically records audit records on all administrative mutations (Container restarts, Silo suspension/resumption, Domain additions/removals, Branch mappings, Cloud backup jobs).
  - REST endpoint: `GET /api/control/tenants/{id}/audit?limit=50`.

### 2.2 `Store.TenantPortal` Security & Hardening
- **IDOR Protection Filter (`TenantOwnerOnlyAttribute.cs`)**:
  - Razor Page filter verifying the authenticated user's `TenantId` claims match any route parameter before execution.
- **HMAC-SHA256 Signed OAuth State (`OAuthService.cs`)**:
  - Signs and validates OAuth state parameters (`{tenantId}:{timestamp}:{signature}`) with 10-minute expiry validation to prevent OAuth replay and CSRF attacks.
- **Dashboard Activity Timeline (`Dashboard.cshtml`)**:
  - Real-time audit trail table displayed on the main dashboard showing recent operational and security events.

---

## 3. Verification & Validation

- `dotnet build Store.ControlPlane/Store.ControlPlane.csproj` &rarr; `0 Errors, 0 Warnings`
- `dotnet build Store.TenantPortal/Store.TenantPortal.csproj` &rarr; `0 Errors, 0 Warnings`
- **Git Commit**: `4598386`
