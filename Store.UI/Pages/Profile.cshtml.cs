using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ProfileModel : SecurePageModel
{
    private readonly IUserService _userService;
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;
    private readonly ILogger<ProfileModel> _logger;

    public string? CurrentUsername { get; private set; }
    public string? CurrentRoleName { get; private set; }
    public string? CurrentStatus { get; private set; }
    public string? CurrentAvatarPath { get; private set; }
    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }
    [BindProperty] public IFormFile? AvatarUpload { get; set; }

    public ProfileModel(
        IUserService userService,
        IApiClientService apiClient,
        IFileService fileService,
        ILogger<ProfileModel> logger)
    {
        _userService = userService;
        _apiClient = apiClient;
        _fileService = fileService;
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
                    CurrentAvatarPath = user.ImagePath;
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
            TempData["StatusMessage"] = "Error: All password fields are required.";
            return RedirectToPage();
        }

        if (NewPassword != ConfirmPassword)
        {
            TempData["StatusMessage"] = "Error: New password and confirmation do not match.";
            return RedirectToPage();
        }

        if (NewPassword.Length < 8)
        {
            TempData["StatusMessage"] = "Error: New password must be at least 8 characters.";
            return RedirectToPage();
        }

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            TempData["StatusMessage"] = "Error: Could not identify current user. Please log in again.";
            return RedirectToPage();
        }

        try
        {
            var ok = await _userService.ChangePasswordAsync(userId, new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            }, ct);

            TempData["StatusMessage"] = ok
                ? "Password changed successfully."
                : "Error: Current password is incorrect.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password failed for {UserId}", userId);
            TempData["StatusMessage"] = "Error: Could not change password.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        
        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            TempData["StatusMessage"] = "Error: Could not identify current user. Please log in again.";
            return RedirectToPage();
        }

        if (AvatarUpload == null || AvatarUpload.Length == 0)
        {
            TempData["StatusMessage"] = "Error: No file selected.";
            return RedirectToPage();
        }

        try
        {
            string? relativePath = null;
            if (AvatarUpload != null && AvatarUpload.Length > 0)
            {
                // Delete the old avatar before uploading a new one
                var user = await _userService.GetByIdAsync(userId, ct);
                if (user != null && !string.IsNullOrWhiteSpace(user.ImagePath))
                {
                    await _fileService.DeleteFileAsync(user.ImagePath, ct);
                }

                using var stream = AvatarUpload.OpenReadStream();
                relativePath = await _fileService.UploadFileAsync(stream, AvatarUpload.FileName, AvatarUpload.ContentType, "users", ct);
            }
            
            await _userService.UpdateAsync(userId, new UpdateUserRequest
            {
                ImagePath = relativePath
            }, ct);

            TempData["StatusMessage"] = "Avatar updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload avatar for user {UserId}", userIdStr);
            TempData["StatusMessage"] = "Error: Failed to upload avatar.";
        }
        
        return RedirectToPage();
    }
}
