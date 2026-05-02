using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Store.API.Application.DependencyInjection;
using Store.API.Contracts;
using Store.API.Middleware;
using Store.DbServices.Extensions;
using Store.DbServices.Seeding;

var builder = WebApplication.CreateBuilder(args);

// ─── Database & Domain Services ──────────────────────────────────────────────
builder.Services.AddStoreDbServices(builder.Configuration);
builder.Services.AddArchitecture();

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is required in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!builder.Environment.IsDevelopment())
{
    if (jwtKey.Contains("REPLACE_WITH_A_LONG_RANDOM_SECRET_KEY", StringComparison.OrdinalIgnoreCase)
        || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "Production JWT key is invalid. Configure Jwt:Key with a strong secret (at least 32 characters)."
        );
    }
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
    });

builder.Services.AddAuthorization();

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
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
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
builder.Services.AddHealthChecks();

// ═════════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ═════════════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
    await app.Services.SeedStoreDatabaseAsync();

app.UseMiddleware<CorrelationIdMiddleware>();

// ─── Global Exception Handling (first in pipeline) ───────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Store API v1"));
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseCors("StorePolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
