# Flutterwave Payment Gateway Integration Conversation History

## User Prompt

Can we add flutterwave ?

### Actions Taken
- **Grep searching**: `PaymentsController` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Grep searching**: `enum Payment` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Enums/StoreEnums.cs`

### Actions Taken
- **Grep searching**: `IMobileMoneyService` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Interfaces/Services/IMobileMoneyService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/MobileMoneyService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/MobileMoneyService.cs`

### Actions Taken
- **Grep searching**: `MobileMoneyProvider` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Enums/StoreEnums.cs`

### Actions Taken
- **Grep searching**: `PayDunya` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `c:/Users/Rodern/Downloads/StoreProject Export & Artifacts/Antigravity Chart Exports/Randoms.md`

### Actions Taken
- **Grep searching**: `flutterwave` in `c:\Users\Rodern\Downloads\StoreProject Export & Artifacts`

### Actions Taken
- **Viewing file**: `c:/Users/Rodern/Downloads/StoreProject Export & Artifacts/Antigravity Chart Exports/Randoms.md`

### Actions Taken
- **Viewing file**: `c:/Users/Rodern/Downloads/StoreProject Export & Artifacts/Antigravity Chart Exports/Randoms.md`

### Actions Taken
- **Grep searching**: `PayDunyaPaymentService` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Interfaces/Services/IPayDunyaPaymentService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Controllers/BillingController.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Controllers/BillingController.cs`

### Actions Taken
- **Asking Question**: `Where would you like to integrate Flutterwave within StoreProject?`
  - (Recommended) Both: SaaS Subscription Billing + In-Store POS & Customer Invoicing
  - Platform SaaS Subscription Billing only (TenantPortal & ControlPlane — expand beyond PayDunya to NGN, KES, GHS, USD)
  - In-Store POS & Customer Invoicing only (Store.API & Store.UI — cards, mobile money, and checkout links for store shoppers)

## User Response to Question

(Recommended) Both: SaaS Subscription Billing + In-Store POS & Customer Invoicing

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/DTOs/Payments/PaymentDtos.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/DTOs/Payments/PaymentDtos.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Services/SubscriptionReconciler.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Services/SubscriptionReconciler.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/PayDunyaPaymentService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/PayDunyaPaymentService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/PayDunyaPaymentService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Grep searching**: `InvoiceTender` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.DbServices`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/InvoiceService.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Services/ControlPlaneClient.cs`

### Actions Taken
- **Running command**: `dotnet test Store.API.Tests/Store.API.Tests.csproj --filter "FullyQualifiedName~PayDunya"`

**Assistant Note / Update**:

We are running the test suite to inspect existing payment tests while preparing the comprehensive architecture and implementation plan for Flutterwave. I'll analyze the output as soon as it completes.

### Actions Taken
- **Running command**: `dotnet test Store.API.Tests/Store.API.Tests.csproj --configuration Release --filter "FullyQualifiedName~PayDunya"`

**Assistant Note / Update**:

Checking test execution in Release configuration...

### Actions Taken
- **Running command**: `dotnet test Store.ControlPlane.Tests/Store.ControlPlane.Tests.csproj --configuration Release --filter "FullyQualifiedName~Billing"`

**Assistant Note / Update**:

Checking ControlPlane billing tests in Release configuration...

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/EndpointAuthorizationSecurityTests.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane.Tests/BillingControllerTests.cs`

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.Models\DTOs\Payments\FlutterwaveDtos.cs` — Create Flutterwave DTOs for hosted payment link generation, transaction verification, webhooks, and POS payment initiation.

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.Models\Interfaces\Services\IFlutterwavePaymentService.cs` — Create IFlutterwavePaymentService interface definition.

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Enums/StoreEnums.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.Models\Enums\StoreEnums.cs` — Add Flutterwave value to MobileMoneyProvider enum

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.DbServices\Services\FlutterwavePaymentService.cs` — Implement FlutterwavePaymentService with typed HttpClient, payment link generation, transaction verification, and constant-time webhook hash validation.

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/MobileMoneyService.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.DbServices\Services\MobileMoneyService.cs` — Map Flutterwave to its string representation alongside MTN MoMo and Orange Money

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Extensions/ServiceCollectionExtensions.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.DbServices\Extensions\ServiceCollectionExtensions.cs` — Add FlutterwaveOptions configuration and typed HttpClient for IFlutterwavePaymentService

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Services/SubscriptionReconciler.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane\Services\SubscriptionReconciler.cs` — Add string provider = "paydunya" parameter and use it in payment record, audit trail, and log

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane\Controllers\FlutterwaveBillingController.cs` — Create FlutterwaveBillingController for ControlPlane SaaS subscriptions.

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Program.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane\Program.cs` — Add FlutterwaveOptions configuration and typed HttpClient for IFlutterwavePaymentService in ControlPlane Program.cs

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/appsettings.json`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane\appsettings.json` — Add Payments section with Flutterwave configuration to ControlPlane appsettings.json

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Services/IControlPlaneClient.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.TenantPortal\Services\IControlPlaneClient.cs` — Add CreateFlutterwaveBillingInvoiceAsync to IControlPlaneClient interface

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Services/ControlPlaneClient.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.TenantPortal\Services\ControlPlaneClient.cs` — Add CreateFlutterwaveBillingInvoiceAsync method calling api/billing/flutterwave/invoice

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.TenantPortal\Pages\Billing.cshtml.cs` — Update OnPostSubscribeAsync to accept gateway and route to CreateFlutterwaveBillingInvoiceAsync or CreateBillingInvoiceAsync

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.TenantPortal\Pages\Billing.cshtml` — Add gateway dropdown selector and update footer text in Billing.cshtml

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Grep searching**: `Payments:` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Program.cs`

### Actions Taken
- **Grep searching**: `MoMoCallbackKey` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Program.cs`

### Actions Taken
- **Grep searching**: `MoMoCallbackKey` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/appsettings.json`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\appsettings.json` — Add Flutterwave configuration to Payments section in Store.API/appsettings.json

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/appsettings.Development.json`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\appsettings.Development.json` — Add Flutterwave dev values to Payments section in appsettings.Development.json

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Grep searching**: `new PaymentsController` in `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API.Tests`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/EndpointAuthorizationSecurityTests.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Controllers\PaymentsController.cs` — Add imports and inject IFlutterwavePaymentService, IInvoiceService, and IUnitOfWork in PaymentsController constructor

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Controllers\PaymentsController.cs` — Add Flutterwave endpoints following OrangeMoneyCallback in PaymentsController

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/EndpointAuthorizationSecurityTests.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/EndpointAuthorizationSecurityTests.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API.Tests\EndpointAuthorizationSecurityTests.cs` — Add PaymentsController.FlutterwaveWebhook to anonymous allowlist in EndpointAuthorizationSecurityTests

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API.Tests\EndpointAuthorizationSecurityTests.cs` — Add test verifying CashWrite policy on PaymentsController.InitiateFlutterwave

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/PayDunyaPaymentServiceTests.cs`

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API.Tests\FlutterwavePaymentServiceTests.cs` — Unit tests for FlutterwavePaymentService verifying webhook hash security, payment link creation, and transaction verification.

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane.Tests/BillingControllerTests.cs`

### Actions Taken
- **Creating file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane.Tests\FlutterwaveBillingControllerTests.cs` — Unit tests for FlutterwaveBillingController in Store.ControlPlane.Tests.

### Actions Taken
- **Running command**: `dotnet test Store.API.Tests/Store.API.Tests.csproj --configuration Release`

**Assistant Note / Update**:

Running the Store.API test suite in Release configuration...

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/DTOs/Common/ErrorCode.cs`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Entities/Invoice.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane\Controllers\FlutterwaveBillingController.cs` — Replace ErrorCode.ResourceNotFound with ErrorCode.NotFound

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.ControlPlane.Tests\FlutterwaveBillingControllerTests.cs` — Replace ErrorCode.ResourceNotFound with ErrorCode.NotFound in FlutterwaveBillingControllerTests

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\Store.API\Controllers\PaymentsController.cs` — Replace invoice.InvoiceNumber with invoice.InvoiceId.ToString()[..8]

### Actions Taken
- **Running command**: `dotnet test Store.API.Tests/Store.API.Tests.csproj --configuration Release`

**Assistant Note / Update**:

Running the Store.API test suite in Release configuration...

### Actions Taken
- **Running command**: `dotnet test Store.ControlPlane.Tests/Store.ControlPlane.Tests.csproj --configuration Release`

**Assistant Note / Update**:

Running the Store.ControlPlane test suite in Release configuration...

### Actions Taken
- **Running command**: `dotnet test Store.TenantPortal.Tests/Store.TenantPortal.Tests.csproj --configuration Release`

**Assistant Note / Update**:

Running the Store.TenantPortal test suite in Release configuration...

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/IMPLEMENTATION_LOG.md`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/IMPLEMENTATION_LOG.md`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/docs/audit-tracker.md`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/IMPLEMENTATION_LOG.md`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\IMPLEMENTATION_LOG.md` — Append Wave 49 documentation to IMPLEMENTATION_LOG.md

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/AGENTS.md`

### Actions Taken
- **Viewing file**: `C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/AGENTS.md`

### Actions Taken
- **Editing file**: `C:\Users\Rodern\source\repos\Architech-Inc\StoreProject\AGENTS.md` — Add Flutterwave environment variables and webhook hash verification details to AGENTS.md

### Actions Taken
- **Running command**: `dotnet build StoreProject.sln --configuration Release`

**Assistant Note / Update**:

Building the entire solution in Release configuration...

### Actions Taken
- **Running command**: `dotnet test Store.API.Tests/Store.API.Tests.csproj --configuration Release`

**Assistant Note / Update**:

Running Store.API tests...

### Actions Taken
- **Running command**: `dotnet test Store.ControlPlane.Tests/Store.ControlPlane.Tests.csproj --configuration Release`

## Assistant Response

Running Store.ControlPlane tests...

### Actions Taken
- **Running command**: `dotnet test Store.TenantPortal.Tests/Store.TenantPortal.Tests.csproj --configuration Release`

## Assistant Response

Running Store.TenantPortal tests...

### Actions Taken
- **Running command**: `git status --short`

## Assistant Response

Yes, **Flutterwave** is now fully integrated into **StoreProject** across both requested surfaces:

1. **Platform SaaS Subscription Billing** ([`Store.ControlPlane`](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane) & [`Store.TenantPortal`](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal))
2. **In-Store POS & Customer Invoicing** ([`Store.API`](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API) & [`Store.DbServices`](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices))

---

### What Was Added

#### 1. Models, Enums & Abstractions
- [**`FlutterwaveDtos.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/DTOs/Payments/FlutterwaveDtos.cs): Options, hosted checkout request/response payloads, transaction verification models, webhooks, and POS payment initiation DTOs.
- [**`StoreEnums.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Enums/StoreEnums.cs): Added `MobileMoneyProvider.Flutterwave`.
- [**`IFlutterwavePaymentService.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.Models/Interfaces/Services/IFlutterwavePaymentService.cs): Core interface contract for hosted checkout link generation, transaction verification, and constant-time webhook signature validation.

#### 2. Core Service Implementation
- [**`FlutterwavePaymentService.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/FlutterwavePaymentService.cs): Typed `HttpClient` implementing Flutterwave v3 API (`POST payments`, `GET transactions/{id}/verify`).
  - **Zero-trust security**: Constant-time `verif-hash` comparison using `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.
  - **Out-of-band verification**: Re-verifies every transaction directly via the Flutterwave API before applying payment state, preventing webhook spoofing or replay.
- [**`ServiceCollectionExtensions.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Extensions/ServiceCollectionExtensions.cs): Registered `FlutterwaveOptions` and typed `HttpClient<IFlutterwavePaymentService, FlutterwavePaymentService>`.
- [**`MobileMoneyService.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.DbServices/Services/MobileMoneyService.cs): Added provider display string mapping for `Flutterwave`.

#### 3. Surface 1: Platform SaaS Subscription Billing
- [**`FlutterwaveBillingController.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Controllers/FlutterwaveBillingController.cs):
  - `POST api/billing/flutterwave/invoice`: Generates hosted-checkout link for SaaS tenant plan upgrades (Starter, Professional, Enterprise) supporting NGN, KES, GHS, ZAR, USD, EUR, GBP, and XAF.
  - `GET api/billing/flutterwave/verify/{transactionId}`: Queries Flutterwave and reconciles tenant subscription state.
  - `POST api/billing/flutterwave/webhook`: Anonymous callback endpoint validating `verif-hash` and reconciling via `SubscriptionReconciler`.
- [**`SubscriptionReconciler.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane/Services/SubscriptionReconciler.cs): Added `provider` tagging to cleanly record payments and audit entries from both `"flutterwave"` and `"paydunya"`.
- [**`ControlPlaneClient.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Services/ControlPlaneClient.cs): Added `CreateFlutterwaveBillingInvoiceAsync`.
- [**`Billing.cshtml`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml) & [**`Billing.cshtml.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal/Pages/Billing.cshtml.cs): Added multi-gateway payment selector allowing tenants to choose between **Flutterwave (Cards, M-Pesa, MoMo, Bank Transfer)** and **PayDunya (XAF / WAEMU)**.

#### 4. Surface 2: In-Store POS & Customer Invoicing
- [**`PaymentsController.cs`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API/Controllers/PaymentsController.cs):
  - `POST api/payments/flutterwave/initiate`: `[Authorize(Policy = PermissionKeys.CashWrite)]` with `[Audit]`. Validates the invoice, creates a pending `MobileMoneyTransaction`, and generates a Flutterwave payment link.
  - `POST api/payments/flutterwave/webhook`: Validates `verif-hash`, performs out-of-band verification with Flutterwave API, marks the transaction completed, and creates an `InvoiceTender` line item via `InvoiceService.AddTenderAsync`.
  - `GET api/payments/flutterwave/verify/{transactionId}`: `[Authorize(Policy = PermissionKeys.PaymentsRead)]` for cashier / POS polling.

---

### Configuration & Environment Variables

Add the following configuration to `appsettings.json` or container environment variables:

| Environment Variable | Service | Description |
|---|---|---|
| `Payments__Flutterwave__SecretKey` | `Store.API` & `Store.ControlPlane` | Flutterwave v3 API Secret Key (`FLWSECK-...`) |
| `Payments__Flutterwave__PublicKey` | `Store.API` & `Store.ControlPlane` | Flutterwave Public Key (`FLWPUBK-...`) |
| `Payments__Flutterwave__SecretHash` | `Store.API` & `Store.ControlPlane` | Webhook verification secret hash configured in Flutterwave Dashboard |

---

### Verification & Test Results

- **Solution Build**: `dotnet build StoreProject.sln --configuration Release` — **0 warnings, 0 errors**.
- [**`Store.API.Tests`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests): **441 passed, 0 failed** (including 7 new tests in [FlutterwavePaymentServiceTests](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/FlutterwavePaymentServiceTests.cs) and security allowlist audits in [EndpointAuthorizationSecurityTests](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.API.Tests/EndpointAuthorizationSecurityTests.cs)).
- [**`Store.ControlPlane.Tests`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane.Tests): **57 passed, 0 failed** (including 6 new tests in [FlutterwaveBillingControllerTests](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.ControlPlane.Tests/FlutterwaveBillingControllerTests.cs)).
- [**`Store.TenantPortal.Tests`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/Store.TenantPortal.Tests): **27 passed, 0 failed**.
- **Total**: **525 passed, 0 failed**.
- Documented in [**`IMPLEMENTATION_LOG.md`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/IMPLEMENTATION_LOG.md) (Wave 49) and [**`AGENTS.md`**](file:///C:/Users/Rodern/source/repos/Architech-Inc/StoreProject/AGENTS.md).
