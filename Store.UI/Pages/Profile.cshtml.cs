using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ProfileModel : SecurePageModel
{
    private readonly IUserService _userService;
    private readonly IEmployeeService _employeeService;
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;
    private readonly ILogger<ProfileModel> _logger;

    public string? CurrentUsername { get; private set; }
    public string? CurrentRoleName { get; private set; }
    public string? CurrentStatus { get; private set; }
    public string? CurrentAvatarPath { get; private set; }
    public string? CurrentFullAvatarPath { get; private set; }
    
    // Employee Identity Data
    public string? EmployeeFullName { get; private set; }
    public string? EmployeeDepartment { get; private set; }
    public string? EmployeeGender { get; private set; }
    public string? EmployeeDateEmployed { get; private set; }

    public bool TwoFactorEnabled { get; private set; }
    public IReadOnlyCollection<AuditLogDto> RecentActivity { get; private set; } = Array.Empty<AuditLogDto>();

    // 2FA Setup state
    [TempData] public string? TwoFactorSharedKey { get; set; }
    [TempData] public string? TwoFactorAuthenticatorUri { get; set; }

    // Contact Data
    [BindProperty] public string? PrimaryEmail { get; set; }
    [BindProperty] public string? PrimaryPhone { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public string? CurrentPassword { get; set; }
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }
    [BindProperty] public IFormFile? AvatarUpload { get; set; }

    public ProfileModel(
        IUserService userService,
        IEmployeeService employeeService,
        IApiClientService apiClient,
        IFileService fileService,
        ILogger<ProfileModel> logger)
    {
        _userService = userService;
        _employeeService = employeeService;
        _apiClient = apiClient;
        _fileService = fileService;
        _logger = logger;
    }

    [BindProperty]
    public int? CropX { get; set; }
    
    [BindProperty]
    public int? CropY { get; set; }
    
    [BindProperty]
    public int? CropW { get; set; }
    
    [BindProperty]
    public int? CropH { get; set; }

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
                    CurrentAvatarPath = user.ThumbnailUrl ?? user.FullImageUrl;
                    CurrentFullAvatarPath = user.FullImageUrl ?? user.ThumbnailUrl;
                    PrimaryEmail = user.PrimaryEmail;
                    PrimaryPhone = user.PrimaryPhone;

                    // Fetch associated Employee data if available
                    if (user.EmployeeId.HasValue)
                    {
                        try
                        {
                            var emp = await _employeeService.GetByIdAsync(user.EmployeeId.Value, ct);
                            if (emp != null)
                            {
                                EmployeeFullName = emp.FullName;
                                EmployeeDepartment = emp.DepartmentName ?? "Unassigned";
                                EmployeeGender = emp.Gender.ToString();
                                EmployeeDateEmployed = emp.DateEmployed.ToString("MMM dd, yyyy");
                            }
                        }
                        catch (Exception empEx)
                        {
                            _logger.LogWarning(empEx, "Failed to load employee details for User {UserId}", userId);
                        }
                    }

                    TwoFactorEnabled = user.TwoFactorEnabled;
                    RecentActivity = await _userService.GetRecentActivityAsync(userId, ct);
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
            string? thumbUrl = null;
            string? fullUrl = null;
            if (AvatarUpload != null && AvatarUpload.Length > 0)
            {
                // Delete the old avatar before uploading a new one
                var user = await _userService.GetByIdAsync(userId, ct);
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(user.ThumbnailUrl))
                        await _fileService.DeleteFileAsync(user.ThumbnailUrl, ct);
                    if (!string.IsNullOrWhiteSpace(user.FullImageUrl))
                        await _fileService.DeleteFileAsync(user.FullImageUrl, ct);
                }

                using var stream = AvatarUpload.OpenReadStream();
                var uploadResult = await _fileService.UploadFileAsync(stream, AvatarUpload.FileName, AvatarUpload.ContentType, "users", CropX, CropY, CropW, CropH, ct);
                thumbUrl = uploadResult.ThumbnailUrl;
                fullUrl = uploadResult.FullImageUrl;
            }
            
            await _userService.UpdateAvatarAsync(thumbUrl, fullUrl, ct);

            TempData["StatusMessage"] = "Avatar updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload avatar for user {UserId}", userIdStr);
            TempData["StatusMessage"] = "Error: Failed to upload avatar.";
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateContactsAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            TempData["StatusMessage"] = "Error: Could not identify current user. Please log in again.";
            return RedirectToPage();
        }

        try
        {
            var request = new UpdateUserContactsRequest
            {
                Email = PrimaryEmail,
                Phone = PrimaryPhone
            };

            var success = await _userService.UpdateContactsAsync(userId, request, ct);

            TempData["StatusMessage"] = success
                ? "Contacts updated successfully."
                : "Error: Failed to update contacts.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update contacts for user {UserId}", userIdStr);
            TempData["StatusMessage"] = "Error: Failed to update contacts.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEnable2FAAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToPage();

        try
        {
            var response = await _userService.Enable2FAAsync(userId, ct);
            TwoFactorSharedKey = response.SharedKey;
            TwoFactorAuthenticatorUri = response.AuthenticatorUri;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate 2FA for user {UserId}", userId);
            TempData["StatusMessage"] = "Error: Failed to setup 2FA.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVerify2FAAsync([FromForm] string VerificationCode, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        var userIdStr = JwtPermissionReader.GetClaim(token, "uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return RedirectToPage();

        if (string.IsNullOrWhiteSpace(VerificationCode) || VerificationCode.Length != 6)
        {
            TempData["StatusMessage"] = "Error: Invalid verification code.";
            return RedirectToPage();
        }

        try
        {
            var success = await _userService.Verify2FAAsync(userId, new Verify2FARequest { Code = VerificationCode }, ct);
            if (success)
            {
                TempData["StatusMessage"] = "Two-factor authentication successfully enabled.";
                TwoFactorSharedKey = null;
                TwoFactorAuthenticatorUri = null;
            }
            else
            {
                TempData["StatusMessage"] = "Error: Invalid verification code.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify 2FA for user {UserId}", userId);
            TempData["StatusMessage"] = "Error: Failed to verify 2FA code.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeSessionsAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var success = await _userService.RevokeAllSessionsAsync(ct);
            if (success)
            {
                TempData["StatusMessage"] = "All sessions revoked. Please log in again.";
                return RedirectToPage("/Logout");
            }
            
            TempData["StatusMessage"] = "Error: Failed to revoke sessions.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke sessions");
            TempData["StatusMessage"] = "Error: Failed to revoke sessions.";
        }

        return RedirectToPage();
    }
}
