# Implementation Plan — Phase 1: Foundation, Auth, Onboarding & Dashboard

**Phase:** 1  
**Status:** Completed & Verified  
**Component:** `Store.ControlPlane` & `Store.TenantPortal`  
**Date:** September 2026  

---

## 1. Goal Description

Establish the architectural foundation of the **Store.TenantPortal** self-service tenant management suite, implement **PBKDF2-SHA512** account authentication, construct the interactive **4-step Onboarding Wizard (`/onboarding`)**, and build the **Main Dashboard (`/dashboard`)**.

---

## 2. Implementation Details

### 2.1 `Store.ControlPlane` Extensions
- **Entities**: Created `PortalAccount.cs` and `PortalAuthDtos.cs`.
- **Auth Service (`PortalAuthService.cs`)**:
  - Implemented password hashing using **PBKDF2-SHA512** with 250,000 iterations and 32-byte cryptographically secure salt.
  - Constant-time comparison (`CryptographicOperations.FixedTimeEquals`) to prevent timing attacks.
  - Slug availability checking with reserved word validation.
- **REST Endpoints (`PortalAuthController.cs`)**:
  - `POST /api/control/auth/register`
  - `POST /api/control/auth/login`
  - `GET /api/control/slugs/check?slug={slug}`
  - `POST /api/control/auth/link-tenant`

### 2.2 `Store.TenantPortal` Scaffolding & Design System
- **Project Structure**: Created `Store.TenantPortal.csproj` (.NET 8.0 Razor Pages).
- **Session & Client**:
  - Configured secure cookie authentication (`ClexAn_Portal_Session`, HttpOnly, SameSite=Strict, 8-hour sliding expiration).
  - Built typed `ControlPlaneClient` with Polly exponential backoff retries.
  - Configured strict Content Security Policy (CSP), `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`.
- **ClexAn Design System**:
  - `portal.css`: Tokens (`#050906`, `#019c01`), Outfit & Inter typography, glassmorphism (`backdrop-filter: blur(12px)`), `.btn-primary-glow`, `.btn-glass`.
  - `portal-onboarding.css`: Interactive step indicators, radio cards, animated progress transitions.

### 2.3 Razor Pages
- `/Index`: Hero landing page with features showcase, architecture highlights, and CTA buttons.
- `/Register` & `/Login`: Glassmorphic auth cards with validation feedback.
- `/Onboarding`: 4-step onboarding wizard (Account details, Store slug with live availability debounce, Admin credentials, Plan tier & instant container provisioning).
- `/Dashboard`: Overview of provisioned silo containers, health check trigger, network URLs, and live provisioning log timeline.

---

## 3. Verification & Validation

- `dotnet build Store.ControlPlane/Store.ControlPlane.csproj` &rarr; `0 Errors, 0 Warnings`
- `dotnet build Store.TenantPortal/Store.TenantPortal.csproj` &rarr; `0 Errors, 0 Warnings`
- **Git Commit**: `1a15387`
