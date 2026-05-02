using System.Diagnostics;

namespace Store.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var userId = context.User?.FindFirst("uid")?.Value ?? "anonymous";
            var correlationId = context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString();

            _logger.LogInformation(
                "Audit {Method} {Path} -> {StatusCode} ({ElapsedMs}ms) user={UserId} correlation={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                correlationId
            );
        }
    }
}
