using Microsoft.AspNetCore.Mvc.Filters;
using Store.Models.DTOs.Audit;
using Store.Models.Interfaces.Services;

namespace Store.API.Attributes;

/// <summary>
/// Apply to controller actions or entire controllers to explicitly mark them as audit-log
/// targets. The <see cref="AuditLoggingMiddleware"/> also auto-classifies a number of
/// security-sensitive paths; this attribute is for non-GET business actions you want to
/// guarantee an audit record for.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class AuditAttribute : Attribute, IAsyncActionFilter
{
    public AuditAttribute(string? summary = null)
    {
        Summary = summary;
    }

    /// <summary>The category stored in <c>AuditLog.Category</c>. Default: "Application".</summary>
    public string Category { get; set; } = "Application";

    /// <summary>The severity stored in <c>AuditLog.Severity</c>. Default: "Info".</summary>
    public string Severity { get; set; } = "Info";

    /// <summary>Optional human-readable summary; falls back to "{METHOD} {PATH}".</summary>
    public string? Summary { get; set; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is not null && !executed.ExceptionHandled) return;

        try
        {
            var sp = context.HttpContext.RequestServices;
            var audit = sp.GetService<IAuditLogService>();
            if (audit is null) return;

            var http = context.HttpContext;
            var userIdClaim = http.User?.FindFirst("uid")?.Value;
            Guid.TryParse(userIdClaim, out var userGuid);

            var action = $"{http.Request.Method} {http.Request.Path}";
            var summary = !string.IsNullOrWhiteSpace(Summary)
                ? Summary
                : action;

            var severity = Severity;
            if (executed.Exception is not null && !executed.ExceptionHandled)
                severity = "Critical";
            else if (http.Response.StatusCode >= 500)
                severity = "Critical";
            else if (http.Response.StatusCode >= 400)
                severity = "Warning";

            var req = new CreateAuditLogEntryRequest
            {
                UserId = userGuid == Guid.Empty ? Guid.Empty : userGuid,
                Action = action,
                Category = Category,
                Severity = severity,
                Summary = summary,
                IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    statusCode = http.Response.StatusCode,
                    userAgent = http.Request.Headers.UserAgent.ToString(),
                    routeValues = context.RouteData.Values
                        .Where(v => v.Key != "action" && v.Key != "controller")
                        .ToDictionary(v => v.Key, v => v.Value)
                })
            };

            await audit.LogAsync(req);
        }
        catch
        {
            // Audit failures must never break the request flow.
        }
    }
}
