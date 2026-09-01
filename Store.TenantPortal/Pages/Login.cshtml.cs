using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.ViewModels;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

public class LoginModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public LoginModel(IControlPlaneClient cpClient, IPortalSessionService sessionService)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    }

    [BindProperty]
    public LoginVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var session = _sessionService.GetCurrentSession(User);
            if (session?.HasTenant == true)
            {
                return RedirectToPage("/Dashboard");
            }
            return RedirectToPage("/Onboarding");
        }

        Input.ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authResult = await _cpClient.LoginAsync(Input.Email, Input.Password, ct);
        if (authResult == null)
        {
            ErrorMessage = "Invalid email address or password.";
            return Page();
        }

        await _sessionService.SignInAsync(HttpContext, authResult);

        if (!string.IsNullOrEmpty(Input.ReturnUrl) && Url.IsLocalUrl(Input.ReturnUrl))
        {
            return Redirect(Input.ReturnUrl);
        }

        if (authResult.TenantId.HasValue && !string.IsNullOrEmpty(authResult.TenantSlug))
        {
            return RedirectToPage("/Dashboard");
        }

        return RedirectToPage("/Onboarding");
    }
}
