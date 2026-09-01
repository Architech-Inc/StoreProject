using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Store.TenantPortal.Models;
using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Services;

public class PortalSessionService : IPortalSessionService
{
    public async Task SignInAsync(HttpContext httpContext, PortalAuthDto authData, bool isPersistent = true)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authData.AccountId.ToString()),
            new(ClaimTypes.Email, authData.Email),
            new(ClaimTypes.Name, authData.FullName),
            new("SessionToken", authData.SessionToken),
            new("IssuedAt", DateTime.UtcNow.ToString("O")),
            new("ExpiresAt", authData.ExpiresAt.ToString("O"))
        };

        if (authData.TenantId.HasValue)
        {
            claims.Add(new Claim("TenantId", authData.TenantId.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(authData.TenantSlug))
        {
            claims.Add(new Claim("TenantSlug", authData.TenantSlug));
        }

        if (!string.IsNullOrEmpty(authData.TenantName))
        {
            claims.Add(new Claim("TenantName", authData.TenantName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = authData.ExpiresAt,
            AllowRefresh = true
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
    }

    public async Task SignOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public PortalSession? GetCurrentSession(ClaimsPrincipal user)
    {
        if (user.Identity == null || !user.Identity.IsAuthenticated)
        {
            return null;
        }

        var accIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(accIdClaim, out var accId))
        {
            return null;
        }

        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var name = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        var token = user.FindFirst("SessionToken")?.Value ?? string.Empty;

        Guid? tenantId = null;
        var tenantIdClaim = user.FindFirst("TenantId")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tid))
        {
            tenantId = tid;
        }

        var slug = user.FindFirst("TenantSlug")?.Value;
        var tName = user.FindFirst("TenantName")?.Value;

        return new PortalSession
        {
            AccountId = accId,
            Email = email,
            FullName = name,
            TenantId = tenantId,
            TenantSlug = slug,
            TenantName = tName,
            SessionToken = token
        };
    }

    public async Task UpdateTenantInfoAsync(HttpContext httpContext, Guid tenantId, string tenantSlug, string tenantName)
    {
        var session = GetCurrentSession(httpContext.User);
        if (session == null) return;

        var authDto = new PortalAuthDto(
            session.AccountId,
            session.Email,
            session.FullName,
            tenantId,
            tenantSlug,
            tenantName,
            session.SessionToken,
            DateTime.UtcNow.AddHours(8)
        );

        await SignInAsync(httpContext, authDto);
    }
}
