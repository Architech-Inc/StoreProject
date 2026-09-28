using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Store.Models.Billing;
using Store.Models.DTOs.Common;

namespace Store.API.Tests;

/// <summary>
/// Wave 20 — tests for the <see cref="EnforceTenantQuotaAttribute"/> action
/// filter. Covers route/argument tenant-id resolution, fail-open semantics,
/// 402-on-denied, and the "no handler in DI" branch.
/// </summary>
public class QuotaEnforcementFilterTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    private static (ActionExecutingContext context, ServiceCollection services) BuildContext(
        RouteData? routeData = null,
        Dictionary<string, object?>? actionArgs = null)
    {
        var services = new ServiceCollection();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            routeData ?? new RouteData(),
            new ActionDescriptor());

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArgs ?? new Dictionary<string, object?>(),
            controller: new object());

        return (context, services);
    }

    private static async Task<bool> RunFilterAsync(
        EnforceTenantQuotaAttribute filter,
        ActionExecutingContext context,
        Func<ActionExecutedContext> next)
    {
        await filter.OnActionExecutionAsync(context, () => Task.FromResult(next()));
        return context.Result is null; // returns true if filter did not short-circuit
    }

    private static void AssignServices(ActionExecutingContext context, ServiceCollection services)
    {
        context.HttpContext.RequestServices = services.BuildServiceProvider();
    }

    // ---- Fail-open when handler is not registered ----

    [Fact]
    public async Task Fails_open_when_handler_not_registered()
    {
        var (context, services) = BuildContext(routeData: new RouteData(new RouteValueDictionary { ["id"] = TenantId.ToString() }));
        AssignServices(context, services); // empty provider — no handler registered
        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        var nextCalled = false;

        var notShortCircuited = await RunFilterAsync(filter, context, () =>
        {
            nextCalled = true;
            return new ActionExecutedContext(context, context.Filters, context.Controller);
        });

        Assert.True(notShortCircuited);
        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    // ---- Tenant id resolution ----

    [Fact]
    public async Task Resolves_tenant_id_from_route_id()
    {
        var capturedTenantId = Guid.Empty;
        var (context, services) = BuildContext(
            routeData: new RouteData(new RouteValueDictionary { ["id"] = TenantId.ToString() }));
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TenantQuota, QuotaUsageSource, CancellationToken>((id, _, _, _) =>
                capturedTenantId = id)
            .ReturnsAsync(QuotaCheck.Ok("branches", 0, 5));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        await RunFilterAsync(filter, context, () => new ActionExecutedContext(context, context.Filters, context.Controller));

        Assert.Equal(TenantId, capturedTenantId);
    }

    [Fact]
    public async Task Resolves_tenant_id_from_route_tenantId()
    {
        var capturedTenantId = Guid.Empty;
        var (context, services) = BuildContext(
            routeData: new RouteData(new RouteValueDictionary { ["tenantId"] = TenantId.ToString() }));
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TenantQuota, QuotaUsageSource, CancellationToken>((id, _, _, _) =>
                capturedTenantId = id)
            .ReturnsAsync(QuotaCheck.Ok("branches", 0, 5));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        await RunFilterAsync(filter, context, () => new ActionExecutedContext(context, context.Filters, context.Controller));

        Assert.Equal(TenantId, capturedTenantId);
    }

    [Fact]
    public async Task Resolves_tenant_id_from_action_arguments()
    {
        var capturedTenantId = Guid.Empty;
        var (context, services) = BuildContext(actionArgs: new Dictionary<string, object?> { ["id"] = TenantId });
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TenantQuota, QuotaUsageSource, CancellationToken>((id, _, _, _) =>
                capturedTenantId = id)
            .ReturnsAsync(QuotaCheck.Ok("branches", 0, 5));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        await RunFilterAsync(filter, context, () => new ActionExecutedContext(context, context.Filters, context.Controller));

        Assert.Equal(TenantId, capturedTenantId);
    }

    [Fact]
    public async Task Fails_open_when_no_tenant_id_in_route_or_arguments()
    {
        // No id anywhere — likely a system endpoint. Don't block.
        var (context, services) = BuildContext();
        var handler = new Mock<IQuotaEnforcementHandler>(MockBehavior.Strict);
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        var nextCalled = false;
        var notShortCircuited = await RunFilterAsync(filter, context, () =>
        {
            nextCalled = true;
            return new ActionExecutedContext(context, context.Filters, context.Controller);
        });

        Assert.True(notShortCircuited);
        Assert.True(nextCalled);
        handler.VerifyNoOtherCalls();
    }

    // ---- Allow / block behavior ----

    [Fact]
    public async Task Calls_next_when_handler_allows()
    {
        var (context, services) = BuildContext(
            routeData: new RouteData(new RouteValueDictionary { ["id"] = TenantId.ToString() }));
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QuotaCheck.Ok("branches", 0, 5));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        var nextCalled = false;
        var notShortCircuited = await RunFilterAsync(filter, context, () =>
        {
            nextCalled = true;
            return new ActionExecutedContext(context, context.Filters, context.Controller);
        });

        Assert.True(notShortCircuited);
        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Returns_402_with_QuotaExceeded_when_handler_blocks()
    {
        var (context, services) = BuildContext(
            routeData: new RouteData(new RouteValueDictionary { ["id"] = TenantId.ToString() }));
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QuotaCheck.Blocked("branches", 2, 1, TenantTier.Professional,
                "Starter plan allows up to 1 branch. Upgrade to Professional."));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Branches);
        var nextCalled = false;
        var notShortCircuited = await RunFilterAsync(filter, context, () =>
        {
            nextCalled = true;
            return new ActionExecutedContext(context, context.Filters, context.Controller);
        });

        Assert.False(notShortCircuited);
        Assert.False(nextCalled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status402PaymentRequired, result.StatusCode);
        var payload = Assert.IsType<ApiErrorResponse>(result.Value);
        Assert.Equal(ErrorCode.QuotaExceeded, payload.Code);
        Assert.Contains("Upgrade", payload.Message);
        Assert.Equal(context.HttpContext.TraceIdentifier, payload.TraceId);
    }

    [Fact]
    public async Task Passes_quota_and_usageSource_to_handler()
    {
        TenantQuota capturedQuota = TenantQuota.Branches;
        QuotaUsageSource capturedSource = (QuotaUsageSource)999;
        var (context, services) = BuildContext(
            routeData: new RouteData(new RouteValueDictionary { ["id"] = TenantId.ToString() }));
        var handler = new Mock<IQuotaEnforcementHandler>();
        handler.Setup(h => h.CheckAsync(It.IsAny<Guid>(), It.IsAny<TenantQuota>(),
                It.IsAny<QuotaUsageSource>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TenantQuota, QuotaUsageSource, CancellationToken>((_, q, s, _) =>
            {
                capturedQuota = q;
                capturedSource = s;
            })
            .ReturnsAsync(QuotaCheck.Ok("users", 0, 1));
        services.AddSingleton(handler.Object);
        AssignServices(context, services);

        var filter = new EnforceTenantQuotaAttribute(TenantQuota.Users, QuotaUsageSource.CurrentPlusOne);
        await RunFilterAsync(filter, context, () => new ActionExecutedContext(context, context.Filters, context.Controller));

        Assert.Equal(TenantQuota.Users, capturedQuota);
        Assert.Equal(QuotaUsageSource.CurrentPlusOne, capturedSource);
    }
}
