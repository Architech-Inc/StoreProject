using Store.Models.Interfaces.Services;
using Store.Models.Logging;
using StoreUI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEnterpriseLogging("Store.UI");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "storeui-antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.HeaderName = "RequestVerificationToken";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "storeui-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// API HttpClient with JWT token handling
var internalApiUrl = builder.Configuration["ApiSettings:InternalBaseUrl"] ?? builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7112";
builder.Services.AddHttpClient("StoreApi", client =>
{
    client.BaseAddress = new Uri(internalApiUrl);
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<IApiClientService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<ApiClientService>>();
    return new ApiClientService(factory.CreateClient("StoreApi"), logger);
});

// API service implementations
builder.Services.AddScoped<IAuthenticationService, ApiAuthenticationService>();
builder.Services.AddScoped<IUserService, ApiUserService>();
builder.Services.AddScoped<IEmployeeService, ApiEmployeeService>();
builder.Services.AddScoped<ICustomerService, ApiCustomerService>();
builder.Services.AddScoped<IItemService, ApiItemService>();
builder.Services.AddScoped<IApiPasswordRecoveryService, ApiPasswordRecoveryService>();
builder.Services.AddScoped<IInvoiceService, ApiInvoiceService>();
builder.Services.AddScoped<IOrderService, ApiOrderService>();
builder.Services.AddScoped<ILoyaltyCampaignService, ApiCampaignService>();
builder.Services.AddScoped<IDiscountService, ApiDiscountService>();
builder.Services.AddScoped<IBatchService, ApiBatchService>();
builder.Services.AddScoped<IStockTransferService, ApiStockTransferService>();
builder.Services.AddScoped<IWastageService, ApiWastageService>();
builder.Services.AddScoped<IDiscountOverrideService, ApiDiscountOverrideService>();
builder.Services.AddScoped<IPurchaseOrderService, ApiPurchaseOrderService>();
builder.Services.AddScoped<ICashVarianceService, ApiCashVarianceService>();
builder.Services.AddScoped<ISupplierService, ApiSupplierService>();
builder.Services.AddScoped<ILoyaltyService, ApiLoyaltyService>();
builder.Services.AddScoped<IFileService, ApiFileService>();
builder.Services.AddScoped<IApiCommunicationLogService, ApiCommunicationLogService>();
builder.Services.AddScoped<IAuditLogService, ApiAuditLogService>();
builder.Services.AddScoped<ICategoryService, ApiCategoryService>();
builder.Services.AddScoped<IEmployeeManager, EmployeeManager>();
builder.Services.AddScoped<IInventoryOpsManager, InventoryOpsManager>();
builder.Services.AddScoped<IBatchManager, BatchManager>();
builder.Services.AddScoped<IStockTransferManager, StockTransferManager>();
builder.Services.AddScoped<IWastageManager, WastageManager>();
builder.Services.AddScoped<IPurchaseOrderManager, PurchaseOrderManager>();
builder.Services.AddScoped<IAuditLogManager, AuditLogManager>();
builder.Services.AddScoped<IDiscountManager, DiscountManager>();
builder.Services.AddScoped<IDiscountOverrideManager, DiscountOverrideManager>();
builder.Services.AddScoped<IPricingOpsManager, PricingOpsManager>();
builder.Services.AddScoped<IPromotionEffectivenessManager, PromotionEffectivenessManager>();
builder.Services.AddScoped<ICashVarianceManager, CashVarianceManager>();
builder.Services.AddScoped<ICashReportsManager, CashReportsManager>();
builder.Services.AddScoped<IReconciliationManager, ReconciliationManager>();
builder.Services.AddScoped<IInvoiceManager, InvoiceManager>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<ILookupManager, LookupManager>();
builder.Services.AddScoped<ICommunicationManager, CommunicationManager>();
builder.Services.AddScoped<IBranchManager, BranchManager>();
builder.Services.AddScoped<IPaymentsManager, PaymentsManager>();
builder.Services.AddScoped<IContactRequestManager, ContactRequestManager>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<IPayrollManager, PayrollManager>();
builder.Services.AddScoped<IFinanceManager, FinanceManager>();
builder.Services.AddSingleton<IBreadcrumbService, BreadcrumbService>();

var app = builder.Build();

// SEC-21 — Forwarded headers for reverse-proxy (Traefik / Docker) TLS termination.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.TryAdd("X-Content-Type-Options", "nosniff");
    headers.TryAdd("X-Frame-Options", "DENY");
    headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
    headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var externalApiUrl = app.Configuration["ApiSettings:ExternalBaseUrl"] ?? app.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7112";
    var targetBase = externalApiUrl.TrimEnd('/');

    if (context.Request.Path.StartsWithSegments("/files", out var remainingPath))
    {
        context.Response.Redirect($"{targetBase}/files{remainingPath}{context.Request.QueryString}");
        return;
    }
    
    await next();
});
app.UseSession();

app.UseRouting();

app.MapRazorPages();

app.MapPost("/api/webauthn/makeCredentialOptions", async (HttpContext httpContext, IHttpClientFactory factory, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();
    
    var client = factory.CreateClient("StoreApi");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.PostAsync("/api/webauthn/makeCredentialOptions", null, ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapPost("/api/webauthn/makeCredential", async (HttpContext httpContext, IHttpClientFactory factory, System.Text.Json.JsonElement request, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();
    
    var client = factory.CreateClient("StoreApi");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.PostAsJsonAsync("/api/webauthn/makeCredential", request, ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapPost("/api/webauthn/assertionOptions", async (IHttpClientFactory factory, System.Text.Json.JsonElement request, CancellationToken ct) =>
{
    var client = factory.CreateClient("StoreApi");
    var response = await client.PostAsJsonAsync("/api/webauthn/assertionOptions", request, ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapPost("/api/webauthn/makeAssertion", async (HttpContext httpContext, IHttpClientFactory factory, System.Text.Json.JsonElement request, CancellationToken ct) =>
{
    var client = factory.CreateClient("StoreApi");
    var response = await client.PostAsJsonAsync("/api/webauthn/makeAssertion", request, ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    
    if (response.IsSuccessStatusCode)
    {
        // Try to parse the login response to set the session token
        try 
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("data", out var dataElement) && 
                dataElement.TryGetProperty("accessToken", out var tokenElement))
            {
                httpContext.Session.SetString("access_token", tokenElement.GetString() ?? "");
            }
        }
        catch { }
    }
    
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapGet("/api/auth/avatar/{username}", async (IHttpClientFactory factory, string username, CancellationToken ct) =>
{
    var client = factory.CreateClient("StoreApi");
    var response = await client.GetAsync($"/api/auth/avatar/{username}", ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapGet("/api/scanner/resolve", async (HttpContext httpContext, IApiClientService apiClient, string code, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }
    apiClient.SetToken(token);
    var result = await apiClient.GetAsync<Store.Models.DTOs.Scanner.ScanResolutionResultDto>(
        $"/api/scanner/resolve?code={Uri.EscapeDataString(code)}", ct);
    if (result == null)
    {
        return Results.Ok(Store.Models.DTOs.Common.ApiResponse<Store.Models.DTOs.Scanner.ScanResolutionResultDto>.Fail("Resolution failed"));
    }
    return Results.Ok(Store.Models.DTOs.Common.ApiResponse<Store.Models.DTOs.Scanner.ScanResolutionResultDto>.Ok(result));
});

app.MapGet("/api/settings/{*key}", async (HttpContext httpContext, IHttpClientFactory factory, string key, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();
    
    var client = factory.CreateClient("StoreApi");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.GetAsync($"/api/settings/{Uri.EscapeDataString(key)}", ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapPut("/api/settings/{*key}", async (HttpContext httpContext, IHttpClientFactory factory, string key, System.Text.Json.JsonElement request, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token)) return Results.Unauthorized();
    
    var client = factory.CreateClient("StoreApi");
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.PutAsJsonAsync($"/api/settings/{Uri.EscapeDataString(key)}", request, ct);
    var content = await response.Content.ReadAsStringAsync(ct);
    return Results.Content(content, "application/json", System.Text.Encoding.UTF8, (int)response.StatusCode);
});

app.MapPost("/api/session/switch-branch", (HttpContext httpContext, System.Text.Json.JsonElement body) =>
{
    if (body.TryGetProperty("branchId", out var bIdProp) && bIdProp.TryGetInt32(out var branchId) && branchId > 0)
    {
        httpContext.Session.SetInt32("active_branch_id", branchId);
        if (body.TryGetProperty("branchName", out var nameProp))
        {
            httpContext.Session.SetString("active_branch_name", nameProp.GetString() ?? "");
        }
        return Results.Ok(new { success = true, branchId });
    }
    return Results.BadRequest(new { success = false, message = "Invalid branch ID" });
});

app.MapGet("/api/countries", () =>
{
    var countries = new[]
    {
        new Store.Models.DTOs.Common.CountryDto { CountryId = 1, IsoCode = "CM", Name = "Cameroon", PhoneCode = "+237", FlagEmoji = "🇨🇲" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 2, IsoCode = "NG", Name = "Nigeria", PhoneCode = "+234", FlagEmoji = "🇳🇬" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 3, IsoCode = "GH", Name = "Ghana", PhoneCode = "+233", FlagEmoji = "🇬🇭" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 4, IsoCode = "CI", Name = "Côte d'Ivoire", PhoneCode = "+225", FlagEmoji = "🇨🇮" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 5, IsoCode = "SN", Name = "Senegal", PhoneCode = "+221", FlagEmoji = "🇸🇳" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 6, IsoCode = "KE", Name = "Kenya", PhoneCode = "+254", FlagEmoji = "🇰🇪" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 7, IsoCode = "ZA", Name = "South Africa", PhoneCode = "+27", FlagEmoji = "🇿🇦" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 8, IsoCode = "FR", Name = "France", PhoneCode = "+33", FlagEmoji = "🇫🇷" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 9, IsoCode = "GB", Name = "United Kingdom", PhoneCode = "+44", FlagEmoji = "🇬🇧" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 10, IsoCode = "US", Name = "United States", PhoneCode = "+1", FlagEmoji = "🇺🇸" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 11, IsoCode = "CA", Name = "Canada", PhoneCode = "+1", FlagEmoji = "🇨🇦" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 12, IsoCode = "DE", Name = "Germany", PhoneCode = "+49", FlagEmoji = "🇩🇪" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 13, IsoCode = "IT", Name = "Italy", PhoneCode = "+39", FlagEmoji = "🇮🇹" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 14, IsoCode = "NL", Name = "Netherlands", PhoneCode = "+31", FlagEmoji = "🇳🇱" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 15, IsoCode = "AE", Name = "United Arab Emirates", PhoneCode = "+971", FlagEmoji = "🇦🇪" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 16, IsoCode = "CN", Name = "China", PhoneCode = "+86", FlagEmoji = "🇨🇳" },
        new Store.Models.DTOs.Common.CountryDto { CountryId = 17, IsoCode = "IN", Name = "India", PhoneCode = "+91", FlagEmoji = "🇮🇳" }
    };
    return Results.Ok(new { success = true, data = countries });
});

app.Run();
