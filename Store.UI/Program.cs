using Store.Models.Interfaces.Services;
using StoreUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "storeui-antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "storeui-session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
app.UseSession();

app.UseRouting();

app.MapRazorPages();

app.Run();
