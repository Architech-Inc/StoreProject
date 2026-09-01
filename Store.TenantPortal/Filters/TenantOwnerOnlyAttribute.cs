using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Store.TenantPortal.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TenantOwnerOnlyAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new RedirectToPageResult("/Login");
            return;
        }

        var sessionTenantId = user.FindFirst("TenantId")?.Value;

        // If the route or query explicitly specified a tenant ID parameter, verify it matches
        if (context.RouteData.Values.TryGetValue("id", out var routeTenantId) && routeTenantId != null)
        {
            if (!string.Equals(sessionTenantId, routeTenantId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        if (context.HttpContext.Request.Query.TryGetValue("tenantId", out var queryTenantId) && !string.IsNullOrEmpty(queryTenantId))
        {
            if (!string.Equals(sessionTenantId, queryTenantId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}
