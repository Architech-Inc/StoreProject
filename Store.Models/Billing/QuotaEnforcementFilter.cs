using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Store.Models.DTOs.Common;

namespace Store.Models.Billing;

/// <summary>
/// Wave 20 — quota enforcement attribute. Apply to a ControlPlane
/// controller action to gate it on a per-tenant quota.
///
/// The filter reads the tenant id from the route (parameter named
/// <c>id</c> or <c>tenantId</c> or <c>slug</c>), resolves the tenant's
/// plan tier via the configured <see cref="IQuotaTenantResolver"/>, and
/// asks the <see cref="IQuotaGate"/> whether the requested action would
/// exceed the tier's limit. When the answer is no, the request is
/// short-circuited with HTTP 402 Payment Required.
///
/// Usage:
/// <code>
///   [HttpPost("{id:guid}/branches")]
///   [EnforceTenantQuota(TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne)]
///   public async Task&lt;IActionResult&gt; CreateBranch(Guid id, ...) { ... }
/// </code>
///
/// The attribute is library-agnostic so it ships in Store.Models without
/// needing a reference to the ControlPlane assembly. The actual
/// IQuotaTenantResolver implementation lives in ControlPlane.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class EnforceTenantQuotaAttribute : Attribute, IAsyncActionFilter
{
    public TenantQuota Quota { get; }
    public QuotaUsageSource UsageSource { get; }

    public EnforceTenantQuotaAttribute(TenantQuota quota, QuotaUsageSource usageSource = QuotaUsageSource.CurrentPlusOne)
    {
        Quota = quota;
        UsageSource = usageSource;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Only run for actual HTTP requests (skip for action filters that
        // run outside the request pipeline).
        if (context.HttpContext.RequestServices.GetService(typeof(IQuotaEnforcementHandler)) is not IQuotaEnforcementHandler handler)
        {
            // Filter is registered without a backing handler — fail open.
            // This keeps the attribute usable in tests + dev environments
            // without forcing every host to wire the handler.
            await next();
            return;
        }

        var tenantId = ResolveTenantId(context);
        if (tenantId is null)
        {
            // No tenant in the route — likely a system endpoint. Skip.
            await next();
            return;
        }

        var result = await handler.CheckAsync(tenantId.Value, Quota, UsageSource, context.HttpContext.RequestAborted);
        if (!result.Allowed)
        {
            context.Result = new ObjectResult(ApiErrorResponse.From(
                ErrorCode.QuotaExceeded,
                result.Reason,
                traceId: context.HttpContext.TraceIdentifier))
            {
                StatusCode = StatusCodes.Status402PaymentRequired
            };
            return;
        }

        await next();
    }

    private static Guid? ResolveTenantId(ActionExecutingContext context)
    {
        // Try route values in priority order.
        var values = context.RouteData.Values;
        foreach (var key in new[] { "id", "tenantId", "TenantId" })
        {
            if (values.TryGetValue(key, out var v) && Guid.TryParse(v?.ToString(), out var g))
                return g;
        }

        // Last resort — try action arguments.
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is Guid direct) return direct;
            if (arg is string slug && Guid.TryParse(slug, out var sg)) return sg;
        }

        return null;
    }
}

public enum TenantQuota
{
    Branches,
    Users,
    MonthlyInvoices
}

public enum QuotaUsageSource
{
    /// <summary>
    /// Count current usage + 1 (i.e., assume the mutation will succeed and
    /// we're checking whether the post-mutation state would exceed the
    /// tier's limit). Used for create operations.
    /// </summary>
    CurrentPlusOne
}

/// <summary>
/// Wave 20 — handler interface resolved from DI. ControlPlane's
/// implementation knows how to look up the tenant + current usage;
/// Store.API's implementation can be a no-op for shared-DB mode where
/// the tenant context isn't available at the edge.
/// </summary>
public interface IQuotaEnforcementHandler
{
    Task<QuotaCheck> CheckAsync(
        Guid tenantId,
        TenantQuota quota,
        QuotaUsageSource usageSource,
        CancellationToken ct);
}