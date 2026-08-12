using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;

namespace StoreUI.Pages;

public class Verify2FAModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<Verify2FAModel> _logger;

    [TempData]
    public string? TwoFactorToken { get; set; }

    [BindProperty]
    public string? Code { get; set; }

    public string? ErrorMessage { get; set; }

    public Verify2FAModel(IAuthenticationService authService, ILogger<Verify2FAModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(TwoFactorToken))
        {
            return RedirectToPage("/Login");
        }

        TempData.Keep("TwoFactorToken");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(TwoFactorToken))
        {
            return RedirectToPage("/Login");
        }

        if (string.IsNullOrWhiteSpace(Code) || Code.Length != 6)
        {
            ErrorMessage = "Please enter a valid 6-digit code.";
            return Page();
        }

        var request = new Login2FARequest
        {
            TwoFactorToken = TwoFactorToken,
            Code = Code
        };

        var response = await _authService.Login2FAAsync(request);

        if (response == null || string.IsNullOrEmpty(response.AccessToken))
        {
            ErrorMessage = "Invalid verification code or session expired. Please try again.";
            // Since TempData is consumed on read, we must preserve the token if they just entered the wrong code
            TempData.Keep("TwoFactorToken");
            return Page();
        }

        if (response.RequiresPasswordReset)
        {
            HttpContext.Session.SetString("force_reset_userId", response.User.UserId.ToString());
            HttpContext.Session.SetString("force_reset_username", response.User.Username);
            return RedirectToPage("/ForceResetPassword");
        }

        HttpContext.Session.SetString("access_token", response.AccessToken);
        if (!string.IsNullOrEmpty(response.RefreshToken))
            HttpContext.Session.SetString("refresh_token", response.RefreshToken);

        _logger.LogInformation("User logged in successfully via 2FA");
        return RedirectToPage("/Dashboard");
    }
}
