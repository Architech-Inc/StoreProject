using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreUI.Services;
using System.ComponentModel.DataAnnotations;

namespace StoreUI.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly IApiPasswordRecoveryService _recoveryService;

    public ResetPasswordModel(IApiPasswordRecoveryService recoveryService)
    {
        _recoveryService = recoveryService;
    }

    [BindProperty]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public bool Success { get; set; }

    public void OnGet(string token)
    {
        Token = token;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Token))
        {
            ErrorMessage = "Please check your password inputs.";
            return Page();
        }

        var success = await _recoveryService.ResetPasswordAsync(Token, NewPassword, ConfirmPassword, ct);
        
        if (!success)
        {
            ErrorMessage = "Invalid or expired reset token.";
            return Page();
        }

        Success = true;
        return Page();
    }
}
