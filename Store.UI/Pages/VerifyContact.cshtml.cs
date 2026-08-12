using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.Interfaces.Services;

namespace StoreUI.Pages;

public class VerifyContactModel : PageModel
{
    private readonly IUserService _userService;

    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    public VerifyContactModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token))
        {
            IsSuccess = false;
            ErrorMessage = "Verification token is missing or invalid.";
            return Page();
        }

        try
        {
            var success = await _userService.VerifyContactChangeAsync(token, ct);
            IsSuccess = success;
            if (!success)
            {
                ErrorMessage = "Verification failed. The link may be expired or already used.";
            }
        }
        catch (Exception)
        {
            IsSuccess = false;
            ErrorMessage = "An error occurred while verifying the contact change.";
        }

        return Page();
    }
}
