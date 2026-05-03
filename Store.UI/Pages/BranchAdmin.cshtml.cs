using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class BranchAdminModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;
    private readonly IUserService _userService;

    public IReadOnlyList<BranchDto> Branches { get; private set; } = Array.Empty<BranchDto>();
    public IReadOnlyList<UserBranchRoleDto> Assignments { get; private set; } = Array.Empty<UserBranchRoleDto>();
    public IReadOnlyList<UserDto> AllUsers { get; private set; } = Array.Empty<UserDto>();
    public IReadOnlyList<RoleMatrixDto> RoleMatrix { get; private set; } = Array.Empty<RoleMatrixDto>();

    [TempData] public string? StatusMessage { get; set; }

    // Branch form
    [BindProperty] public int? EditBranchId { get; set; }
    [BindProperty] public string BranchName { get; set; } = string.Empty;
    [BindProperty] public string BranchCode { get; set; } = string.Empty;
    [BindProperty] public string? BranchAddress { get; set; }
    [BindProperty] public bool BranchIsActive { get; set; } = true;

    // Assignment form
    [BindProperty] public Guid AssignUserId { get; set; }
    [BindProperty] public int AssignBranchId { get; set; }
    [BindProperty] public int AssignRoleId { get; set; }

    public BranchAdminModel(IApiClientService apiClient, IUserService userService)
    {
        _apiClient = apiClient;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        await LoadDataAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostUpsertBranchAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        var req = new UpsertBranchRequest
        {
            BranchId = EditBranchId,
            Name = BranchName,
            Code = BranchCode,
            Address = BranchAddress,
            IsActive = BranchIsActive
        };

        var result = await _apiClient.PostAsync<BranchDto>("/api/admin/branches", req, ct);
        StatusMessage = result is not null
            ? $"Branch '{result.Name}' saved."
            : "Failed to save branch.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        var req = new AssignUserBranchRoleRequest
        {
            UserId = AssignUserId,
            BranchId = AssignBranchId,
            RoleId = AssignRoleId
        };

        var result = await _apiClient.PostAsync<UserBranchRoleDto>("/api/admin/branches/assignments", req, ct);
        StatusMessage = result is not null
            ? $"Assigned {result.UserName} to {result.BranchName} as {result.RoleName}."
            : "Failed to assign user.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeAsync(long assignmentId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        var ok = await _apiClient.DeleteAsync($"/api/admin/branches/assignments/{assignmentId}", ct);
        StatusMessage = ok ? "Assignment removed." : "Failed to remove assignment.";

        return RedirectToPage();
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var branchesTask = _apiClient.GetAsync<List<BranchDto>>("/api/admin/branches", ct);
        var assignmentsTask = _apiClient.GetAsync<List<UserBranchRoleDto>>("/api/admin/branches/assignments", ct);
        var usersTask = _userService.GetAllAsync(new Store.Models.DTOs.Common.PagedRequest { Page = 1, PageSize = 500 }, ct);
        var matrixTask = _apiClient.GetAsync<List<RoleMatrixDto>>("/api/admin/role-matrix", ct);

        await Task.WhenAll(branchesTask, assignmentsTask, usersTask, matrixTask);

        Branches = (await branchesTask) ?? new List<BranchDto>();
        Assignments = (await assignmentsTask) ?? new List<UserBranchRoleDto>();
        AllUsers = (await usersTask).Items?.ToList() ?? new List<UserDto>();
        RoleMatrix = (await matrixTask) ?? new List<RoleMatrixDto>();
    }
}
