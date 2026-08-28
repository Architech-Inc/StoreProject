using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class BranchAdminModel : SecurePageModel
{
    private readonly IBranchManager _branchManager;
    private readonly IApiClientService _apiClient;
    private readonly IUserService _userService;

    public IReadOnlyList<BranchDto> Branches { get; private set; } = Array.Empty<BranchDto>();
    public IReadOnlyList<UserBranchRoleDto> Assignments { get; private set; } = Array.Empty<UserBranchRoleDto>();
    public IReadOnlyList<UserDto> AllUsers { get; private set; } = Array.Empty<UserDto>();
    public IReadOnlyList<RoleMatrixDto> RoleMatrix { get; private set; } = Array.Empty<RoleMatrixDto>();

    // ─── KPI Metrics ──────────────────────────────────────────────────────────
    public int TotalBranches => Branches.Count;
    public int ActiveBranchesCount => Branches.Count(b => b.IsActive);
    public int InactiveBranchesCount => Branches.Count(b => !b.IsActive);
    public int TotalAssignmentsCount => Assignments.Count;
    public int MultiBranchUsersCount => Assignments
        .GroupBy(a => a.UserId)
        .Count(g => g.Select(x => x.BranchId).Distinct().Count() > 1);

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

    public BranchAdminModel(
        IBranchManager branchManager,
        IApiClientService apiClient,
        IUserService userService)
    {
        _branchManager = branchManager;
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

    public async Task<IActionResult> OnGetSearchUsersAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return Forbid();

        var result = await _userService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 15, SearchTerm = q?.Trim() }, ct);
        var users = result.Items.Select(u => new
        {
            id = u.UserId.ToString(),
            title = u.Username,
            sub = $"Role: {u.RoleName ?? "Standard"} | Status: {u.Status}",
            badge = u.RoleName ?? "User"
        });

        return new JsonResult(users);
    }

    public async Task<IActionResult> OnGetSearchBranchesAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return Forbid();

        var branches = await _branchManager.GetBranchesAsync(ct);
        var query = q?.Trim().ToLowerInvariant();
        var results = branches
            .Where(b => string.IsNullOrEmpty(query) ||
                        b.Name.ToLowerInvariant().Contains(query) ||
                        b.Code.ToLowerInvariant().Contains(query))
            .Select(b => new
            {
                id = b.BranchId.ToString(),
                title = $"{b.Name} ({b.Code})",
                sub = b.Address ?? "No address specified",
                badge = b.IsActive ? "Active" : "Inactive"
            });

        return new JsonResult(results);
    }

    public async Task<IActionResult> OnPostUpsertBranchAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        // ── Deactivation Guardrail Check ──
        if (EditBranchId.HasValue && !BranchIsActive)
        {
            var (canDeactivate, reason) = await _branchManager.ValidateDeactivationAsync(EditBranchId.Value, ct);
            if (!canDeactivate)
            {
                StatusMessage = $"Error: {reason}";
                return RedirectToPage();
            }
        }

        var req = new UpsertBranchRequest
        {
            BranchId = EditBranchId,
            Name = BranchName,
            Code = BranchCode,
            Address = BranchAddress,
            IsActive = BranchIsActive
        };

        var result = await _branchManager.UpsertBranchAsync(req, ct);
        StatusMessage = result is not null
            ? $"Branch '{result.Name}' saved successfully."
            : "Error: Failed to save branch.";

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

        var result = await _branchManager.AssignUserAsync(req, ct);
        StatusMessage = result is not null
            ? $"Assigned {result.UserName} to {result.BranchName} as {result.RoleName}."
            : "Error: Failed to assign user.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRevokeAsync(long assignmentId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        var ok = await _branchManager.RevokeAssignmentAsync(assignmentId, ct);
        StatusMessage = ok ? "Branch assignment revoked." : "Error: Failed to remove assignment.";

        return RedirectToPage();
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var branchesTask = _branchManager.GetBranchesAsync(ct);
        var assignmentsTask = _branchManager.GetAssignmentsAsync(null, null, ct);
        var usersTask = _userService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 500 }, ct);
        var matrixTask = _apiClient.GetAsync<List<RoleMatrixDto>>("/api/admin/role-matrix", ct);

        await Task.WhenAll(branchesTask, assignmentsTask, usersTask, matrixTask);

        Branches = (await branchesTask) ?? new List<BranchDto>();
        Assignments = (await assignmentsTask) ?? new List<UserBranchRoleDto>();
        AllUsers = (await usersTask).Items?.ToList() ?? new List<UserDto>();
        RoleMatrix = (await matrixTask) ?? new List<RoleMatrixDto>();
    }
}
