using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class UsersModel : SecurePageModel
{
    private readonly IUserService _userService;
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;

    public IReadOnlyList<UserDto> Users { get; private set; } = Array.Empty<UserDto>();
    public int TotalUsers { get; private set; }
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

    public UsersModel(IUserService userService, IApiClientService apiClient, IFileService fileService)
    {
        _userService = userService;
        _apiClient = apiClient;
        _fileService = fileService;
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
        var result = await _userService.GetAllAsync(request, ct);
        Users = result.Items?.ToList() ?? new List<UserDto>();
        TotalUsers = result.TotalCount;
        
        var pendingChanges = await _userService.GetPendingContactChangesAsync(ct);
        PendingContactChangesCount = pendingChanges.Count(p => p.Status == ContactChangeStatus.PendingApproval);

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            string? thumbUrl = null;
            string? fullUrl = null;
            if (ImageUpload != null && ImageUpload.Length > 0)
            {
                if (EditUserId.HasValue && EditUserId.Value != Guid.Empty)
                {
                    var existingUser = await _userService.GetByIdAsync(EditUserId.Value, ct);
                    if (existingUser != null)
                    {
                        if (!string.IsNullOrWhiteSpace(existingUser.ThumbnailUrl))
                            await _fileService.DeleteFileAsync(existingUser.ThumbnailUrl, ct);
                        if (!string.IsNullOrWhiteSpace(existingUser.FullImageUrl))
                            await _fileService.DeleteFileAsync(existingUser.FullImageUrl, ct);
                    }
                }
                using var stream = ImageUpload.OpenReadStream();
                var uploadResult = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "users", CropX, CropY, CropW, CropH, ct);
                thumbUrl = uploadResult.ThumbnailUrl;
                fullUrl = uploadResult.FullImageUrl;
            }

            if (EditUserId.HasValue && EditUserId.Value != Guid.Empty)
            {
                // Edit existing user
                Enum.TryParse<UserStatus>(NewStatus, out var status);
                var update = new UpdateUserRequest
                {
                    Username = NewUsername,
                    RoleId = NewRoleId,
                    Status = status,
                    ThumbnailUrl = thumbUrl,
                    FullImageUrl = fullUrl
                };
                var updated = await _userService.UpdateAsync(EditUserId.Value, update, ct);
                StatusMessage = updated is not null ? $"User '{updated.Username}' updated." : "Error: User not found.";
            }
            else
            {
                // Create new user
                var create = new CreateUserRequest
                {
                    Username = NewUsername,
                    Email = NewEmail,
                    Password = NewPassword,
                    RoleId = NewRoleId,
                    ThumbnailUrl = thumbUrl,
                    FullImageUrl = fullUrl
                };
                var created = await _userService.CreateAsync(create, ct);
                StatusMessage = $"User '{created.Username}' created.";
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
            var update = new UpdateUserRequest { Status = UserStatus.Suspended };
            var updated = await _userService.UpdateAsync(userId, update, ct);
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
            var tempPwd = await _userService.IssueTempPasswordAsync(userId, ct);
            if (!string.IsNullOrEmpty(tempPwd))
            {
                StatusMessage = $"Temporary password issued: {tempPwd}";
            }
            else
            {
                StatusMessage = "Error: Failed to issue temporary password.";
            }
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
            await _userService.RevokeAllSessionsAsync(userId, ct);
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
            var user360 = await _userService.Get360ByIdAsync(id, ct);
            if (user360 == null) return NotFound();

            return new JsonResult(user360);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
