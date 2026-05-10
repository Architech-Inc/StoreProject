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
- **Backend**: ASP.NET Core 8.0 Web API (minimal APIs), EF Core with MySQL/Pomelo provider.
- **Frontend**: ASP.NET Core Razor Pages and Blazor for web UI; .NET MAUI for mobile/tablet POS apps.
- **Database**: MySQL for transactional data; Redis for caching/sessions; object storage for receipts/docs.
- **Messaging**: RabbitMQ for asynchronous workflows with outbox pattern.
- **Infrastructure**: Modular monolith initially; evolves to microservices as needed. Deployable on Kubernetes/App Service/ECS.
- **Security**: JWT authentication, rate limiting, CORS, security headers, and audit logging.

### Project Structure
- `Store.API/`: ASP.NET Core Web API with controllers, middleware, and Swagger.
- `Store.UI/`: Razor Pages web UI for back-office and dashboards.
- `Store.DbServices/`: Data access layer with EF Core contexts, services, and migrations.
- `Store.Models/`: Shared DTOs, entities, and domain contracts.
- `Store.API.Tests/`: Unit tests for middleware and core logic.

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- MySQL Server (or Docker container)
- Node.js (if React components are added later)
- Android SDK (for .NET MAUI POS app development)

### Setup
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Architech-Inc/StoreProject.git
   cd StoreProject
   ```

2. **Database Setup**:
   - Install MySQL and create a database (e.g., `store_db`).
   - Update connection string in `Store.API/appsettings.json` or environment variables.
   - Run migrations:
     ```bash
     dotnet ef database update --project Store.DbServices
     ```

3. **Build and Run**:
   - Build the solution:
     ```bash
     dotnet build StoreProject.sln
     ```
   - Run the API:
     ```bash
     dotnet run --project Store.API
     ```
     - API available at `https://localhost:7112` (or configured URL).
     - Swagger UI at `https://localhost:7112/swagger`.
   - Run the UI (in another terminal):
     ```bash
     dotnet run --project Store.UI
     ```
     - Web UI at `https://localhost:5001` (or configured).

4. **Seed Data** (Development Only)**:
   - The API auto-seeds sample data on startup in Development environment.

### Configuration
- **JWT**: Set `Jwt:Key` in appsettings (strong secret, min 32 chars in production).
- **CORS**: Configure `Cors:AllowedOrigins` (wildcard allowed in dev, explicit in prod).
- **Mobile Money**: Add API keys for MTN MoMo/Orange Money in appsettings.
- **Database**: Connection string in `ConnectionStrings:StoreDb`.

### Testing
- Run unit tests:
  ```bash
  dotnet test Store.API.Tests
  ```
- For integration tests, ensure MySQL is running.

## API Documentation
- Access Swagger UI at `/swagger` when running the API.
- Key endpoints:
  - Auth: `/api/auth/login`, `/api/auth/refresh`
  - Users: `/api/users`
  - Inventory: `/api/inventory`
  - POS: `/api/invoices` (for sales)
  - Pricing: `/api/pricing`
  - Cash: `/api/cash`
- All endpoints require JWT Bearer token except login.

## Contributing
1. Fork the repository.
2. Create a feature branch.
3. Make changes and add tests.
4. Submit a pull request with a clear description.

## License
This project is licensed under the terms in [LICENSE.txt](LICENSE.txt).

## Support
For issues or questions, contact the development team or open an issue on GitHub.

---

*Built for Cameroon, designed for Africa and beyond.*