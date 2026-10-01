using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Store.TenantPortal.Filters;
using Xunit;

namespace Store.TenantPortal.Tests;

public class TenantOwnerOnlyAttributeTests
{
    private static PageHandlerExecutingContext CreateExecutingContext(
        HttpContext httpContext,
        RouteValueDictionary? routeValues = null)
    {
        var routeData = new RouteData();
        if (routeValues != null)
        {
            foreach (var kvp in routeValues)
            {
                routeData.Values[kvp.Key] = kvp.Value;
            }
        }

        var actionContext = new ActionContext(httpContext, routeData, new PageActionDescriptor());
        var pageContext = new PageContext(actionContext);

        var handlerMethod = new HandlerMethodDescriptor
        {
            MethodInfo = typeof(TenantOwnerOnlyAttributeTests).GetMethods()[0],
            Name = "OnGet"
        };

        return new PageHandlerExecutingContext(
            pageContext,
            new List<IFilterMetadata>(),
            handlerMethod,
            new Dictionary<string, object?>(),
            new object());
    }

    [Fact]
    public void OnPageHandlerExecuting_Unauthenticated_RedirectsToLogin()
    {
        var filter = new TenantOwnerOnlyAttribute();
        var httpContext = new DefaultHttpContext(); // unauthenticated user
        var context = CreateExecutingContext(httpContext);

        filter.OnPageHandlerExecuting(context);

        var redirect = Assert.IsType<RedirectToPageResult>(context.Result);
        Assert.Equal("/Login", redirect.PageName);
    }

    [Fact]
    public void OnPageHandlerExecuting_Authenticated_NoRouteRestrictions_AllowsAccess()
    {
        var filter = new TenantOwnerOnlyAttribute();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("TenantId", Guid.NewGuid().ToString())
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = CreateExecutingContext(httpContext);

        filter.OnPageHandlerExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnPageHandlerExecuting_MatchingRouteTenantId_AllowsAccess()
    {
        var filter = new TenantOwnerOnlyAttribute();
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("TenantId", tenantId.ToString())
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var routeValues = new RouteValueDictionary { { "id", tenantId.ToString() } };
        var context = CreateExecutingContext(httpContext, routeValues);

        filter.OnPageHandlerExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnPageHandlerExecuting_MismatchedRouteTenantId_ReturnsForbid()
    {
        var filter = new TenantOwnerOnlyAttribute();
        var sessionTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("TenantId", sessionTenantId.ToString())
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var routeValues = new RouteValueDictionary { { "id", otherTenantId.ToString() } };
        var context = CreateExecutingContext(httpContext, routeValues);

        filter.OnPageHandlerExecuting(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public void OnPageHandlerExecuting_MismatchedQueryTenantId_ReturnsForbid()
    {
        var filter = new TenantOwnerOnlyAttribute();
        var sessionTenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString($"?tenantId={otherTenantId}");
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("TenantId", sessionTenantId.ToString())
        }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);

        var context = CreateExecutingContext(httpContext);

        filter.OnPageHandlerExecuting(context);

        Assert.IsType<ForbidResult>(context.Result);
    }
}
