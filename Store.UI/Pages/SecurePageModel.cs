using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreUI.Services;

namespace StoreUI.Pages;

public abstract class SecurePageModel : PageModel
{
    protected bool TryGetSecurityContext(out string token, out HashSet<string> permissions)
    {
        token = HttpContext.Session.GetString("access_token") ?? string.Empty;
        permissions = JwtPermissionReader.GetPermissions(token);
        return !string.IsNullOrWhiteSpace(token);
    }

    protected bool HasPermission(HashSet<string> permissions, string permissionKey)
    {
        return permissions.Contains(permissionKey);
    }

    protected RedirectToPageResult GoToLogin() => RedirectToPage("Login");
}
