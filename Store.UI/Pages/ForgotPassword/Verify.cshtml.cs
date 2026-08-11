using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreUI.Services;

namespace StoreUI.Pages.ForgotPassword;

public class VerifyModel : PageModel
{
    private readonly IApiPasswordRecoveryService _recoveryService;

    public VerifyModel(IApiPasswordRecoveryService recoveryService)
    {
        _recoveryService = recoveryService;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string OtpCode { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet(string username)
    {
        Username = username;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(OtpCode) || string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter a valid 6-digit OTP.";
            return Page();
        }

        var token = await _recoveryService.VerifyOtpAsync(Username, OtpCode, ct);
        
        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "Invalid or expired OTP.";
            return Page();
        }

        // Successfully verified, redirect to ResetPassword with the token
        return RedirectToPage("/ResetPassword", new { token = token });
    }
}
