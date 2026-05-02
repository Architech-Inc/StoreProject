using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Store.API.Middleware;

namespace Store.API.Tests;

public class SecurityMiddlewareTests
{
    [Fact]
    public async Task CorrelationMiddleware_GeneratesCorrelationId_WhenMissing()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var id));
        Assert.False(string.IsNullOrWhiteSpace(id.ToString()));
        Assert.Equal(id.ToString(), context.TraceIdentifier);
    }

    [Fact]
    public async Task CorrelationMiddleware_UsesIncomingCorrelationId_WhenProvided()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = new StringValues("incoming-id-123");

        await middleware.InvokeAsync(context);

        Assert.Equal("incoming-id-123", context.TraceIdentifier);
        Assert.Equal("incoming-id-123", context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_AddsExpectedHeaders()
    {
        var middleware = new SecurityHeadersMiddleware(async ctx =>
        {
            await ctx.Response.WriteAsync("ok");
        });

        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";

        await middleware.InvokeAsync(context);
        await context.Response.StartAsync();

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"].ToString());
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"].ToString());
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"].ToString());
        Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"].ToString());
        Assert.Equal("max-age=31536000; includeSubDomains", context.Response.Headers["Strict-Transport-Security"].ToString());
    }
}
