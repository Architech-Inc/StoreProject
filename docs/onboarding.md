# Developer Onboarding & Quickstart Guide — ClexAn Foods (StoreProject)

> **Welcome to the ClexAn Foods platform team!**  
> This guide is designed to take a new engineer from zero to a fully operational local development environment in under 30 minutes. It documents system topology, prerequisite tooling, environment configuration, database seeding, daily development workflows, testing standards, and troubleshooting common pitfalls.

---

## 1. System Overview & Architecture

**ClexAn Foods** is a multi-tenant retail ERP, procurement, inventory, point-of-sale (POS), and customer loyalty platform. It is engineered to operate either as an isolated containerized stack per tenant or in a hybrid cloud topology behind edge routing.

### 1.1 Four-Tier Architecture Topology

```
                              ┌─────────────────────────┐
                              │     Edge Proxy          │
                              │     (Traefik 2.10)      │
                              └────────────┬────────────┘
                                           │
                ┌──────────────────────────┴──────────────────────────┐
                │                                                     │
                ▼                                                     ▼
┌───────────────────────────────┐                     ┌───────────────────────────────┐
│     Control Plane Services    │                     │     Isolated Tenant Stacks    │
├───────────────────────────────┤                     ├───────────────────────────────┤
│ • Store.ControlPlane (5050)   │                     │ • Store.API (7112 / 5000)     │
│ • Store.TenantPortal (5060)   │                     │ • Store.UI (5001 / 80)        │
│ • ControlPlane MySQL (3306)   │                     │ • Tenant MySQL (3306)         │
│ • Master Cryptographic Vault  │                     │ • Tenant MongoDB 6+ (27017)   │
│ • Docker Orchestration Engine │                     │ • ClamAV Anti-Virus Sidecar   │
└───────────────────────────────┘                     └───────────────────────────────┘
```

### 1.2 Solution Projects Directory Map

| Project | Target Framework | Purpose & Responsibilities |
|---|---|---|
| [`Store.Models`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models) | `net8.0` | Pure domain models, entities, DTOs, request/response contracts, and permission keys (`PermissionKeys.All`). No database dependencies. |
| [`Store.DbServices`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices) | `net8.0` | EF Core `StoreDbContext`, Pomelo MySQL configurations, database migrations, generic repositories, and business service implementations. |
| [`Store.API`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API) | `net8.0` | REST API layer containing controllers, request dispatchers, authorization policies, rate-limiting handlers, virus scanner integration, and audit middleware. |
| [`Store.UI`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.UI) | `net8.0` | Razor Pages front-end for cashiers, managers, and store administrators. Consumes the ClexAn design system, POS offline engine, and real-time command palette. |
| [`Store.ControlPlane`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane) | `net8.0` | Internal orchestration engine that manages tenant lifecycles: provisioning, suspension, quota enforcement, backups, and restores. |
| [`Store.TenantPortal`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal) | `net8.0` | Public SaaS customer signup, pricing tiers, and onboarding surface. Communicates internally with `Store.ControlPlane`. |
| [`Store.API.Tests`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests) | `net8.0` | Comprehensive xUnit test suite (414+ tests) validating security authorization, controllers, business rules, and quota enforcement. |

---

## 2. Prerequisites & Tooling

Before setting up the repository, ensure your development machine has the following tools installed:

1. **.NET 8 SDK**: Version `8.0.x` or later. Verify with `dotnet --version`.
2. **Docker & Docker Compose**: Docker Desktop (Windows/macOS) or Docker Engine + Docker Compose v2 (Linux). Verify with `docker compose version`.
3. **Database Server**:
   - MySQL 8.0+ (run locally or via Docker).
   - MongoDB 6.0+ (optional locally; required if testing document-based attachments).
4. **Command Shell**:
   - **Windows**: PowerShell 7+ (`pwsh`) or Windows PowerShell 5.1.
   - **Linux / macOS**: Bash or Zsh, with GNU `make`.
5. **Git**: Version 2.30+ with LF line-ending support.
6. **IDE / Editor (Recommended)**:
   - Visual Studio 2022 (v17.8+), Visual Studio Code with C# Dev Kit, or JetBrains Rider.

---

## 3. First-Time Setup & Configuration

Follow these steps sequentially to configure your local environment:

### Step 1: Clone Repository
```bash
git clone https://github.com/Architech-Inc/StoreProject.git
cd StoreProject
```

### Step 2: Configure Environment Variables
Copy the template configuration file:
```bash
# On Linux / macOS:
cp .env.example .env

# On Windows PowerShell:
Copy-Item .env.example .env
```

Open `.env` and verify the local development values:
- `ROOT_DOMAIN=store.127.0.0.1.nip.io` (enables wildcard loopback DNS out of the box).
- `TRAEFIK_API_ENABLED=true` (exposes Traefik dashboard at `http://localhost:8080` for local inspection).
- Ensure passwords and keys meet minimum security requirements (minimum 32 characters for `STORE_JWT_SECRET`, `STORE_OTP_PEPPER`, and `CONTROLPLANE_MASTER_KEY`).

### Step 3: Trust Development HTTPS Certificates
```bash
dotnet dev-certs https --trust
```

### Step 4: Restore Dependencies & Build Solution
```bash
# Using Make (Linux / macOS):
make restore
make build

# Using PowerShell (Windows):
.\tasks.ps1 restore
.\tasks.ps1 build
```

---

## 4. Database Setup & Seeding

The platform uses MySQL 8 as its relational store. You can run MySQL directly or spin up the development container.

### Option A: Spin Up MySQL via Docker
```bash
docker run -d \
  --name store-mysql-dev \
  -p 3306:3306 \
  -e MYSQL_ROOT_PASSWORD=rootpassword \
  -e MYSQL_DATABASE=store_db_v2 \
  -e MYSQL_USER=store_user \
  -e MYSQL_PASSWORD=storepassword \
  mysql:8.0 --default-authentication-plugin=mysql_native_password
```

### Option B: Apply EF Core Migrations
Once MySQL is running and reachable, update the database schema:
```bash
# Using CLI:
dotnet ef database update --project Store.DbServices --startup-project Store.API

# Using Task Runner:
make migrate
# or (Windows):
.\tasks.ps1 migrate
```

### Option C: Seed Baseline Demo Data
To load standard baseline products, categories, units, and initial admin accounts:
```bash
# Import schema and sample catalog into MySQL:
# (Replace credentials as appropriate for your setup)
mysql -u store_user -p store_db_v2 < Database/templates/001_production_base.sql
```

---

## 5. Running the Application Locally

You can run individual services on your host or launch the multi-service container platform.

### 5.1 Host Development (Recommended for Coding & Debugging)

Open separate terminal tabs or use your IDE to run the services:

```bash
# Terminal 1 — Backend REST API
# Using Make:
make run-api
# Using PowerShell:
.\tasks.ps1 run api
# Or native dotnet:
dotnet run --project Store.API

# Terminal 2 — Web UI (Admin & POS Console)
# Using Make:
make run-ui
# Using PowerShell:
.\tasks.ps1 run ui
# Or native dotnet:
dotnet run --project Store.UI
```

**Local Ports & Endpoints:**
- **Store.API**: `https://localhost:7112` (Swagger UI at `/swagger`)
- **Store.UI**: `https://localhost:5001` or `http://localhost:5000`
- **Store.ControlPlane**: `http://localhost:5050`
- **Store.TenantPortal**: `http://localhost:5060`

### 5.2 Containerized Platform Run

To run the complete platform (Edge proxy + Control Plane + Databases) inside Docker:
```bash
# Start platform services:
make platform-up
# or on Windows:
.\tasks.ps1 platform up

# Follow live container logs:
make platform-logs
# or on Windows:
.\tasks.ps1 platform logs

# Stop platform services:
make platform-down
# or on Windows:
.\tasks.ps1 platform down
```

### 5.3 Provisioning a Tenant Store

To provision a fresh, isolated tenant stack through the Control Plane:
```bash
# Using Make:
make provision-tenant SLUG=bastos-market STORE="Bastos Fresh Market" EMAIL=admin@bastos.cm

# Using PowerShell:
.\tasks.ps1 provision -Slug "bastos-market" -StoreName "Bastos Fresh Market" -AdminEmail "admin@bastos.cm"
```

---

## 6. Daily Task Runners (`Makefile` & `tasks.ps1`)

We provide unified task runners for both POSIX (`make`) and Windows PowerShell (`.\tasks.ps1`) environments.

| Task Action | Makefile Command | PowerShell Command | Description |
|---|---|---|---|
| **Help** | `make help` | `.\tasks.ps1 help` | Display formatted help and all available targets |
| **Build (Release)** | `make build` | `.\tasks.ps1 build` | Build entire solution in Release mode |
| **Build (Debug)** | `make build-dev` | `.\tasks.ps1 build -Configuration Debug` | Build entire solution in Debug mode |
| **Test** | `make test` | `.\tasks.ps1 test` | Run xUnit test suite (414+ tests) |
| **Clean** | `make clean` | `.\tasks.ps1 clean` | Delete bin/obj outputs and temporary log files |
| **Run API** | `make run-api` | `.\tasks.ps1 run api` | Launch `Store.API` with hot reload |
| **Run UI** | `make run-ui` | `.\tasks.ps1 run ui` | Launch `Store.UI` Razor Pages app |
| **Run ControlPlane** | `make run-cp` | `.\tasks.ps1 run cp` | Launch `Store.ControlPlane` service |
| **Run TenantPortal** | `make run-tp` | `.\tasks.ps1 run tp` | Launch `Store.TenantPortal` signup portal |
| **Platform Up** | `make platform-up` | `.\tasks.ps1 platform up` | Start Traefik and Control Plane via Docker Compose |
| **Platform Down**| `make platform-down` | `.\tasks.ps1 platform down` | Stop Docker Compose platform |
| **Migrate DB** | `make migrate` | `.\tasks.ps1 migrate` | Apply EF Core migrations to target database |
| **Audit & Verify**| `make audit` | `.\tasks.ps1 audit` | Clean build, test suite execution, and secrets scan |

---

## 7. Security, Conventions & Code Standards

All contributors are expected to uphold the following standards:

1. **Zero Warnings, Zero Errors Policy**:
   - Every build must pass clean with **0 warnings and 0 errors** in Release mode (`dotnet build StoreProject.sln -c Release`).
2. **Clean Architecture Boundaries**:
   - `Controllers` delegate all operations to `Application/Handlers` or domain services. Business rules belong in services or aggregate handlers, never inside controller action methods.
   - Entities are mapped at the boundary into strongly-typed DTOs; never expose EF Core entities directly.
   - Read queries in `IUnitOfWork` must include `.AsNoTracking()`.
3. **Authorization & Policies**:
   - Every mutating endpoint must declare fine-grained permissions: `[Authorize(Policy = PermissionKeys....)]`.
   - Security-relevant events (user modifications, inventory write-offs, salary/payroll actions) must declare `[Audit("...", Category = "...")]`.
   - Anonymous endpoints are strictly allowlisted; rate limiting must be applied via `[EnableRateLimiting("auth" | "general")]`.
4. **Secret Protection**:
   - Never commit API keys, connection strings, or JWT keys.
   - The startup guard in `Store.API/Program.cs` and `SecretEncryptionService.cs` strictly forbids default/placeholder keys in non-development environments.
5. **ClexAn Design System**:
   - All front-end UI work must adhere to the design specifications in [`docs/design_system_specification.md`](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/docs/design_system_specification.md).
   - Use CSS tokens defined in `wwwroot/css/tokens.css` (e.g., `--brand`, `--surface`, `--border`, `--z-modal`). Avoid hardcoded hex colors or bare `z-index` integers.

---

## 8. Common Troubleshooting & FAQ

### Q1: `SecretEncryptionService` throws `InvalidOperationException` on startup
- **Cause**: The application detected a placeholder value (e.g. `OVERRIDE_ME__...`) in production mode or an empty JWT/OTP key.
- **Fix**: When running locally, ensure `ASPNETCORE_ENVIRONMENT=Development` is set, or configure a local key ≥ 32 characters in `appsettings.Development.json` or via `dotnet user-secrets`.

### Q2: Port 5000 or 7112 is already in use
- **Cause**: A previous `dotnet run` instance or background process is still bound to the port.
- **Fix**: Check running processes:
  - Windows: `Get-Process -Name dotnet` or `netstat -ano | findstr 7112`, then terminate using `Stop-Process -Id <PID>`.
  - Linux / macOS: `lsof -i :7112` or `killall dotnet`.

### Q3: ClamAV virus scanner connection refused
- **Cause**: The API is configured to look for the ClamAV REST container, but none is running.
- **Fix**: In development, set `Antivirus:Provider=NoOp` in `appsettings.Development.json`. In production, ensure the ClamAV sidecar container is active.

### Q4: CORS errors when calling API from UI
- **Cause**: The origin URL of the UI is not in the API's allowed CORS origins list.
- **Fix**: In development, `Cors:AllowedOrigins` defaults to allow local origins. Verify that your UI port is listed in `Store.API/appsettings.Development.json`.

---

## 9. Key Documentation Links

- [**AGENTS.md**](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/AGENTS.md) — Comprehensive technical overview, security model, and rules for agents.
- [**docs/audit-tracker.md**](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/docs/audit-tracker.md) — Master tracker of security audits, bug fixes, and feature status.
- [**docs/design_system_specification.md**](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/docs/design_system_specification.md) — Design tokens, typography, component guidelines, and blade/modal rules.
- [**docs/security_runbook.md**](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/docs/security_runbook.md) — Incident response and security triage runbook.
- [**IMPLEMENTATION_LOG.md**](file:///c:/Users/Rodern/source/repos/Architech-Inc/StoreProject/IMPLEMENTATION_LOG.md) — Chronological log of all engineering remediation waves.
