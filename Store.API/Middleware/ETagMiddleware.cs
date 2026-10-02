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
        // Only GET and HEAD responses are eligible. POST/PUT/PATCH/DELETE must NOT be
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

        try
        {
            await _next(context);

            var cacheControl = context.Response.Headers.CacheControl.ToString();
            bool hasNoStore = cacheControl.IndexOf("no-store", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!hasNoStore &&
                context.Response.StatusCode == StatusCodes.Status200OK &&
                buffer.Length > 0)
            {
                // Set default cache revalidation hint if downstream didn't set Cache-Control
                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                {
                    context.Response.Headers.CacheControl = "private, max-age=0, must-revalidate";
                }

                string etag;
                if (context.Response.Headers.TryGetValue("ETag", out var existingEtag) &&
                    !string.IsNullOrWhiteSpace(existingEtag))
                {
                    etag = existingEtag.ToString();
                }
                else
                {
                    buffer.Position = 0;
                    using var sha = SHA256.Create();
                    var hashBytes = await sha.ComputeHashAsync(buffer);
                    etag = $"W/\"{Convert.ToHexString(hashBytes).ToLowerInvariant()}\"";
                    context.Response.Headers["ETag"] = etag;
                }

                var requestEtag = context.Request.Headers.IfNoneMatch.ToString();
                if (IsIfNoneMatchHit(requestEtag, etag))
                {
                    // Short-circuit to 304 — clear the body and update the status.
                    buffer.SetLength(0);
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.ContentLength = 0;
                    _logger.LogDebug("ETag hit for {Method} {Path}", method, context.Request.Path);
                }
            }
            else if (!context.Response.Headers.ContainsKey("Cache-Control") && !hasNoStore)
            {
                context.Response.Headers.CacheControl = "private, max-age=0, must-revalidate";
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    /// <summary>
    /// RFC 7232 compliant If-None-Match evaluator.
    /// Handles wildcard '*', comma-separated lists of tags, and weak vs strong tags.
    /// </summary>
    public static bool IsIfNoneMatchHit(string? ifNoneMatchHeader, string? currentEtag)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatchHeader) || string.IsNullOrWhiteSpace(currentEtag))
            return false;

        var headerValue = ifNoneMatchHeader.Trim();
        if (headerValue == "*")
            return true;

        var normalizedCurrent = NormalizeEtag(currentEtag);

        var tokens = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var token in tokens)
        {
            if (token == "*")
                return true;

            if (string.Equals(NormalizeEtag(token), normalizedCurrent, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string NormalizeEtag(string tag)
    {
        var trimmed = tag.Trim();
        if (trimmed.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(2).Trim();
        }
        return trimmed.Trim('"');
    }
}

public static class ETagMiddlewareExtensions
{
    /// <summary>Wire the ETag middleware into the pipeline. Place it AFTER response-writing middleware (e.g. exception handling) but BEFORE the controllers.</summary>
    public static IApplicationBuilder UseETag(this IApplicationBuilder app)
        => app.UseMiddleware<ETagMiddleware>();
}
