# StoreProject

StoreProject is a comprehensive, scalable retail management platform designed for Cameroon and Africa, built to handle everything from single-person kiosks to multinational supermarket chains. It provides end-to-end retail lifecycle management: procurement, inventory, POS sales, cash control, analytics, and multi-branch governance.

## Features

### Core Functionality
- **Authentication & Authorization**: JWT-based login with role-based access (Admin, Manager, Cashier). Supports username/email/phone login, refresh tokens, and permission-claim policies.
- **User & Identity Management**: CRUD operations for users, employees, customers, and suppliers with contact management.
- **Inventory Management**: Real-time stock tracking, goods receipt, returns, adjustments, reorder suggestions, and batch/lot expiry handling.
- **POS (Point of Sale)**: Offline-capable POS with barcode scanning, promotions, split payments, and mobile money integration (MTN MoMo, Orange Money).
- **Pricing & Promotions**: Tax profiles, bundle rules, segment pricing, and manager-approved discounts.
- **Cash Management**: Shift open/close, Z-reports, variance tracking, and reconciliation by payment channel.
- **Multi-Branch Support**: Centralized catalog, inter-branch transfers, approval workflows, and branch-level overrides.
- **Analytics & Reporting**: Sales trends, stockout rates, shrinkage detection, and exportable financial reports (OHADA-compliant).
- **Offline-First Design**: Continues operations during internet/power outages with background sync and conflict resolution.

### Advanced Modules
- **Procurement & Supplier Management**: Purchase orders, goods receipt notes, landed cost tracking, and supplier KPIs.
- **Warehouse & Logistics**: Multi-location bin tracking, stock counts, transfer requests, and route planning.
- **Customer Loyalty & Engagement**: Profiles, points accrual, SMS/WhatsApp campaigns, and segmentation.
- **Finance & Compliance**: VAT computations, accounting exports, immutable audit trails, and fraud controls.

### Scalability & Localization
- Scales from 1 kiosk to 5,000+ terminals across multi-country operations.
- Cameroon-first: French/English UI, XAF currency, OHADA accounting, mobile money, and low-cost Android device support.
- Africa-ready: Unreliable connectivity handling, power outage resilience, and extensible for other countries.

## Architecture

### Technology Stack
- **Backend**: ASP.NET Core 8.0 Web API with Clean Architecture (Dispatcher/Handlers/Ports) and EF Core (Pomelo MySQL provider).
- **Frontend**: ASP.NET Core Razor Pages consuming the ClexAn Design System tokens, responsive POS engine, and offline Service Worker.
- **SaaS & Multi-Tenancy**: `Store.ControlPlane` (tenant provisioning & lifecycle engine) and `Store.TenantPortal` (self-service signup).
- **Database & Storage**: MySQL 8.0 for relational transactions; MongoDB 6.0+ for audit trails/attachments.
- **Infrastructure**: Traefik 2.10 Edge Proxy, Docker Compose, ClamAV antivirus sidecar, and automated backup containers.
- **Security**: JWT Bearer auth with security-stamp rotation, fine-grained permission claim policies, HMAC callback validation, strict rate limiting, and security headers.

### Project Structure
- `Store.API/`: REST API layer with controllers, authorization policies, audit logging, and antivirus scanners.
- `Store.UI/`: Back-office and POS web interface for cashiers, managers, and administrators.
- `Store.ControlPlane/`: Tenant provisioning, suspension, lifecycle orchestration, and plan-quota enforcement.
- `Store.TenantPortal/`: Public SaaS onboarding and registration portal.
- `Store.DbServices/`: EF Core `StoreDbContext`, Pomelo MySQL migrations, and business services.
- `Store.Models/`: Shared DTOs, domain models, and permission keys (`PermissionKeys.All`).
- `Store.API.Tests/`: xUnit test suite (414+ tests) covering security policies, controllers, and domain logic.

## Getting Started

> **For step-by-step setup, see the complete [Developer Onboarding Guide](docs/onboarding.md).**

### Prerequisites
- .NET 8.0 SDK
- Docker Engine & Docker Compose
- MySQL 8.0 (local server or Docker container)
- PowerShell 7+ (Windows) or GNU Make (Linux/macOS)

### Quickstart with Task Runners

We provide unified task runners for both POSIX (`make`) and Windows PowerShell (`tasks.ps1`):

```bash
# 1. Clone repository
git clone https://github.com/Architech-Inc/StoreProject.git
cd StoreProject

# 2. Configure environment
cp .env.example .env

# 3. Build solution (Enforces 0 warnings policy)
# On Linux/macOS:
make build
# On Windows:
.\tasks.ps1 build

# 4. Run test suite
# On Linux/macOS:
make test
# On Windows:
.\tasks.ps1 test

# 5. Launch development services
# On Linux/macOS:
make run-api    # Terminal 1: REST API (https://localhost:7112)
make run-ui     # Terminal 2: Web Console (https://localhost:5001)

# On Windows:
.\tasks.ps1 run api
.\tasks.ps1 run ui
```

For a full list of runner commands (`make help` or `.\tasks.ps1 help`):
- `clean`: Remove build outputs and temporary caches
- `platform-up` / `platform-down`: Orchestrate Traefik and Control Plane via Docker Compose
- `provision-tenant`: Provision a new isolated tenant store stack
- `migrate`: Apply pending EF Core database migrations
- `audit`: Run clean release build and test suite verification

## Documentation & Runbooks

- [**Developer Onboarding Guide**](docs/onboarding.md) — Comprehensive local setup, database seeding, and troubleshooting FAQ.
- [**Agent & Contributor Rules (AGENTS.md)**](AGENTS.md) — Architecture principles, coding conventions, and secrets guards.
- [**Security Runbook**](docs/security_runbook.md) — Security incident response, secret rotation, and operational triage.
- [**ClexAn Design System Specification**](docs/design_system_specification.md) — Design tokens, component primitives, and UX rules.
- [**Master Audit Tracker**](docs/audit-tracker.md) — Status of all security, functional, and infrastructure audit items.
- [**Implementation Log**](IMPLEMENTATION_LOG.md) — Chronological record of remediation waves.

## Contributing
1. Fork the repository and create a feature branch.
2. Ensure solution builds clean with 0 warnings: `make build` / `.\tasks.ps1 build`.
3. Ensure all tests pass: `make test` / `.\tasks.ps1 test`.
4. Review the pull request against [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md).

## License
This project is licensed under the terms in [LICENSE.txt](LICENSE.txt).

---

*Built for Cameroon, designed for Africa and beyond.*