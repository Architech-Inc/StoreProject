using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class BranchDashboardModel : SecurePageModel
{
    private readonly IBranchManager _branchManager;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<BranchDto> Branches { get; private set; } = Array.Empty<BranchDto>();
    public BranchPerformanceDto? Performance { get; private set; }
    public string? ErrorMessage { get; private set; }

    public int? SelectedBranchId { get; private set; }
    public DateTime FromDate { get; private set; } = DateTime.Today.AddDays(-30);
    public DateTime ToDate { get; private set; } = DateTime.Today;

    public BranchDashboardModel(IBranchManager branchManager, IApiClientService apiClient)
    {
        _branchManager = branchManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int? branchId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.AdminBranches))
            return AccessDenied();

        _apiClient.SetToken(token);

        SelectedBranchId = branchId;
        if (from.HasValue) FromDate = from.Value;
        if (to.HasValue) ToDate = to.Value;

        // Load branch list
        Branches = await _branchManager.GetBranchesAsync(ct);

        if (branchId.HasValue)
        {
            var fromUtc = FromDate.ToUniversalTime();
            var toUtc = ToDate.AddDays(1).AddSeconds(-1).ToUniversalTime();
            try
            {
                Performance = await _branchManager.GetPerformanceAsync(branchId.Value, fromUtc, toUtc, ct);
            }
            catch
            {
                ErrorMessage = "Failed to load branch performance data.";
            }
        }

        return Page();
    }
}
