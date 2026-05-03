using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ProfileModel : SecurePageModel
{
    private readonly IUserService _userService;
    private readonly IApiClientService _apiClient;
    private readonly ILogger<ProfileModel> _logger;

    public string? CurrentUsername { get; private set; }
    public string? CurrentRoleName { get; private set; }
    public string? CurrentStatus { get; private set; }
    public string? StatusMessage { get; private set; }

    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    public ProfileModel(
        IUserService userService,
        IApiClientService apiClient,
        ILogger<ProfileModel> logger)
    {
        _userService = userService;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        // Extract user id from JWT "uid" claim
        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        CurrentUsername = JwtPermissionReader.GetClaim(token, "sub");

        if (Guid.TryParse(userIdStr, out var userId))
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId, ct);
                if (user is not null)
                {
                    CurrentUsername = user.Username;
                    CurrentRoleName = user.RoleName;
                    CurrentStatus = user.Status.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load user profile for {UserId}", userId);
                // Use JWT data as fallback — don't fail page load
                CurrentRoleName = JwtPermissionReader.GetClaim(token, "role");
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (string.IsNullOrWhiteSpace(CurrentPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            StatusMessage = "Error: All password fields are required.";
            return await OnGetAsync(ct);
        }

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Error: New password and confirmation do not match.";
            return await OnGetAsync(ct);
        }

        if (NewPassword.Length < 8)
        {
            StatusMessage = "Error: New password must be at least 8 characters.";
            return await OnGetAsync(ct);
        }

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            StatusMessage = "Error: Could not identify current user. Please log in again.";
            return await OnGetAsync(ct);
        }

        try
        {
            var ok = await _userService.ChangePasswordAsync(userId, new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            }, ct);

            StatusMessage = ok
                ? "Password changed successfully."
                : "Error: Current password is incorrect.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password failed for {UserId}", userId);
            StatusMessage = "Error: Could not change password.";
        }

        // Clear binding values on page reload
        CurrentPassword = null;
        NewPassword = null;
        ConfirmPassword = null;

        return await OnGetAsync(ct);
    }
}
