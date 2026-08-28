using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class UsersModel : SecurePageModel
{
    private readonly IUserManager _userManager;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<UserDto> Users { get; private set; } = Array.Empty<UserDto>();
    public int TotalUsers { get; private set; }
    public int ActiveUsersCount { get; private set; }
    public int SuspendedUsersCount { get; private set; }
    public string? SearchQuery { get; private set; }
    
    public int PendingContactChangesCount { get; private set; }
    
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);

    [BindProperty] public Guid? EditUserId { get; set; }
    [BindProperty] public string NewUsername { get; set; } = string.Empty;
    [BindProperty] public string NewEmail { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public int NewRoleId { get; set; } = 3;
    [BindProperty] public string NewStatus { get; set; } = "Active";
    [BindProperty] public IFormFile? ImageUpload { get; set; }

    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public UsersModel(IUserManager userManager, IApiClientService apiClient)
    {
        _userManager = userManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(string? search = null, int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PageNumber = Math.Max(1, page);
        SearchQuery = search;
        
        var request = new PagedRequest 
        { 
            Page = PageNumber, 
            PageSize = PageSize,
            SearchTerm = search
        };
        var result = await _userManager.GetUsersPagedAsync(request, ct);
        Users = result.Items?.ToList() ?? new List<UserDto>();
        TotalUsers = result.TotalCount;
        ActiveUsersCount = Users.Count(u => u.Status == UserStatus.Active);
        SuspendedUsersCount = Users.Count(u => u.Status == UserStatus.Suspended);
        
        PendingContactChangesCount = await _userManager.GetPendingContactChangesCountAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            if (EditUserId.HasValue && EditUserId.Value != Guid.Empty)
            {
                Enum.TryParse<UserStatus>(NewStatus, out var status);
                var update = new UpdateUserRequest
                {
                    Username = NewUsername,
                    RoleId = NewRoleId,
                    Status = status
                };
                var updated = await _userManager.UpdateUserAsync(EditUserId.Value, update, ImageUpload, CropX, CropY, CropW, CropH, ct);
                StatusMessage = updated is not null ? $"User '{updated.Username}' updated successfully." : "Error: User not found.";
            }
            else
            {
                var create = new CreateUserRequest
                {
                    Username = NewUsername,
                    Email = NewEmail,
                    Password = NewPassword,
                    RoleId = NewRoleId
                };
                var created = await _userManager.CreateUserAsync(create, ImageUpload, CropX, CropY, CropW, CropH, ct);
                StatusMessage = $"User '{created.Username}' created successfully.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSuspendAsync(Guid userId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var updated = await _userManager.SuspendUserAsync(userId, ct);
            StatusMessage = updated is not null ? $"User '{updated.Username}' suspended." : "Error: User not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostIssuePasswordAsync(Guid userId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var tempPwd = await _userManager.IssueTempPasswordAsync(userId, ct);
            StatusMessage = !string.IsNullOrEmpty(tempPwd)
                ? $"Temporary password issued: {tempPwd}"
                : "Error: Failed to issue temporary password.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            await _userManager.RevokeAllSessionsAsync(userId, ct);
            StatusMessage = "All active sessions revoked successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetUserDrawerAsync(Guid id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return Unauthorized();
        _apiClient.SetToken(token);

        try
        {
            var user360 = await _userManager.Get360ByIdAsync(id, ct);
            if (user360 == null) return NotFound();

            return new JsonResult(user360);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
