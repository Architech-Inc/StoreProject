using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.ControlPlane.Workers;

var builder = WebApplication.CreateBuilder(args);

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
var connectionString = builder.Configuration.GetConnectionString("ControlPlane") 
    ?? "Server=localhost;Port=3306;Database=store_controlplane;User Id=root;Password=;AllowPublicKeyRetrieval=True;";

builder.Services.AddSingleton<ISecretEncryptionService, SecretEncryptionService>();

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
builder.Services.AddHostedService<TenantHealthMonitorWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// Run database migration and ensure schema exists
await ControlPlaneDataMigrator.MigrateAsync(app.Services, app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Control Plane API v1"));
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();
