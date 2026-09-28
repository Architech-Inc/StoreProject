using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Store.API.Middleware;

/// <summary>
/// GAP-16 — ETag middleware for GET responses (catalog endpoints in particular).
///
/// <para>
/// On the way out, we buffer the response body, compute a SHA-256 of the bytes,
/// emit a weak ETag header (<c>W/"hex"</c>) plus a cache hint, and let the
/// response continue. On the way in, if the request carries
/// <c>If-None-Match</c> matching the just-computed tag, we short-circuit with
/// a 304 Not Modified and never write the body.
/// </para>
///
/// <para>
/// The middleware only kicks in for successful GETs whose response body is
/// fully buffered before send. Streaming responses and POST/PUT/PATCH/DELETE
/// pass through untouched. The cache hint is <c>private, max-age=0</c> — the
/// data may be cached locally but the browser must revalidate every time,
/// which is what we want for a multi-tenant SaaS where item prices change.
/// </para>
/// </summary>
public class ETagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ETagMiddleware> _logger;

    public ETagMiddleware(RequestDelegate next, ILogger<ETagMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only GET responses are eligible. POST/PUT/PATCH/DELETE must NOT be
        // ETagged — that would let an attacker poison caches for writes.
        var method = context.Request.Method;
        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;
        context.Response.Headers["Cache-Control"] = "private, max-age=0, must-revalidate";

        try
        {
            await _next(context);

            if (!context.Response.Headers.ContainsKey("ETag") &&
                context.Response.StatusCode == StatusCodes.Status200OK &&
                buffer.Length > 0)
            {
                buffer.Position = 0;
                using var sha = SHA256.Create();
                var hashBytes = await sha.ComputeHashAsync(buffer);
                var etag = $"W/\"{Convert.ToHexString(hashBytes).ToLowerInvariant()}\"";

                context.Response.Headers["ETag"] = etag;

                var requestEtag = context.Request.Headers.IfNoneMatch.ToString();
                if (!string.IsNullOrEmpty(requestEtag) &&
                    string.Equals(requestEtag.Trim(), etag, System.StringComparison.Ordinal))
                {
                    // Short-circuit to 304 — clear the body and update the status.
                    buffer.SetLength(0);
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.ContentLength = 0;
                    _logger.LogDebug("ETag hit for {Method} {Path}", method, context.Request.Path);
                }
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}

public static class ETagMiddlewareExtensions
{
    /// <summary>Wire the ETag middleware into the pipeline. Place it AFTER response-writing middleware (e.g. exception handling) but BEFORE the controllers.</summary>
    public static IApplicationBuilder UseETag(this IApplicationBuilder app)
        => app.UseMiddleware<ETagMiddleware>();
}
