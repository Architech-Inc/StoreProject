using Store.Models.Interfaces.Services;
using StoreUI.Services;

var builder = WebApplication.CreateBuilder(args);

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
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7112";
builder.Services.AddHttpClient("StoreApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var targetBase = app.Configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:7112";

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

app.MapGet("/api/scanner/resolve", async (HttpContext httpContext, IApiClientService apiClient, string code, CancellationToken ct) =>
{
    var token = httpContext.Session.GetString("access_token");
    if (string.IsNullOrWhiteSpace(token))
    {
        return Results.Unauthorized();
    }
    apiClient.SetToken(token);
    var result = await apiClient.GetAsync<Store.Models.DTOs.Common.ApiResponse<Store.Models.DTOs.Scanner.ScanResolutionResultDto>>(
        $"/api/scanner/resolve?code={Uri.EscapeDataString(code)}", ct);
    if (result == null)
    {
        return Results.Ok(Store.Models.DTOs.Common.ApiResponse<Store.Models.DTOs.Scanner.ScanResolutionResultDto>.Fail("Resolution failed"));
    }
    return Results.Ok(result);
});

app.Run();
