using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.ViewModels;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

public class RegisterModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public RegisterModel(IControlPlaneClient cpClient, IPortalSessionService sessionService)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    }

    [BindProperty]
    public RegisterVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Onboarding");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var authResult = await _cpClient.RegisterAccountAsync(Input.Email, Input.FullName, Input.Password, ct);
            await _sessionService.SignInAsync(HttpContext, authResult);

            return RedirectToPage("/Onboarding");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (Exception)
        {
            ErrorMessage = "An unexpected error occurred during registration. Please try again.";
            return Page();
        }
    }
}
