using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly IApiPasswordRecoveryService _recoveryService;

    public ForgotPasswordModel(IApiPasswordRecoveryService recoveryService)
    {
        _recoveryService = recoveryService;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter a valid username.";
            return Page();
        }

        var success = await _recoveryService.RequestOtpAsync(Username, ct);
        
        // Regardless of success/failure, redirect to verification to prevent user enumeration
        return RedirectToPage("/ForgotPassword/Verify", new { username = Username });
    }
}
