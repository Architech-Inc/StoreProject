using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Store.API.Application.DependencyInjection;
using Store.Models.DTOs.Common;
using Store.API.Middleware;
using Store.DbServices.Extensions;
using Store.DbServices.Seeding;
using Store.Models.DTOs.Operations;
using Store.API.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Fido2NetLib;
using Microsoft.EntityFrameworkCore;
using Store.API.Hubs;
using Store.DbServices.Context;
using Store.API.Services;
using Store.Models.Interfaces.Services;
using Hangfire;
using Store.API.Extensions;
using Store.Models.Logging;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureEnterpriseLogging("Store.API");

// ─── Database & Domain Services ──────────────────────────────────────────────
builder.Services.AddStoreDbServices(builder.Configuration);
builder.Services.AddArchitecture();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<Store.API.Infrastructure.Processing.IImageProcessorService, Store.API.Infrastructure.Processing.ImageProcessorService>();

// ─── Antivirus (ClamAV sidecar) ───────────────────────────────────────────────
var avProvider = builder.Configuration["Antivirus:Provider"] ?? "NoOp";
if (string.Equals(avProvider, "ClamAV", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<Store.DbServices.Abstractions.IVirusScanner, Store.DbServices.Services.ClamAvVirusScanner>();
}
else
{
    builder.Services.AddScoped<Store.DbServices.Abstractions.IVirusScanner, Store.DbServices.Services.NoOpVirusScanner>();
    if (!builder.Environment.IsDevelopment())
    {
        // Hard warning, not a hard stop — operators may be running a fleet without
        // AV during a phased rollout. CI will block deploys with NoOp in prod.
        Console.Error.WriteLine("WARN: Antivirus provider is NoOp in non-development. Set Antivirus:Provider=ClamAV before production rollout.");
    }
}
builder.Services.AddScoped<Store.Models.Interfaces.Services.IWebAuthnService, Store.API.Services.WebAuthnService>();
builder.Services.AddMemoryCache();

// SEC-26 — server-side password policy. Bound from config with secure defaults
// baked in so a missing config block still produces a hard policy.
builder.Services.Configure<Store.Models.DTOs.Auth.PasswordPolicyOptions>(
    builder.Configuration.GetSection(Store.Models.DTOs.Auth.PasswordPolicyOptions.SectionName));
builder.Services.AddSingleton(sp =>
{
    var opts = builder.Configuration.GetSection(Store.Models.DTOs.Auth.PasswordPolicyOptions.SectionName)
        .Get<Store.Models.DTOs.Auth.PasswordPolicyOptions>() ?? Store.Models.DTOs.Auth.PasswordPolicyOptions.WithSecureDefaults();
    // Ensure the embedded blocklist is always present, even if config doesn't supply one.
    if (opts.CommonPasswords.Count == 0)
    {
        opts.CommonPasswords = Store.Models.DTOs.Auth.PasswordPolicyOptions.DefaultCommonPasswords.ToList();
    }
    return new Store.Models.DTOs.Auth.PasswordPolicyHolder(opts);
});

// SEC-27 — HIBP k-anonymity breach checker. Registered as a typed HttpClient
// so the framework owns its lifecycle + DNS rotation. Fails open on any
// network error — see PwnedPasswordsBreachChecker for the contract.
builder.Services.AddHttpClient<Store.Models.Interfaces.Services.IBreachChecker,
    Store.DbServices.Services.PwnedPasswordsBreachChecker>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(3);
        if (client.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (HIBP-Auditor)");
        }
    });

// MT-02 — PayDunya aggregator is registered in ControlPlane since the
// billing surface (IPN + invoice creation) lives there. Store.API doesn't
// depend on the payment flow.
builder.Services.AddHttpContextAccessor();

builder.Services.AddFido2(options =>
{
    options.ServerDomain = builder.Configuration["Fido2:ServerDomain"] ?? "localhost";
    options.ServerName = "Store FIDO2 Authentication";
    
    // Support a single origin string or multiple comma-separated origins
    var originConfig = builder.Configuration["Fido2:Origin"];
    var origins = new HashSet<string>();
    if (!string.IsNullOrWhiteSpace(originConfig))
    {
        foreach (var o in originConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            origins.Add(o);
    }
    // Include known UI ports as fallbacks only in development
    if (builder.Environment.IsDevelopment())
    {
        origins.Add("https://localhost:7258");
        origins.Add("http://localhost:5135");
    }
    options.Origins = origins;
    options.TimestampDriftTolerance = 300000;
});

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is required in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!builder.Environment.IsDevelopment())
{
    if (jwtKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
        || jwtKey.Contains("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase)
        || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Production JWT key is invalid. Configure Jwt:Key with a strong secret (at least 32 characters)."
        );
    }

    var otpPepper = builder.Configuration["Auth:OtpPepper"];
    if (string.IsNullOrWhiteSpace(otpPepper)
        || otpPepper.Contains("REPLACE", StringComparison.OrdinalIgnoreCase)
        || otpPepper.Contains("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase)
        || Encoding.UTF8.GetByteCount(otpPepper) < 32)
    {
        throw new InvalidOperationException(
            "Production Auth:OtpPepper is invalid. Configure Auth:OtpPepper (env var Auth__OtpPepper) with a strong secret (at least 32 bytes)."
        );
    }
}

// ─── Connection String guard (fail-fast on empty password in non-dev) ──────────
var defaultConn = builder.Configuration.GetConnectionString("Default");
if (!builder.Environment.IsDevelopment() &&
    defaultConn is not null &&
    ContainsEmptyMySqlPassword(defaultConn))
{
    throw new InvalidOperationException(
        "Empty MySQL password detected in production. Set ConnectionStrings__Default with a real password.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = jwtIssuer is not null,
            ValidIssuer = jwtIssuer,
            ValidateAudience = jwtAudience is not null,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                // SEC-19 — accept the access token from the HttpOnly cookie if the
                // Authorization header is missing. The cookie name is owned by
                // Store.API.Auth.AuthCookieHelper; if it changes there, change here.
                if (string.IsNullOrEmpty(context.Token))
                {
                    var cookieToken = context.HttpContext.Request.Cookies
                        .TryGetValue(Store.API.Auth.AuthCookieHelper.AccessTokenCookieName, out var v) ? v : null;
                    if (!string.IsNullOrEmpty(cookieToken))
                    {
                        context.Token = cookieToken;
                    }
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var uow = context.HttpContext.RequestServices.GetRequiredService<Store.Models.Interfaces.IUnitOfWork>();
                var userIdStr = context.Principal?.FindFirst("uid")?.Value;
                var stampStr = context.Principal?.FindFirst("stamp")?.Value;

                if (Guid.TryParse(userIdStr, out var userId) && Guid.TryParse(stampStr, out var stamp))
                {
                    var user = await uow.Repository<Store.Models.Entities.User>().Query()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == userId);

                    if (user == null || user.SecurityStamp != stamp || user.Status != Store.Models.Enums.UserStatus.Active)
                    {
                        context.Fail("Security stamp is invalid or user is not active.");
                    }
                }
                else
                {
                    context.Fail("Token does not contain required claims.");
                }
            }
        };
    });

builder.Services.AddSignalR();
builder.Services.AddScoped<IRealTimeNotificationService, RealTimeNotificationService>();

builder.Services.AddAuthorization(options =>
{
    // ─── Permission policies — auto-registered from PermissionKeys.All. ────────
    // Adding a new permission? Just add the constant to PermissionKeys.All —
    // it gets a policy automatically.
    foreach (var key in PermissionKeys.All)
    {
        options.AddPolicy(key, p => p.RequireClaim("perm", key));
    }
});

// ─── Rate Limiting ────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // Strict limit on auth endpoints to prevent brute force
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // Dedicated strict limit on password recovery endpoints
    options.AddFixedWindowLimiter("password-recovery", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(15);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // SEC-23 — WebAuthn assertion + registration endpoints. Same shape
    // as password-recovery: 5 attempts per 15 minutes per IP. FIDO2
    // itself is computationally infeasible to brute-force, but this
    // caps DoS / replay volume against the assertion callback.
    options.AddFixedWindowLimiter("webauthn-assertion", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(15);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    // General API limit
    options.AddFixedWindowLimiter("general", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 5;
    });

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { success = false, message = "Too many requests. Please try again later." }, ct);
    };
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

if (!builder.Environment.IsDevelopment())
{
    if (allowedOrigins.Length == 0 || allowedOrigins.Any(o => o == "*"))
    {
        throw new InvalidOperationException(
            "Production CORS must define explicit allowed origins and cannot include '*'."
        );
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("StorePolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ─── API Versioning — GAP-15 ─────────────────────────────────────────────────
// Defaults every existing route to v1 without requiring per-controller attributes;
// future v2 endpoints can opt in with `[ApiVersion("2.0")]` + `MapToApiVersion`.
// The `X-Api-Version` response header is emitted on every response, plus
// `?api-version=` query string and `X-Version` header are honored for callers
// that want to opt into a specific version. This is non-breaking: no existing
// route moves, and the wire format is unchanged.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader =
        Microsoft.AspNetCore.Mvc.Versioning.ApiVersionReader.Combine(
            new Microsoft.AspNetCore.Mvc.Versioning.QueryStringApiVersionReader(),
            new Microsoft.AspNetCore.Mvc.Versioning.HeaderApiVersionReader("X-Version"));
});

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(e =>
                string.IsNullOrWhiteSpace(e.ErrorMessage) ? $"Invalid field '{kvp.Key}'." : e.ErrorMessage))
            .ToArray();

        var response = ApiErrorResponse.From(
            "validation_error",
            "Validation failed.",
            errors,
            context.HttpContext.TraceIdentifier);

        return new BadRequestObjectResult(response);
    };
});
builder.Services.AddEndpointsApiExplorer();

// ─── Swagger with JWT support ─────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Store API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT token below.",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// ─── Health Checks ────────────────────────────────────────────────────────────
// Deep readiness probe: the API must be able to actually reach MySQL.
// Without this, the docker-compose healthcheck `curl /health` returns 200
// even when the DB is unreachable, and Traefik routes traffic to a broken
// instance.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StoreDbContext>("mysql", tags: new[] { "ready" });

// ─── Background Jobs (Hangfire) ───────────────────────────────────────────────
builder.Services.AddHangfireServices(builder.Configuration);

// Wave 23.C — demote stale Approved discount overrides to Expired
// after a 15-min TTL so the approval window can't outlive a POS
// session. Polls every 60s; one indexed UPDATE per tick.
builder.Services.AddHostedService<Store.DbServices.Workers.DiscountOverrideExpiryHostedService>();

// ═════════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ═════════════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    try
    {
        await app.Services.SeedStoreDatabaseAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not complete database seeding on startup. Verify MySQL server is running at connection string.");
    }
}

app.UseMiddleware<CorrelationIdMiddleware>();

// ─── Global Exception Handling (first in pipeline) ───────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Store API v1"));
}

// SEC-21 — Forwarded headers for reverse-proxy (Traefik / Docker) TLS termination.
// Ensures Request.IsHttps and Request.Scheme reflect client-facing protocol.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();

var uploadsPath = app.Configuration["FileStorage:BasePath"] ?? "./Uploads";
if (!Path.IsPathRooted(uploadsPath))
{
    uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
}
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/files"
});

app.UseCors("StorePolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// GAP-16 — ETag middleware for GET responses. Placed AFTER auth so the
// response stream is captured post-authz, and BEFORE MapControllers so
// controller handlers can still emit their own Cache-Control headers if
// they need to (the middleware only sets ETag if the controller didn't).
app.UseETag();

app.MapControllers();
app.MapHub<Store.API.Hubs.StoreNotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Configure authorization properly in production
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

app.Services.ScheduleProcurementJobs();
app.Services.SchedulePayrollJobs();

using (var scope = app.Services.CreateScope())
{
    var financeService = scope.ServiceProvider.GetRequiredService<Store.Models.Interfaces.Services.IFinanceService>();
    await financeService.SeedDefaultChartOfAccountsAsync();
}

app.Run();

// ─── Helpers ──────────────────────────────────────────────────────────────────
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
