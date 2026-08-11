using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace StoreUI.Pages;

public class ForceResetPasswordModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<ForceResetPasswordModel> _logger;

    public ForceResetPasswordModel(IAuthenticationService authService, ILogger<ForceResetPasswordModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [BindProperty]
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public bool Success { get; set; }

    public IActionResult OnGet()
    {
        var username = HttpContext.Session.GetString("force_reset_username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToPage("/Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please check your password inputs.";
            return Page();
        }

        var username = HttpContext.Session.GetString("force_reset_username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToPage("/Login");
        }

        var request = new ResetPasswordRequest 
        { 
            Username = username,
            CurrentPassword = CurrentPassword,
            NewPassword = NewPassword
        };

        var success = await _authService.ResetPasswordAsync(request, ct);
        
        if (!success)
        {
            ErrorMessage = "Failed to update password. Ensure your temporary password is correct.";
            return Page();
        }

        // Clear session state
        HttpContext.Session.Remove("force_reset_userId");
        HttpContext.Session.Remove("force_reset_username");

        Success = true;
        return Page();
    }
}
