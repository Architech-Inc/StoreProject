using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.ControlPlane.Workers;
using Store.Models.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEnterpriseLogging("Store.ControlPlane");

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ClexAn Foods SaaS Control Plane API",
        Version = "v1",
        Description = "Multi-Tenant Container Silo Provisioning & Orchestration Engine"
    });
});

// Configure Enterprise Rate Limiting Policies
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Portal Auth Rate Limit: 10 requests per 15 minutes per IP
    options.AddPolicy("PortalAuth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            }));

    // Backup Trigger Rate Limit: 5 requests per 10 minutes per IP
    options.AddPolicy("BackupTrigger", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_client",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0
            }));
});

// Database & Security
// The ControlPlane connection string MUST come from configuration (env var
// `ConnectionStrings__ControlPlane`). We refuse to start with an empty password
// or with the legacy fallback value — both are security holes.
var connectionString = builder.Configuration.GetConnectionString("ControlPlane")
    ?? throw new InvalidOperationException(
        "ConnectionStrings__ControlPlane is required. " +
        "Set the env var ConnectionStrings__ControlPlane (no default/fallback).");

if (ContainsEmptyMySqlPassword(connectionString) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Empty MySQL password detected in production. Set a real password in " +
        "ConnectionStrings__ControlPlane.");
}

builder.Services.AddSingleton<ISecretEncryptionService, SecretEncryptionService>();

// Fail-fast on missing/placeholder master encryption key BEFORE we build the host.
// We do this directly off the configuration rather than via BuildServiceProvider()
// (which would create a second service container).
var masterKey = builder.Configuration["ControlPlane:MasterEncryptionKey"];
if (string.IsNullOrWhiteSpace(masterKey) ||
    masterKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase) ||
    masterKey.Contains("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase) ||
    masterKey.Contains("MasterSecretKey2026", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "ControlPlane:MasterEncryptionKey is missing or set to a placeholder. " +
        "Set it via env var ControlPlane__MasterEncryptionKey (>=32 chars, no placeholder).");
}
if (masterKey.Length < 32)
{
    throw new InvalidOperationException(
        "ControlPlane:MasterEncryptionKey must be at least 32 characters. " +
        "Generate with: openssl rand -base64 48");
}

builder.Services.AddDbContextFactory<ControlPlaneDbContext>((sp, options) =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
    });
});

builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ControlPlaneDbContext>>().CreateDbContext());

// Register Control Plane Dependencies
builder.Services.AddSingleton<ITenantRepository, MySqlTenantRepository>();
builder.Services.AddSingleton<IPortalAuthService, PortalAuthService>();
builder.Services.AddSingleton<IAuditService, AuditService>();
builder.Services.AddSingleton<IDomainVerificationService, DomainVerificationService>();
builder.Services.AddSingleton<ITraefikConfigWriter, TraefikConfigWriter>();
builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddScoped<ITenantOrchestrator, TenantOrchestrator>();
builder.Services.AddScoped<ITenantSmtpService, TenantSmtpService>();
// Wave 18 — payment reconciliation. Wired into the IPN handler so a
// successful PayDunya webhook upgrades the tenant's plan tier.
builder.Services.AddScoped<Store.ControlPlane.Services.SubscriptionReconciler>();
builder.Services.AddHostedService<TenantHealthMonitorWorker>();
// MT-01 — async provisioning queue processor
builder.Services.AddHostedService<TenantProvisioningHostedService>();

// MT-06 — per-tenant backup scheduler. Polls every 30s; evaluates each
// tenant's BackupScheduleConfig and fires TriggerBackupNowAsync when due.
builder.Services.AddHostedService<TenantBackupHostedService>();

// Wave 20 — quota enforcement handler. The EnforceTenantQuota attribute
// resolves IQuotaEnforcementHandler from DI; without this registration the
// filter fails open (kept that way so tests + dev hosts can run unconfigured).
builder.Services.AddSingleton<Store.Models.Billing.IQuotaEnforcementHandler,
    Store.ControlPlane.Services.ControlPlaneQuotaHandler>();

// Wave 17 / Wave 18 — PayDunya aggregator + options. Typed HttpClient so
// DNS rotation + connection pooling are owned by the framework.
builder.Services.Configure<Store.DbServices.Services.PayDunyaOptions>(
    builder.Configuration.GetSection(Store.DbServices.Services.PayDunyaOptions.SectionName));
builder.Services.AddHttpClient<Store.Models.Interfaces.Services.IPayDunyaPaymentService,
    Store.DbServices.Services.PayDunyaPaymentService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    if (client.DefaultRequestHeaders.UserAgent.Count == 0)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (PayDunya-Aggregator)");
    }
});

// CORS — explicit allowlist only, env-supplied. We refuse AllowAnyOrigin.
var controlPlaneOrigins = builder.Configuration.GetSection("ControlPlaneCors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

if (!builder.Environment.IsDevelopment() &&
    (controlPlaneOrigins.Length == 0 || controlPlaneOrigins.Any(o => o == "*")))
{
    throw new InvalidOperationException(
        "ControlPlaneCors:AllowedOrigins must be explicitly configured for production (no '*' allowed).");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ControlPlanePolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(controlPlaneOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Run database migration and ensure schema exists
await ControlPlaneDataMigrator.MigrateAsync(app.Services, app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Control Plane API v1"));
}

app.UseCors("ControlPlanePolicy");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "store-controlplane" }))
   .AllowAnonymous();

app.Run();

// Helper for fail-fast on empty-password MySQL in non-dev environments.
static bool ContainsEmptyMySqlPassword(string cs)
{
    foreach (var part in cs.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        var kv = part.Split('=', 2);
        if (kv.Length == 2 &&
            kv[0].Trim().Equals("Password", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(kv[1]))
        {
            return true;
        }
    }
    return false;
}
