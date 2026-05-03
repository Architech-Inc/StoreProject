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

    public IReadOnlyList<UserDto> Users { get; private set; } = Array.Empty<UserDto>();
    public int TotalUsers { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);

    [BindProperty] public Guid? EditUserId { get; set; }
    [BindProperty] public string NewUsername { get; set; } = string.Empty;
    [BindProperty] public string NewEmail { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public int NewRoleId { get; set; } = 3;
    [BindProperty] public string NewStatus { get; set; } = "Active";

    [TempData] public string? StatusMessage { get; set; }

    public UsersModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        PageNumber = Math.Max(1, page);
        var result = await _userService.GetAllAsync(new PagedRequest { Page = PageNumber, PageSize = PageSize }, ct);
        Users = result.Items?.ToList() ?? new List<UserDto>();
        TotalUsers = result.TotalCount;
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        try
        {
            if (EditUserId.HasValue && EditUserId.Value != Guid.Empty)
            {
                // Edit existing user
                Enum.TryParse<UserStatus>(NewStatus, out var status);
                var update = new UpdateUserRequest
                {
                    Username = NewUsername,
                    RoleId = NewRoleId,
                    Status = status
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
                    RoleId = NewRoleId
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
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

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
}
