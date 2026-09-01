using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Extensions.Http;
using Store.TenantPortal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Dashboard");
    options.Conventions.AuthorizeFolder("/Onboarding");
});

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ClexAn_Portal_Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Configure Typed HttpClient for Control Plane with Polly Retry
var cpBaseUrl = builder.Configuration["ControlPlane:BaseUrl"] ?? "http://localhost:9999";
var timeoutSeconds = builder.Configuration.GetValue<int>("ControlPlane:TimeoutSeconds", 30);

builder.Services.AddHttpClient<IControlPlaneClient, ControlPlaneClient>(client =>
{
    client.BaseAddress = new Uri(cpBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt))));

// Register Portal Services
builder.Services.AddScoped<IPortalSessionService, PortalSessionService>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    
    // Content-Security-Policy
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com data:; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';";

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Slug Availability Proxy Endpoint for Onboarding Wizard
app.MapGet("/api/slugs/check", async (string slug, IControlPlaneClient client, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(slug))
    {
        return Results.Ok(new { isAvailable = false, reason = "Slug cannot be empty." });
    }
    var check = await client.CheckSlugAvailabilityAsync(slug, ct);
    return Results.Ok(new { isAvailable = check.IsAvailable, reason = check.Reason, slug = check.Slug });
});

// Backups Proxy Endpoints
app.MapPost("/api/backups/trigger", async (HttpContext ctx, IControlPlaneClient client, IPortalSessionService sessionService, CancellationToken ct) =>
{
    var session = sessionService.GetCurrentSession(ctx.User);
    if (session?.TenantId == null) return Results.Unauthorized();
    try {
        var res = await client.TriggerBackupAsync(session.TenantId.Value, ct);
        return Results.Ok(new { success = true, message = res.Message, data = res });
    } catch (Exception ex) {
        return Results.Ok(new { success = false, message = ex.Message });
    }
}).RequireAuthorization();

app.MapPost("/api/backups/providers/s3", async (HttpContext ctx, [FromBody] Store.TenantPortal.Models.DTOs.ConfigureS3Request req, IControlPlaneClient client, IPortalSessionService sessionService, CancellationToken ct) =>
{
    var session = sessionService.GetCurrentSession(ctx.User);
    if (session?.TenantId == null) return Results.Unauthorized();
    try {
        var res = await client.ConfigureS3ProviderAsync(session.TenantId.Value, req, ct);
        return Results.Ok(new { success = true, message = "S3 Configured successfully.", data = res });
    } catch (Exception ex) {
        return Results.Ok(new { success = false, message = ex.Message });
    }
}).RequireAuthorization();

app.MapDelete("/api/backups/providers/{provider}", async (string provider, HttpContext ctx, IControlPlaneClient client, IPortalSessionService sessionService, CancellationToken ct) =>
{
    var session = sessionService.GetCurrentSession(ctx.User);
    if (session?.TenantId == null) return Results.Unauthorized();
    try {
        var success = await client.DisconnectBackupProviderAsync(session.TenantId.Value, provider, ct);
        return Results.Ok(new { success, message = success ? "Disconnected" : "Failed to disconnect" });
    } catch (Exception ex) {
        return Results.Ok(new { success = false, message = ex.Message });
    }
}).RequireAuthorization();

app.MapPut("/api/backups/schedule", async (HttpContext ctx, [FromBody] Store.TenantPortal.Models.DTOs.UpdateScheduleRequest req, IControlPlaneClient client, IPortalSessionService sessionService, CancellationToken ct) =>
{
    var session = sessionService.GetCurrentSession(ctx.User);
    if (session?.TenantId == null) return Results.Unauthorized();
    try {
        var res = await client.UpdateBackupScheduleAsync(session.TenantId.Value, req, ct);
        return Results.Ok(new { success = true, message = "Schedule updated." });
    } catch (Exception ex) {
        return Results.Ok(new { success = false, message = ex.Message });
    }
}).RequireAuthorization();

app.Run();
