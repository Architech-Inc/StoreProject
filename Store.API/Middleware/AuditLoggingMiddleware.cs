using System.Diagnostics;
using Store.Models.DTOs.Audit;
using Store.Models.Interfaces.Services;

namespace Store.API.Middleware;

/// <summary>
/// Emits an audit entry on every authenticated request that touches a security-relevant
/// endpoint. Endpoints opt in via the <see cref="Attributes.AuditAttribute"/>; everything
/// else is logged at Information level only (cheap, structured).
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    // Endpoints that MUST be recorded in the audit log regardless of attribute use —
    // these are security-relevant by their nature (auth, password, recovery, 2FA, MoMo).
    private static readonly string[] AlwaysAuditedPathPrefixes = new[]
    {
        "/api/auth/login",
        "/api/auth/logout",
        "/api/auth/refresh",
        "/api/auth/recovery",
        "/api/auth/avatar/",
        "/api/webauthn/",
        "/api/payments/momo/callback",
        "/api/payments/orange/callback"
    };

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider sp)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
        }

        var userId = context.User?.FindFirst("uid")?.Value;
        Guid.TryParse(userId, out var userGuid);

        var correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;
        var statusCode = context.Response.StatusCode;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "Audit {Method} {Path} -> {StatusCode} ({ElapsedMs}ms) user={UserId} correlation={CorrelationId}",
            method, path, statusCode, elapsedMs, userId ?? "anonymous", correlationId);

        // Only POST/PUT/PATCH/DELETE that resulted in 2xx or 4xx go to the audit log.
        // We avoid logging GETs unless they're explicitly opted in via [Audit].
        var isMutating = method is "POST" or "PUT" or "PATCH" or "DELETE";
        var isAlwaysAudited = AlwaysAuditedPathPrefixes.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        var isSensitive = isMutating || isAlwaysAudited;

        if (!isSensitive) return;

        try
        {
            using var scope = sp.CreateScope();
            var audit = scope.ServiceProvider.GetService<IAuditLogService>();
            if (audit is null) return;

            var (category, severity, summary) = ClassifyRequest(method, path, statusCode);

            var req = new CreateAuditLogEntryRequest
            {
                UserId = userGuid == Guid.Empty ? Guid.Empty : userGuid,
                Action = $"{method} {path}",
                Category = category,
                Severity = severity,
                Summary = summary,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    statusCode,
                    elapsedMs,
                    correlationId,
                    userAgent = context.Request.Headers.UserAgent.ToString(),
                    query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null
                })
            };

            await audit.LogAsync(req);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit entry for {Method} {Path}", method, path);
        }
    }

    private static (string Category, string Severity, string Summary) ClassifyRequest(string method, string path, int statusCode)
    {
        var p = path.ToLowerInvariant();

        if (p.Contains("/login") || p.Contains("/logout") || p.Contains("/refresh"))
            return ("Authentication", statusCode >= 400 ? "Security" : "Info", $"{method} {path}");

        if (p.Contains("/auth/recovery") || p.Contains("/password"))
            return ("Security", "Security", $"{method} {path}");

        if (p.Contains("/webauthn") || p.Contains("/2fa"))
            return ("Security", "Security", $"{method} {path}");

        if (p.Contains("/momo/callback") || p.Contains("/orange/callback"))
            return ("Payments", "Warning", $"{method} {path}");

        if (p.Contains("/role") || p.Contains("/permission") || p.Contains("/admin"))
            return ("Administration", "Warning", $"{method} {path}");

        if (statusCode >= 500)
            return ("System", "Critical", $"{method} {path} -> {statusCode}");

        if (statusCode >= 400)
            return ("Application", "Warning", $"{method} {path} -> {statusCode}");

        return ("Application", "Info", $"{method} {path}");
    }
}
