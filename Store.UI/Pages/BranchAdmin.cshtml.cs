using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
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
    private readonly IEmployeeService _employeeService;

    public IReadOnlyList<BranchDto> Branches { get; private set; } = Array.Empty<BranchDto>();
    public IReadOnlyList<UserBranchRoleDto> Assignments { get; private set; } = Array.Empty<UserBranchRoleDto>();
    public IReadOnlyList<UserDto> AllUsers { get; private set; } = Array.Empty<UserDto>();
    public IReadOnlyList<EmployeeDto> AllEmployees { get; private set; } = Array.Empty<EmployeeDto>();
    public IReadOnlyList<RoleMatrixDto> RoleMatrix { get; private set; } = Array.Empty<RoleMatrixDto>();
    public IReadOnlyList<PersonnelTransferDto> Transfers { get; private set; } = Array.Empty<PersonnelTransferDto>();
    public IReadOnlyList<BranchItemStockDto> BranchStocks { get; private set; } = Array.Empty<BranchItemStockDto>();

    // ─── KPI Metrics ──────────────────────────────────────────────────────────
    public int TotalBranches => Branches.Count;
    public int ActiveBranchesCount => Branches.Count(b => b.IsActive);
    public int InactiveBranchesCount => Branches.Count(b => !b.IsActive);
    public int TotalAssignmentsCount => Assignments.Count;
    public int MultiBranchUsersCount => Assignments
        .GroupBy(a => a.UserId)
        .Count(g => g.Select(x => x.BranchId).Distinct().Count() > 1);
    public int TotalTransfersCount => Transfers.Count;

    [TempData] public string? StatusMessage { get; set; }

    // Branch form
    [BindProperty] public int? EditBranchId { get; set; }
    [BindProperty] public string BranchName { get; set; } = string.Empty;
    [BindProperty] public string BranchCode { get; set; } = string.Empty;
    [BindProperty] public string? BranchAddress { get; set; }
    [BindProperty] public bool BranchIsActive { get; set; } = true;
    [BindProperty] public decimal BranchPriceMultiplier { get; set; } = 1.0m;
    [BindProperty] public decimal? BranchTaxRateOverride { get; set; }

    // Assignment form
    [BindProperty] public Guid AssignUserId { get; set; }
    [BindProperty] public int AssignBranchId { get; set; }
    [BindProperty] public int AssignRoleId { get; set; }
    [BindProperty] public DateTime? AssignValidFrom { get; set; }
    [BindProperty] public DateTime? AssignValidTo { get; set; }
    [BindProperty] public string? AssignGrantReason { get; set; }

    // Transfer form
    [BindProperty] public Guid TransferEmployeeId { get; set; }
    [BindProperty] public int TransferToBranchId { get; set; }
    [BindProperty] public int TransferType { get; set; } = 0;
    [BindProperty] public DateTime TransferEffectiveDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime? TransferExpectedEndDate { get; set; }
    [BindProperty] public string? TransferReason { get; set; }
    [BindProperty] public int? TransferDestinationRoleId { get; set; }

    // Stock override form
    [BindProperty] public int StockBranchId { get; set; }
    [BindProperty] public Guid StockItemId { get; set; }
    [BindProperty] public int? StockDelta { get; set; }
    [BindProperty] public int? StockReorderLevel { get; set; }
    [BindProperty] public decimal? StockCustomPrice { get; set; }

    public BranchAdminModel(
        IBranchManager branchManager,
        IApiClientService apiClient,
        IUserService userService,
        IEmployeeService employeeService)
    {
        _branchManager = branchManager;
        _apiClient = apiClient;
        _userService = userService;
        _employeeService = employeeService;
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
            IsActive = BranchIsActive,
            PriceMultiplier = BranchPriceMultiplier > 0 ? BranchPriceMultiplier : 1.0m,
            TaxRateOverride = BranchTaxRateOverride
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
            RoleId = AssignRoleId,
            ValidFrom = AssignValidFrom,
            ValidTo = AssignValidTo,
            GrantReason = AssignGrantReason
        };

        var result = await _branchManager.AssignUserAsync(req, ct);
        StatusMessage = result is not null
            ? $"Assigned {result.UserName} to {result.BranchName} as {result.RoleName}."
            : "Error: Failed to assign user.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTransferAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        var req = new TransferEmployeeRequest
        {
            EmployeeId = TransferEmployeeId,
            ToBranchId = TransferToBranchId,
            DestinationRoleId = TransferDestinationRoleId,
            TransferType = TransferType,
            EffectiveDate = TransferEffectiveDate,
            ExpectedEndDate = TransferExpectedEndDate,
            Reason = TransferReason
        };

        var result = await _branchManager.TransferPersonnelAsync(req, ct);
        StatusMessage = result is not null
            ? $"Successfully transferred {result.EmployeeName} to {result.ToBranchName} ({result.TransferType})."
            : "Error: Failed to process personnel transfer.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStockAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!HasPermission(permissions, PermissionKeys.AdminBranches) && !HasPermission(permissions, PermissionKeys.InventoryWrite))
            return AccessDenied();

        var req = new UpdateBranchStockRequest
        {
            BranchId = StockBranchId,
            ItemId = StockItemId,
            InStockDelta = StockDelta,
            ReorderLevel = StockReorderLevel,
            CustomUnitPrice = StockCustomPrice
        };

        var result = await _branchManager.UpdateBranchStockAsync(StockBranchId, req, ct);
        StatusMessage = result is not null
            ? $"Updated branch stock for '{result.ItemName}' at {result.BranchName}."
            : "Error: Failed to update branch stock.";

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
        var employeesTask = _employeeService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 500 }, ct);
        var matrixTask = _apiClient.GetAsync<List<RoleMatrixDto>>("/api/admin/role-matrix", ct);
        var transfersTask = _branchManager.GetTransfersAsync(null, null, ct);

        await Task.WhenAll(branchesTask, assignmentsTask, usersTask, employeesTask, matrixTask, transfersTask);

        Branches = (await branchesTask) ?? new List<BranchDto>();
        Assignments = (await assignmentsTask) ?? new List<UserBranchRoleDto>();
        AllUsers = (await usersTask).Items?.ToList() ?? new List<UserDto>();
        AllEmployees = (await employeesTask).Items?.ToList() ?? new List<EmployeeDto>();
        RoleMatrix = (await matrixTask) ?? new List<RoleMatrixDto>();
        Transfers = (await transfersTask) ?? new List<PersonnelTransferDto>();

        if (Branches.Count > 0)
        {
            BranchStocks = await _branchManager.GetBranchStockAsync(Branches[0].BranchId, ct);
        }
    }
}
