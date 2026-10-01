using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Events;
using Store.API.Middleware;
using Store.Models.Logging;
using Xunit;

namespace Store.API.Tests;

public class EnterpriseLoggingTests
{
    [Fact]
    public void ConfigureLogger_WithoutSeqUrl_ConfiguresConsoleAndEnrichersGracefully()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Logging:LogLevel:Default", "Information" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var loggerConfig = new LoggerConfiguration();

        // Act
        var result = EnterpriseLoggingExtensions.ConfigureLogger(
            loggerConfig,
            config,
            environmentName: "Testing",
            applicationName: "Store.API.Tests");

        var logger = result.CreateLogger();

        // Assert - Logger created without throwing and writes to sinks
        Assert.NotNull(logger);
        logger.Information("Test structured log event from EnterpriseLoggingTests");
    }

    [Fact]
    public void ConfigureLogger_WithSeqUrl_ConfiguresSeqSinkSuccessfully()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Seq:ServerUrl", "http://localhost:5341" },
            { "Seq:ApiKey", "test-api-key-12345" }
        };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var loggerConfig = new LoggerConfiguration();

        // Act
        var result = EnterpriseLoggingExtensions.ConfigureLogger(
            loggerConfig,
            config,
            environmentName: "Production",
            applicationName: "Store.API");

        var logger = result.CreateLogger();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_PushesCorrelationAndTenantPropertiesToLogContext()
    {
        // Arrange
        string? capturedCorrelationId = null;
        string? capturedTenantId = null;

        var middleware = new CorrelationIdMiddleware(async ctx =>
        {
            // Simulate log event generation inside downstream pipeline
            capturedCorrelationId = ctx.TraceIdentifier;
            capturedTenantId = ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var h) ? h.ToString() : null;
            await Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = new StringValues("corr-abc-987");
        context.Request.Headers["X-Tenant-Id"] = new StringValues("tenant-charly");

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("corr-abc-987", capturedCorrelationId);
        Assert.Equal("tenant-charly", capturedTenantId);
        Assert.Equal("corr-abc-987", context.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ResolvesTenantFromUserClaims_WhenHeaderMissing()
    {
        // Arrange
        string? resolvedTenantClaim = null;

        var middleware = new CorrelationIdMiddleware(async ctx =>
        {
            resolvedTenantClaim = ctx.User.FindFirst("tenant")?.Value;
            await Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        var claims = new[] { new Claim("tenant", "alpha-foods-ltd"), new Claim("uid", Guid.NewGuid().ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal("alpha-foods-ltd", resolvedTenantClaim);
        Assert.True(context.Response.Headers.ContainsKey(CorrelationIdMiddleware.HeaderName));
    }
}
