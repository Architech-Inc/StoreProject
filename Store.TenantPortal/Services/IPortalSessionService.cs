using System.Security.Claims;
using Store.TenantPortal.Models;
using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Services;

public interface IPortalSessionService
{
    Task SignInAsync(HttpContext httpContext, PortalAuthDto authData, bool isPersistent = true);
    Task SignOutAsync(HttpContext httpContext);
    PortalSession? GetCurrentSession(ClaimsPrincipal user);
    Task UpdateTenantInfoAsync(HttpContext httpContext, Guid tenantId, string tenantSlug, string tenantName);
}
