# StoreProject Software Requirements Specification (SRS)

## 1. Introduction

### 1.1 Purpose
This document defines the functional and non-functional requirements for StoreProject based on the current implementation in the repository. It is intended to align development, testing, and release planning for the API, data, and UI layers.

### 1.2 Scope
StoreProject is a retail/store management solution with:
- A REST API for identity, users, inventory, pricing, invoicing, orders, and cash operations.
- A Razor Pages web UI for authenticated operations and reporting.
- A shared domain model and infrastructure services using EF Core and MySQL.

### 1.3 Definitions
- JWT: JSON Web Token used for API authorization.
- Permission Claim: `perm` claim values used by policy authorization.
- RBAC: Role-based access control via role names (`Admin`, `Manager`, etc.) and permission keys.
- SRS: Software Requirements Specification.

## 2. Overall Description

### 2.1 Product Perspective
The solution is composed of four active projects:
- `Store.API` (ASP.NET Core Web API, `net8.0`)
- `Store.UI` (ASP.NET Core Razor Pages UI, `net8.0`)
- `Store.DbServices` (infrastructure/data services, `net8.0`)
- `Store.Models` (shared DTO/entity/domain contracts, `net8.0`)

Test coverage currently exists in `Store.API.Tests` for middleware behavior.

### 2.2 User Classes
- Administrator: full user/lookup/role matrix and operational write capabilities.
- Manager: operational and business management roles (depending on assigned permissions).
- Cashier/Operator: shift, cash, POS, inventory, and pricing reads/writes based on permission claims.

### 2.3 Operating Environment
- Runtime: .NET 8
- API host: ASP.NET Core with HTTPS
- Data store: MySQL via Pomelo EF provider
- UI: ASP.NET Core Razor Pages with server-side session and anti-forgery

### 2.4 Constraints
- Production must provide a strong JWT key (minimum 32 chars; no placeholder secret).
- Production CORS cannot be wildcard and must define explicit origins.
- API access for secured endpoints requires valid JWT and role/permission claims.

## 3. System Features and Functional Requirements

### 3.1 Authentication and Session Management
- FR-1.1 The system shall support login with username/password.
- FR-1.2 The system shall support login with email/password.
- FR-1.3 The system shall support login with phone/password.
- FR-1.4 The system shall support refresh token issuance.
- FR-1.5 The system shall support authenticated logout and token invalidation workflow.
- FR-1.6 The UI shall store access/refresh tokens in server session after successful login.
- FR-1.7 The UI shall redirect unauthenticated users to the login page for secured pages.

### 3.2 Authorization and Permissions
- FR-2.1 The API shall support role-based authorization on protected endpoints.
- FR-2.2 The API shall support policy authorization using `perm` claims for operations modules.
- FR-2.3 The UI shall read permission claims from JWT and gate page access/actions.
- FR-2.4 The role matrix module shall allow permission toggling for a role (authorized users only).

### 3.3 User and Identity Administration
- FR-3.1 The system shall provide CRUD-style user management endpoints (create, query, update, deactivate).
- FR-3.2 The system shall provide password change capability for authenticated users.
- FR-3.3 The system shall return standardized API envelopes or error responses for user operations.

### 3.4 Lookup and Master Data Management
- FR-4.1 The system shall manage categories (create, read, update, delete with role constraints).
- FR-4.2 The system shall manage units (create, read, update, delete with role constraints).
- FR-4.3 The system shall manage departments (create, read, update, delete with role constraints).

### 3.5 Core Business Operations
- FR-5.1 The system shall provide customer management APIs.
- FR-5.2 The system shall provide employee management APIs.
- FR-5.3 The system shall provide item management APIs, including low-stock retrieval.
- FR-5.4 The system shall provide invoice creation, listing, retrieval, and void operations.
- FR-5.5 The system shall provide order management APIs.

### 3.6 Inventory Operations Module
- FR-6.1 The system shall expose stock movement listing with pagination.
- FR-6.2 The system shall support goods receipt posting with item lines.
- FR-6.3 The system shall support stock return processing.
- FR-6.4 The system shall support stock adjustment/audit posting.
- FR-6.5 The system shall provide reorder suggestions.
- FR-6.6 The UI inventory operations page shall support movement viewing and write actions based on permission claims.

### 3.7 Pricing Operations Module
- FR-7.1 The system shall support tax profile retrieval and upsert.
- FR-7.2 The system shall support bundle rule retrieval and upsert.
- FR-7.3 The system shall support segment pricing retrieval and upsert.
- FR-7.4 The system shall support pricing preview for item/quantity/segment input.
- FR-7.5 The UI pricing operations page shall expose tax, bundle, segment pricing, and preview actions with permission gating.

### 3.8 Cash Management and Reports
- FR-8.1 The system shall provide active cashier shift retrieval.
- FR-8.2 The system shall support opening a cashier shift.
- FR-8.3 The system shall support closing a cashier shift.
- FR-8.4 The system shall provide daily Z-report retrieval by UTC date.
- FR-8.5 The UI cash and reports page shall support read/write/report operations based on permission claims.

### 3.9 Dashboard and POS UI
- FR-9.1 The dashboard shall display aggregate counts for items, customers, invoices, and low stock.
- FR-9.2 The dashboard shall display recent invoices and low-stock item lists.
- FR-9.3 The dashboard shall handle partial/failed data retrieval by surfacing a user-facing load error state.
- FR-9.4 The POS page shall serialize catalog and customer data for terminal interactions.

### 3.10 API Platform Behavior
- FR-10.1 The API shall expose Swagger/OpenAPI in development.
- FR-10.2 The API shall expose a health endpoint at `/health`.
- FR-10.3 The API shall return structured validation errors for invalid request payloads.
- FR-10.4 The API shall issue HTTP 429 responses on rate-limit rejection.

## 4. Non-Functional Requirements

### 4.1 Security
- NFR-1.1 JWT bearer authentication shall be required for protected API endpoints.
- NFR-1.2 Authorization shall enforce both role and permission-claim requirements where configured.
- NFR-1.3 API and UI shall enforce HTTPS in production runtime paths.
- NFR-1.4 UI session cookie and anti-forgery cookie shall be `HttpOnly`, `Secure`, and `SameSite=Strict`.
- NFR-1.5 The API shall add baseline security headers (including HSTS on HTTPS requests).

### 4.2 Reliability and Error Handling
- NFR-2.1 The API shall use centralized exception handling middleware.
- NFR-2.2 The API shall include correlation IDs for request traceability.
- NFR-2.3 The API shall produce consistent machine-readable error responses.

### 4.3 Performance and Scalability
- NFR-3.1 API rate limiting shall apply stricter limits on authentication endpoints than general endpoints.
- NFR-3.2 Dashboard data retrieval shall use concurrent requests for independent data sources.

### 4.4 Maintainability
- NFR-4.1 The solution shall maintain layered separation between API, infrastructure, and domain models.
- NFR-4.2 The solution shall centralize DTO/entity contracts in the shared models project.

### 4.5 Testability
- NFR-5.1 Middleware behavior for correlation and security headers shall be verifiable through automated tests.
- NFR-5.2 Additional coverage is required for controller/service business scenarios before production hardening.

## 5. Data Requirements

### 5.1 Persistence Model
The system shall persist core business data in MySQL via EF Core, including:
- Identity and access entities (users, roles, passwords, tokens, privileges).
- Business entities (items, invoices, sales, orders, customers, employees, suppliers, manufacturers).
- Operations module entities (stock movements, tax profiles, bundle rules, segment pricing, cashier shifts, role-permission mappings).

### 5.2 Data Integrity Expectations
- Token-related and role-permission data shall support authentication and authorization decisions.
- Item stock and pricing data shall support inventory, POS, and pricing workflows.
- Invoice and sale relationships shall support financial and audit reporting use cases.

## 6. External Interface Requirements

### 6.1 API Interface
Primary API controller groups include:
- Auth: `api/auth`
- Users: `api/users`
- Employees: `api/employees`
- Customers: `api/customers`
- Items: `api/item`, `api/items`
- Invoices: `api/invoices`
- Orders: `api/orders`
- Lookup: `api/categories`, `api/units`, `api/departments`
- Inventory Operations: `api/inventory`
- Pricing Operations: `api/pricing`
- Cash Management: `api/cash`
- Role Matrix Admin: `api/admin/role-matrix`

### 6.2 UI Interface
Primary UI pages include:
- Login
- Dashboard
- POS
- InventoryOps
- PricingOps
- CashReports
- RoleMatrix

### 6.3 Configuration Interface
The system shall consume environment/appsettings values for:
- JWT key, issuer, and audience.
- CORS allowed origins.
- API base URL used by UI HttpClient.
- Database and related service settings.

## 7. Assumptions and Risks
- AR-1 Only middleware-focused automated tests are currently present; business logic regression risk remains.
- AR-2 Session-based token handling in UI assumes server-side session availability and valid token lifetimes.
- AR-3 The SRS reflects the repository as implemented; future feature growth should add explicit requirement IDs and acceptance criteria per module.

## 8. Traceability to Current Codebase

### 8.1 Requirements-to-Code Mapping (Representative)
- FR-1.x: `Store.API/Controllers/AuthController.cs`, `Store.UI/Pages/Login.cshtml.cs`, `Store.UI/Services/ApiAuthenticationService.cs`
- FR-2.x: `Store.API/Program.cs`, `Store.API/Controllers/*Operations*.cs`, `Store.UI/Pages/SecurePageModel.cs`, `Store.UI/Services/JwtPermissionReader.cs`
- FR-3.x: `Store.API/Controllers/UsersController.cs`
- FR-4.x: `Store.API/Controllers/LookupControllers.cs`
- FR-5.x: `Store.API/Controllers/CustomersController.cs`, `Store.API/Controllers/EmployeesController.cs`, `Store.API/Controllers/ItemController.cs`, `Store.API/Controllers/InvoicesController.cs`, `Store.API/Controllers/OrdersController.cs`
- FR-6.x: `Store.API/Controllers/InventoryOperationsController.cs`, `Store.UI/Pages/InventoryOps.cshtml.cs`
- FR-7.x: `Store.API/Controllers/PricingController.cs`, `Store.UI/Pages/PricingOps.cshtml.cs`
- FR-8.x: `Store.API/Controllers/CashManagementController.cs`, `Store.UI/Pages/CashReports.cshtml.cs`
- FR-9.x: `Store.UI/Pages/Dashboard.cshtml.cs`, `Store.UI/Pages/Pos.cshtml`
- FR-10.x/NFR-1.x/NFR-2.x/NFR-3.x: `Store.API/Program.cs`, `Store.API/Middleware/*.cs`
- NFR-5.x: `Store.API.Tests/SecurityMiddlewareTests.cs`

## 9. Future SRS Expansion Recommendations
- Add measurable acceptance criteria for each FR/NFR (inputs, outputs, and pass/fail rules).
- Add sequence diagrams for auth, invoice creation, and pricing preview workflows.
- Add data dictionary tables for key entities and DTOs.
- Add explicit operational SLAs (availability, latency, backup/recovery targets).

## 10. Cameroon-First, Pan-African, Global Expansion Blueprint

### 10.1 Product Vision and Scaling Tiers
StoreProject shall operate as one unified platform that scales across four tiers:
- Tier 1: Single-person kiosk (single terminal, offline-heavy, minimal admin overhead).
- Tier 2: Small retail shop (2-5 users, local inventory control, basic reporting).
- Tier 3: Multi-branch supermarket chain (central catalog/pricing, branch performance governance).
- Tier 4: Multi-national retail group (multi-country, multi-currency, multi-legal-entity operations).

### 10.2 End-to-End Retail Lifecycle Coverage
The platform shall cover every stage of the retail lifecycle:
- Buy: procurement, purchase orders, supplier management, goods receipt.
- Move: warehouse management, inter-branch transfers, stock counts, logistics.
- Sell: POS transactions, invoices, returns, promotions, customer engagement.
- Control: cash management, audit trail, role matrix, fraud detection, compliance.
- Understand: analytics dashboards, forecasting, BI exports, KPI tracking.

### 10.3 Cameroon Context Requirements
- CR-1 Connectivity resilience: POS and branch workflows shall continue during unstable internet and power conditions, with deferred synchronization.
- CR-2 Payment realities: the platform shall support cash, card (where available), MTN MoMo, and Orange Money integration flows.
- CR-3 Localization: French and English UI shall be first-class language options.
- CR-4 Currency baseline: XAF shall be default, with extensibility for USD/EUR and additional currencies.
- CR-5 Compliance baseline: VAT reporting and OHADA-aligned accounting exports shall be supported.
- CR-6 Device constraints: low-cost Android tablets, thermal printers, and barcode scanners shall be supported with graceful degradation.

### 10.4 Functional Expansion Modules (Beyond Current Implementation)

#### 10.4.1 Procurement and Supplier Management
- EX-FR-1.1 The system shall maintain supplier master records with contacts, terms, and lead times.
- EX-FR-1.2 The system shall support purchase requisitions, purchase orders, and goods-received notes.
- EX-FR-1.3 The system shall support landed-cost capture for imports (freight, duty, handling).
- EX-FR-1.4 The system shall track supplier performance KPIs (lead time adherence, fill rates, quality incidents).

#### 10.4.2 Warehouse and Logistics
- EX-FR-2.1 The system shall support multi-location warehouse modeling (zone/bin/shelf).
- EX-FR-2.2 The system shall support branch transfer requests, approvals, dispatch, and receipt confirmation.
- EX-FR-2.3 The system shall support handheld stock-count workflows with discrepancy resolution.
- EX-FR-2.4 The system shall support route planning inputs for branch replenishment dispatches.

#### 10.4.3 Advanced Pricing and Promotion Engine
- EX-FR-3.1 The system shall support rule-based promotions (time-based, quantity-based, segment-based).
- EX-FR-3.2 The system shall support coupon and campaign discount mechanisms with usage limits.
- EX-FR-3.3 The system shall support manager approval flows for exceptional discount overrides.
- EX-FR-3.4 The system shall provide promotion effectiveness reporting by SKU, category, and branch.

#### 10.4.4 Customer, Loyalty, and Engagement
- EX-FR-4.1 The system shall maintain customer profiles with opt-in consent tracking.
- EX-FR-4.2 The system shall support loyalty point accrual and redemption rules.
- EX-FR-4.3 The system shall support campaign integration channels (SMS/WhatsApp/email adapters).
- EX-FR-4.4 The system shall support customer segmentation for targeted pricing and offers.

#### 10.4.5 Finance and Reconciliation Controls
- EX-FR-5.1 The system shall support day-end reconciliation by cashier, terminal, branch, and payment channel.
- EX-FR-5.2 The system shall support cash variance workflows with reason codes and manager sign-off.
- EX-FR-5.3 The system shall export accounting journals mapped to OHADA-compliant chart structures.
- EX-FR-5.4 The system shall support tax reporting packs by period, branch, and legal entity.

#### 10.4.6 Multi-Entity Governance
- EX-FR-6.1 The system shall support hierarchical org structures (group > country > legal entity > region > branch).
- EX-FR-6.2 The system shall support centralized policy distribution (pricing, roles, tax, catalog constraints).
- EX-FR-6.3 The system shall support branch-level overrides only where policy permits.
- EX-FR-6.4 The system shall support inter-entity reporting with secure data partitioning.

## 11. Target Architecture and Evolution Strategy

### 11.1 Recommended Delivery Strategy
- ARH-1 The initial architecture should be a modular monolith to reduce complexity and speed delivery.
- ARH-2 Rationale for modular monolith: faster delivery, lower operational cost, easier consistency for offline and sync workflows.
- ARH-3 Evolution path:
  - Phase A: modular monolith — all domain modules in one deployable.
  - Phase B: extract Inventory Sync service when branch volume requires it.
  - Phase C: extract Payments and Analytics pipelines for independent scaling.
  - Phase D: selective microservices only where scale and team capacity justify it.
- ARH-4 Service extraction shall preserve API contracts and event schemas to avoid branch disruption.

### 11.2 Logical Architecture Components
- Client Applications:
  - Web back office and UI: ASP.NET Core Razor Pages and Blazor (current stack) as the primary web layer. React + TypeScript shall only be introduced where Blazor cannot adequately address a specific UI requirement (e.g., highly interactive data-visualization widgets).
  - POS tablet app: .NET MAUI for store staff and cashier operations on low-cost Android devices.
  - React Native is not used; all mobile/tablet POS development is .NET MAUI.
  - Optional lightweight web POS fallback via Blazor Server for desktop browsers.
- API Layer:
  - API Gateway / BFF pattern for client-facing API aggregation.
  - Versioned domain APIs with auth, policy gates, idempotency, and validation.
- Domain Modules: identity, POS, inventory, pricing, invoicing, cash, procurement, warehouse, reporting.
- Background Processing: sync worker, settlement worker, notification worker, export/report worker.
- Data Layer:
  - PostgreSQL or MySQL for transactional core.
  - Redis for caching, session, and queue-backed workflows.
  - Object storage for receipts, documents, and exports.
  - Read replicas and scheduled aggregates for analytics; lakehouse layer added at cross-country scale.
- Event Layer: RabbitMQ or Azure Service Bus for asynchronous workflows with reliable outbox publishing.
- Infrastructure: Kubernetes or App Service/ECS depending on team maturity; CDN + WAF; centralized monitoring and alerting.

### 11.3 Data and Event Reliability Patterns
- ARH-5 Write APIs shall support idempotency keys to prevent duplicate financial/stock actions.
- ARH-6 Outbox pattern shall be used for durable event publishing from transactional writes.
- ARH-7 Failed asynchronous processing shall route to dead-letter handling with operator visibility.

## 12. Core Module Functional Detail

### 12.1 POS Terminal Requirements
- POS-1 Cashier shall search products by barcode scan or name with results < 300 ms on local cache.
- POS-2 POS shall support suspend/resume sale for handling multiple concurrent transactions.
- POS-3 POS shall support split payment and mixed tender types (cash + mobile money in one transaction).
- POS-4 POS shall issue receipts offline and queue reconciliation with server on reconnect.
- POS-5 POS UX shall be optimized for low-training cashier environments with minimal navigation depth.

### 12.2 Payments Module Requirements
- PAY-1 The system shall support a cash drawer open/close workflow tied to sale events.
- PAY-2 The system shall support Mobile Money (MTN MoMo, Orange Money) authorization and callback handling.
- PAY-3 The system shall support daily settlement reporting by payment channel.
- PAY-4 The system shall support partial payment recording and outstanding balance tracking.
- PAY-5 Payment transactions shall be idempotent; duplicate callbacks shall not produce duplicate records.

### 12.3 Inventory Module Detail
- INV-1 Stock on hand shall be tracked by branch and, where applicable, by warehouse bin location.
- INV-2 The system shall support batch/lot tracking and expiry-date management for perishable and regulated goods.
- INV-3 Wastage and shrinkage events shall be captured as typed stock adjustment records.
- INV-4 Reorder point triggers shall generate auto-purchase suggestions surfaced to procurement.

### 12.4 Multi-Branch Control Tower
- MCT-1 Branch performance dashboard shall aggregate sales, stock, and cash KPIs per branch.
- MCT-2 Central catalog and pricing changes shall be distributed to branches via policy push.
- MCT-3 Inter-branch stock transfer workflows shall require request, approval, dispatch, and receipt steps.
- MCT-4 Approval workflows for discounts, voids, and overrides shall be enforced across all branches.

## 13. Data Model Structure

### 13.1 Tenant and Organizational Hierarchy
The data model shall support:
- Organization (group-level entity).
- Country (legal and regulatory scope).
- Legal Entity (VAT registration, accounting unit).
- Region (operational grouping).
- Branch (physical store or kiosk).

### 13.2 Product and Catalog Hierarchy
- Category and subcategory tree.
- SKU with unit of measure and conversion rates.
- Product variants (size, weight, pack) and bundle definitions.
- Price lists per customer segment and branch policy.

### 13.3 Inventory Ledger Design
- Movement table shall be the authoritative source of truth for stock changes.
- Derived stock balance views shall be computed from the movement ledger.
- Each movement shall record: type, quantity, actor, branch, lot/batch, timestamp, and reference.

### 13.4 Financial Record Structure
- Sale: header (branch, cashier, customer, date) + line items (SKU, qty, unit price, discount, tax).
- Payment transactions: linked to sale, typed by tender, with reference codes for mobile money.
- Tax breakdowns: per line and per invoice, linked to active tax profile.
- Refunds and voids: linked to original sale with reason, actor, and approval reference.

### 13.5 Identity and Authorization Model
- User-role-branch assignment table controls which user has which role at which branch.
- Permission grants/overrides at user or role level for granular access control.

### 13.6 Audit Record Structure
- Each audit record shall capture: actor (user id), action type, affected entity, before/after snapshot, device id, and correlation id.

## 14. Offline-First Design Requirements
- OFF-1 POS shall support local transaction capture when disconnected.
- OFF-2 Local persistent storage (for example SQLite on terminal devices) shall queue writes until sync.
- OFF-3 Sync shall support retry with exponential backoff and jitter.
- OFF-4 Sync conflicts shall use explicit domain policies:
  - inventory = server-authoritative with compensating entries,
  - customer metadata = last-write-wins with audit,
  - pricing/catalog = HQ-authoritative.
- OFF-5 Branch operators shall be warned when critical reference data is stale beyond threshold.
- OFF-6 Replayed sync operations shall be idempotent and traceable by correlation/idempotency IDs.

## 15. Security, Compliance, and Fraud Controls (Expanded)

### 15.1 Security Controls
- SEC-1 Role-based and permission-claim authorization shall be branch-scoped where applicable.
- SEC-2 Manager/HQ users shall support step-up authentication (MFA) for sensitive actions.
- SEC-3 Sensitive data shall be encrypted at rest and in transit.
- SEC-4 Device registration/binding controls shall be available for POS terminals.

### 15.2 Compliance Controls
- COM-1 The platform shall support VAT computations and period-end tax outputs.
- COM-2 Financial exports shall support OHADA-oriented accounting structures.
- COM-3 The platform shall maintain immutable audit logs for sensitive operations.
- COM-4 Data retention and data-access logs shall support legal and internal audit requests.

### 15.3 Fraud and Abuse Controls
- FRA-1 Discount override attempts beyond threshold shall require approval.
- FRA-2 Unusual refund and void patterns shall trigger alerting.
- FRA-3 Cash variance outliers shall trigger manager review and investigation workflow.

## 16. Non-Functional Requirements (Expanded Targets)
- ENFR-1 Availability target for core POS APIs shall be >= 99.9% monthly.
- ENFR-2 POS product lookup shall meet p95 < 300 ms on warm cache paths.
- ENFR-3 Online sale commit shall target p95 < 2 seconds; offline local commit < 500 ms.
- ENFR-4 Platform shall support growth from 1 kiosk to 5,000+ terminals through horizontal scaling patterns.
- ENFR-5 Central observability shall provide logs, metrics, and traces with correlation IDs end-to-end.
- ENFR-6 Recovery objectives should target RPO <= 15 minutes and RTO <= 2 hours for critical services.

## 17. Delivery Roadmap and Rollout Plan

### 17.1 Recommended 18-Month Program
- Phase 1 (Months 1-3): core identity, offline-capable POS, catalog/stock read, cash payments.
- Phase 2 (Months 4-6): inventory movements, procurement basics, mobile money integrations, branch dashboards.
- Phase 3 (Months 7-9): promotions, returns/refunds controls, role governance deepening, transfer workflows.
- Phase 4 (Months 10-12): forecasting v1, fraud alerting v1, accounting/tax export packs.
- Phase 5 (Months 13-18): country-pack framework, multi-entity model, advanced BI and central governance.

### 17.2 Cameroon Pilot Execution Model
- PILOT-1 Select one pilot branch with clear baseline KPIs before implementation.
- PILOT-2 Define 90-day MVP scope: POS offline, inventory receive/adjust, MoMo/Orange Money, shift open/close, base dashboards.
- PILOT-3 Conduct controlled expansion to 3-5 branches after KPI thresholds are met.
- PILOT-4 Use feature flags and staged rollout for risk control in production.

## 18. KPI and Success Measurement Framework

### 18.1 Business KPIs
- KPI-B1 Daily gross sales by branch and channel.
- KPI-B2 Gross margin by category and branch.
- KPI-B3 Stockout rate and shrinkage rate.

### 18.2 Operations KPIs
- KPI-O1 Replenishment lead time and supplier fill rate.
- KPI-O2 Cash variance per shift and reconciliation accuracy.
- KPI-O3 Returns/voids rate with anomaly segmentation.

### 18.3 Platform KPIs
- KPI-P1 Sync success ratio and average sync delay.
- KPI-P2 Terminal uptime and incident frequency.
- KPI-P3 API latency/error SLO attainment by module.

### 18.4 Adoption KPIs
- KPI-A1 Active cashiers per day by branch.
- KPI-A2 Feature usage rate by branch maturity tier.

## 19. Deployment and Environment Strategy

### 19.1 Environment Definitions
- DEP-1 Four environments shall be maintained: Dev, Test, Staging, and Production.
- DEP-2 Staging shall mirror Production configuration and data shape for pre-release validation.

### 19.2 Release Strategy
- DEP-3 Backend deployments shall use blue/green strategy to minimize downtime risk.
- DEP-4 High-risk features shall be gated behind feature flags and enabled per branch/region.
- DEP-5 Remote config shall be used for branch-level rollout without redeployment.

### 19.3 Branch Rollout Model
- DEP-6 Phase 1: pilot in one kiosk or store.
- DEP-7 Phase 2: expand to 3-5 stores in Douala and Yaounde.
- DEP-8 Phase 3: regional rollout across Cameroon.
- DEP-9 Phase 4: country-pack template replication for additional countries.

## 20. Team Structure and Roles

### 20.1 Product and Business
- Product Manager: owns scope, backlog, and stakeholder alignment.
- Retail Operations SME: validates workflows against real store operational reality.
- Implementation Lead: drives delivery and coordinates cross-functional dependencies.

### 20.2 Engineering
- Backend Squad: API, domain modules, event handling, database.
- POS/Mobile Squad: .NET MAUI POS app, offline sync, and device integration (Android tablets, scanners, printers).
- Frontend Dashboard Squad: React + TypeScript back-office and reporting.
- Data/Analytics Engineer: data pipelines, aggregates, BI layer.
- DevOps/SRE: CI/CD, infrastructure, monitoring, incident response.

### 20.3 QA and Delivery
- Automation QA: test coverage for API contracts, sync scenarios, and critical business flows.
- Field QA: device, network, and power-failure scenario testing in real store conditions.
- Support and Training Team: L1/L2 support, cashier/manager training materials, SOPs.

## 21. Cameroon Go-Live Readiness Checklist

### 21.1 Business Readiness
- GL-B1 Standard operating procedures documented for cashier, manager, and auditor roles.
- GL-B2 Branch opening and closing procedures defined and tested.

### 21.2 Device and Hardware Readiness
- GL-D1 Tablets, barcode scanners, and thermal printers tested end-to-end per branch configuration.
- GL-D2 Offline/online transition tested under simulated internet interruption and power failure scenarios.

### 21.3 Payment Readiness
- GL-P1 MTN MoMo and Orange Money provider certifications obtained.
- GL-P2 Reconciliation report format validated with finance team.

### 21.4 Legal and Finance Readiness
- GL-L1 VAT report outputs reviewed and approved by accountant.
- GL-L2 Audit export samples reviewed and accepted by compliance/finance.

### 21.5 Operational Readiness
- GL-O1 Support playbook published and L1/L2/L3 ownership assigned.
- GL-O2 Incident response and escalation matrix defined.
- GL-O3 Staff training complete with sign-off for all active roles.

## 22. Immediate Next Steps (Cameroon v1 Pilot)
- NS-1 Freeze v1 scope: POS offline, inventory receive/adjust, mobile money, shift open/close, base dashboards.
- NS-2 Write a formal v1 SRS with measurable acceptance criteria per module.
- NS-3 Build a 90-day implementation plan with weekly deliverables and named owners.
- NS-4 Select one pilot branch and define baseline success metrics before development begins.
- NS-5 Validate hardware and payment provider availability in the selected pilot location.
