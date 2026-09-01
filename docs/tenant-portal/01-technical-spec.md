# 01 — Store.TenantPortal Technical Specification

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

---

## 1. Project Overview

`Store.TenantPortal` is a standalone ASP.NET Core 8 Razor Pages web application that gives prospective
and existing tenants a self-service interface to provision, monitor, and manage their ClexAn Foods
store silo — without needing access to the internal Control Plane admin API.

### Project Identity

| Property | Value |
|:---|:---|
| **Solution project name** | `Store.TenantPortal` |
| **Namespace root** | `Store.TenantPortal` |
| **Framework** | `net8.0` |
| **Project type** | ASP.NET Core Razor Pages |
| **Default port (dev)** | `9998` |
| **Production URL** | `https://portal.store.{rootDomain}` |
| **Communicates with** | `Store.ControlPlane` (HTTP, internal network) |

---

## 2. Solution Integration

### 2.1 New project in `StoreProject.sln`

```
StoreProject.sln
  ├── Store.Models          (shared DTOs, entities)
  ├── Store.DbServices      (business services, workers)
  ├── Store.API             (tenant REST API)
  ├── Store.UI              (tenant Razor Pages app)
  ├── Store.ControlPlane    (orchestration API — EXTENDED)
  ├── Store.TenantPortal    (NEW — this project)
  └── Store.API.Tests       (extended with portal tests)
```

### 2.2 Project References

`Store.TenantPortal` has **no project references** to other solution projects.
It is a fully independent web app that communicates **only** with `Store.ControlPlane`
via HTTP. This maintains clear separation of concerns and avoids circular dependencies.

All shared types (e.g. `TenantDto`, `BranchDto`) that the Portal needs from the
Control Plane are consumed as **HTTP response deserialization targets** — not as
compiled project references.

---

## 3. Directory Structure

```
Store.TenantPortal/
├── Store.TenantPortal.csproj
├── Program.cs
├── appsettings.json
├── appsettings.Production.json
│
├── Pages/
│   ├── Shared/
│   │   ├── _Layout.cshtml              # Portal-specific layout (NOT _AppLayout)
│   │   ├── _LoginPartial.cshtml
│   │   └── Error.cshtml
│   ├── Index.cshtml                    # Landing page
│   ├── Index.cshtml.cs
│   ├── Login.cshtml                    # Portal login
│   ├── Login.cshtml.cs
│   ├── Register.cshtml                 # New account registration
│   ├── Register.cshtml.cs
│   ├── Onboarding.cshtml               # 4-step provisioning wizard
│   ├── Onboarding.cshtml.cs
│   ├── Dashboard.cshtml                # Post-login home
│   ├── Dashboard.cshtml.cs
│   ├── Environment.cshtml              # Silo control panel
│   ├── Environment.cshtml.cs
│   ├── Domains.cshtml                  # Domain management
│   ├── Domains.cshtml.cs
│   ├── Branches.cshtml                 # Branch subdomain mapping
│   ├── Branches.cshtml.cs
│   ├── Backups.cshtml                  # Cloud backup config + history
│   ├── Backups.cshtml.cs
│   └── Settings.cshtml                 # Account & plan settings
│       └── Settings.cshtml.cs
│
├── Services/
│   ├── IControlPlaneClient.cs          # Typed HTTP client interface
│   ├── ControlPlaneClient.cs           # HTTP client wrapping all CP API calls
│   ├── IPortalSessionService.cs
│   ├── PortalSessionService.cs         # Cookie auth + session management
│   ├── IOAuthService.cs
│   ├── MicrosoftOAuthService.cs        # OneDrive OAuth2 flow
│   └── GoogleOAuthService.cs           # Google Drive OAuth2 flow
│
├── Models/
│   ├── PortalSession.cs                # Session data stored in cookie
│   ├── TenantSummaryVm.cs              # View models for dashboard
│   ├── EnvironmentVm.cs
│   ├── DomainVm.cs
│   ├── BranchVm.cs
│   └── BackupVm.cs
│
├── wwwroot/
│   ├── css/
│   │   ├── portal.css                  # Portal-specific stylesheet
│   │   └── portal-onboarding.css       # Wizard step animations
│   ├── js/
│   │   ├── portal.js                   # Global portal JS
│   │   ├── onboarding.js               # Wizard step navigation
│   │   ├── environment.js              # Health polling (SSE or setInterval)
│   │   └── domains.js                  # DNS check live feedback
│   └── images/
│       └── portal-logo.svg
│
└── Dockerfile
```

---

## 4. NuGet Dependencies

| Package | Version | Purpose |
|:---|:---|:---|
| `Microsoft.AspNetCore.Authentication.Cookies` | 8.x | Cookie-based session auth |
| `Microsoft.Extensions.Http` | 8.x | Typed `IHttpClientFactory` for Control Plane client |
| `Microsoft.Graph` | 5.x | OneDrive / Microsoft 365 backup integration |
| `Google.Apis.Drive.v3` | 1.x | Google Drive backup integration |
| `DnsClient` | 1.7.x | DNS TXT record lookup for domain verification |
| `FluentValidation.AspNetCore` | 11.x | Server-side model validation |
| `Polly` | 8.x | HTTP retry + circuit breaker for Control Plane calls |

---

## 5. Data Models (Portal-Side View Models)

These are **not** domain entities — they are deserialized from Control Plane API responses
and used as Razor Page view models.

### 5.1 `PortalSession`

Stored in the authentication cookie claims. Contains only what is needed to identify the
session — no secrets or full tenant data.

```csharp
public class PortalSession
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public TenantTier PlanTier { get; set; }
    public string Slug { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

**Stored as:** Named claims in the `ClaimsPrincipal`. The `TenantId` is the primary session key.
**Cookie lifetime:** 8 hours sliding expiry, `HttpOnly`, `SameSite=Strict`, `Secure` in production.

### 5.2 `EnvironmentVm`

```csharp
public class EnvironmentVm
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string UiUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime? LastHealthCheck { get; set; }
    public string? LastHealthMessage { get; set; }
    public List<ContainerCardVm> Containers { get; set; } = new();
    public List<ProvisioningLogVm> ProvisioningLogs { get; set; } = new();
}

public class ContainerCardVm
{
    public string Name { get; set; } = string.Empty;         // "MySQL", "MongoDB", "API", "UI"
    public string ContainerId { get; set; } = string.Empty;  // "{slug}-mysql", etc.
    public bool IsHealthy { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public DateTime? LastCheck { get; set; }
}

public class ProvisioningLogVm
{
    public string StepName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

### 5.3 `DomainVm`

```csharp
public class DomainVm
{
    public Guid TenantId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string PlatformUiUrl { get; set; } = string.Empty;
    public string PlatformApiUrl { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public DomainVerificationStatus CustomDomainStatus { get; set; }
    public string? VerificationRecordName { get; set; }    // "_clexan-verify.{domain}"
    public string? VerificationRecordValue { get; set; }   // "clxv_{token}"
}
```

### 5.4 `BranchVm`

```csharp
public class BranchVm
{
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string BranchSlug { get; set; } = string.Empty;
    public BranchDomainType DomainType { get; set; }
    public string ResolvedUrl { get; set; } = string.Empty;
    public DomainVerificationStatus VerificationStatus { get; set; }
    public DateTime DateCreated { get; set; }
}
```

### 5.5 `BackupVm`

```csharp
public class BackupVm
{
    public List<BackupProviderVm> Providers { get; set; } = new();
    public string? ScheduleCron { get; set; }
    public int RetentionDays { get; set; } = 7;
    public List<BackupJobVm> RecentJobs { get; set; } = new();
}

public class BackupProviderVm
{
    public Guid ProviderId { get; set; }
    public BackupProviderType ProviderType { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsConnected { get; set; }           // true if OAuth token is present and valid
    public string? TargetFolder { get; set; }
    public DateTime? LastBackupAt { get; set; }
    public bool? LastBackupSuccess { get; set; }
}

public class BackupJobVm
{
    public DateTime StartedAt { get; set; }
    public string DatabaseType { get; set; } = string.Empty;  // "MySQL" or "MongoDB"
    public string ProviderName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## 6. Service Layer

### 6.1 `IControlPlaneClient` / `ControlPlaneClient`

A typed HTTP client that wraps every call to `Store.ControlPlane`'s REST API.
All HTTP logic is isolated here — no raw `HttpClient` usage in PageModels.

```csharp
public interface IControlPlaneClient
{
    // Auth
    Task<PortalAuthResult> RegisterAsync(RegisterPortalRequest req, CancellationToken ct = default);
    Task<PortalAuthResult> LoginAsync(LoginPortalRequest req, CancellationToken ct = default);

    // Tenant lifecycle
    Task<TenantDto> ProvisionAsync(ProvisionTenantRequest req, CancellationToken ct = default);
    Task<TenantDetailDto?> GetTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDto?> SuspendAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDto?> ResumeAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> CheckHealthAsync(Guid tenantId, CancellationToken ct = default);

    // Domains
    Task<DomainConfigDto?> GetDomainConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task<DomainConfigDto> SetCustomDomainAsync(Guid tenantId, string domain, CancellationToken ct = default);
    Task<DomainVerificationResult> VerifyCustomDomainAsync(Guid tenantId, CancellationToken ct = default);
    Task RemoveCustomDomainAsync(Guid tenantId, CancellationToken ct = default);

    // Branches
    Task<List<BranchDto>> GetBranchesAsync(Guid tenantId, CancellationToken ct = default);
    Task<BranchDto> AddBranchAsync(Guid tenantId, AddBranchRequest req, CancellationToken ct = default);
    Task RemoveBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);

    // Backups
    Task<BackupConfigDto> GetBackupConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task UpdateBackupScheduleAsync(Guid tenantId, UpdateScheduleRequest req, CancellationToken ct = default);
    Task<List<BackupJobDto>> GetBackupHistoryAsync(Guid tenantId, CancellationToken ct = default);
    Task TriggerBackupAsync(Guid tenantId, CancellationToken ct = default);
    Task RemoveBackupProviderAsync(Guid tenantId, Guid providerId, CancellationToken ct = default);
}
```

**Resilience:** Registered with `Polly` retry (3 attempts, exponential backoff) and circuit breaker
(break after 5 consecutive failures for 30 seconds). Configured in `Program.cs`:

```csharp
builder.Services
    .AddHttpClient<IControlPlaneClient, ControlPlaneClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ControlPlane:BaseUrl"]!);
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddStandardResilienceHandler();
```

### 6.2 `IPortalSessionService` / `PortalSessionService`

Manages the authentication cookie lifecycle:

```csharp
public interface IPortalSessionService
{
    Task SignInAsync(HttpContext context, PortalSession session);
    Task SignOutAsync(HttpContext context);
    PortalSession? GetCurrentSession(HttpContext context);
    bool IsAuthenticated(HttpContext context);
}
```

### 6.3 `MicrosoftOAuthService`

Manages the Microsoft Graph OAuth2 Authorization Code flow for OneDrive:

```csharp
public interface IOAuthService
{
    Uri BuildAuthorizationUrl(Guid tenantId, string state);
    Task<OAuthTokenResult> ExchangeCodeAsync(string code, string state, CancellationToken ct);
    Task<string> GetValidAccessTokenAsync(Guid tenantId, CancellationToken ct);  // auto-refreshes
}
```

- **Scopes:** `Files.ReadWrite.AppFolder offline_access`
- **Redirect URI:** `https://portal.store.{rootDomain}/oauth/microsoft/callback`
- **Token storage:** Refresh token AES-256-GCM encrypted before storing in Control Plane

### 6.4 `GoogleOAuthService`

Same interface as above for Google Drive:
- **Scopes:** `https://www.googleapis.com/auth/drive.file`
- **Redirect URI:** `https://portal.store.{rootDomain}/oauth/google/callback`

---

## 7. Authentication Architecture

### 7.1 Cookie Auth Setup (`Program.cs`)

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Name = "TP_Session";
    });
```

### 7.2 Page Authorization

All pages except `/`, `/login`, `/register` require authentication.

```csharp
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Register");
});
```

### 7.3 Onboarding Flow Authorization

The `/onboarding` page requires the user to have completed registration but NOT yet
provisioned a tenant. A `[TenantNotProvisioned]` page filter enforces this:
if the session already has a `TenantId`, redirect to `/dashboard`.

---

## 8. Configuration

### 8.1 `appsettings.json` schema

```json
{
  "ControlPlane": {
    "BaseUrl": "http://localhost:9999",
    "ApiKey": ""
  },
  "PortalDomain": {
    "RootDomain": "store.157.173.112.19.nip.io",
    "PortalHost": "portal.store.157.173.112.19.nip.io"
  },
  "OAuth": {
    "Microsoft": {
      "ClientId": "",
      "ClientSecret": "",
      "TenantId": "common",
      "RedirectPath": "/oauth/microsoft/callback"
    },
    "Google": {
      "ClientId": "",
      "ClientSecret": "",
      "RedirectPath": "/oauth/google/callback"
    }
  },
  "DataProtection": {
    "KeyPath": "/root/.aspnet/DataProtection-Keys"
  }
}
```

### 8.2 Production Environment Variable Overrides

```bash
ControlPlane__BaseUrl=http://store-control-plane:9999
PortalDomain__RootDomain=store.yourcompany.com
PortalDomain__PortalHost=portal.store.yourcompany.com
OAuth__Microsoft__ClientId=<azure-client-id>
OAuth__Microsoft__ClientSecret=<azure-client-secret>
OAuth__Google__ClientId=<google-client-id>
OAuth__Google__ClientSecret=<google-client-secret>
```

---

## 9. Startup Pipeline (`Program.cs` outline)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(/* auth conventions */);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(/* options */);
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();

// Typed HTTP client to Control Plane with Polly resilience
builder.Services
    .AddHttpClient<IControlPlaneClient, ControlPlaneClient>( /* config */ )
    .AddStandardResilienceHandler();

// Portal services
builder.Services.AddScoped<IPortalSessionService, PortalSessionService>();
builder.Services.AddScoped<MicrosoftOAuthService>();
builder.Services.AddScoped<GoogleOAuthService>();

// Security headers
builder.Services.AddHsts(options => { options.MaxAge = TimeSpan.FromDays(365); });

var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapRazorPages();
app.MapGet("/oauth/microsoft/callback", OAuthCallbackHandlers.MicrosoftCallback);
app.MapGet("/oauth/google/callback", OAuthCallbackHandlers.GoogleCallback);

app.Run();
```

---

## 10. Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Store.TenantPortal/Store.TenantPortal.csproj", "Store.TenantPortal/"]
RUN dotnet restore "Store.TenantPortal/Store.TenantPortal.csproj"
COPY . .
WORKDIR "/src/Store.TenantPortal"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.TenantPortal.dll"]
```

---

## 11. Error Handling Strategy

| Scenario | Behaviour |
|:---|:---|
| Control Plane unreachable | Circuit breaker trips; branded "Service Temporarily Unavailable" page |
| Unauthenticated access to protected page | Redirect to `/login?returnUrl={path}` |
| Tenant already provisioned attempts re-provision | Redirect to `/dashboard` with info toast |
| OAuth state mismatch (CSRF) | Abort OAuth flow, log warning, redirect to `/backups` with error |
| Invalid slug in onboarding | Inline real-time validation before form submission |
| DNS verification failure | Clear error card showing expected vs found TXT record value |
