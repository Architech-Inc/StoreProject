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
    // Re-issues the sign-in cookie without any tenant claims (e.g. after the store is deleted).
    Task ClearTenantInfoAsync(HttpContext httpContext);
}
