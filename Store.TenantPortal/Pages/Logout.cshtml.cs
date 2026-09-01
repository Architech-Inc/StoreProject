using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

public class LogoutModel : PageModel
{
    private readonly IPortalSessionService _sessionService;

    public LogoutModel(IPortalSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await _sessionService.SignOutAsync(HttpContext);
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _sessionService.SignOutAsync(HttpContext);
        return RedirectToPage("/Index");
    }
}
