using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Transfers;
using Store.Models.DTOs.Cash;

namespace StoreUI.Services;

public class BranchManager : IBranchManager
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<BranchManager> _logger;

    public BranchManager(IApiClientService apiClient, ILogger<BranchManager> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<List<BranchDto>> GetBranchesAsync(CancellationToken ct = default)
    {
        return await _apiClient.GetAsync<List<BranchDto>>("/api/admin/branches", ct) ?? new List<BranchDto>();
    }

    public async Task<BranchDto?> UpsertBranchAsync(UpsertBranchRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<BranchDto>("/api/admin/branches", request, ct);
    }

    public async Task<List<UserBranchRoleDto>> GetAssignmentsAsync(int? branchId = null, Guid? userId = null, CancellationToken ct = default)
    {
        var query = string.Empty;
        if (branchId.HasValue && userId.HasValue)
            query = $"?branchId={branchId.Value}&userId={userId.Value}";
        else if (branchId.HasValue)
            query = $"?branchId={branchId.Value}";
        else if (userId.HasValue)
            query = $"?userId={userId.Value}";

        return await _apiClient.GetAsync<List<UserBranchRoleDto>>($"/api/admin/branches/assignments{query}", ct) ?? new List<UserBranchRoleDto>();
    }

    public async Task<UserBranchRoleDto?> AssignUserAsync(AssignUserBranchRoleRequest request, CancellationToken ct = default)
    {
        return await _apiClient.PostAsync<UserBranchRoleDto>("/api/admin/branches/assignments", request, ct);
    }

    public async Task<bool> RevokeAssignmentAsync(long assignmentId, CancellationToken ct = default)
    {
        return await _apiClient.DeleteAsync($"/api/admin/branches/assignments/{assignmentId}", ct);
    }

    public async Task<(bool CanDeactivate, string? Reason)> ValidateDeactivationAsync(int branchId, CancellationToken ct = default)
    {
        try
        {
            // Check for pending/in-transit transfers for this branch
            var transfers = await _apiClient.GetAsync<Store.Models.DTOs.Common.PagedResult<StockTransferDto>>($"/api/transfers?branchId={branchId}&page=1&pageSize=10", ct);
            if (transfers?.Items != null)
            {
                var activeTransfers = transfers.Items.Where(t => t.Status == "Pending" || t.Status == "Dispatched" || t.Status == "InTransit").ToList();
                if (activeTransfers.Any())
                {
                    return (false, $"Cannot deactivate branch: There are {activeTransfers.Count} active or in-transit stock transfer(s) associated with this location.");
                }
            }

            // Check for open cashier shifts for this branch
            var shifts = await _apiClient.GetAsync<List<CashierShiftDto>>($"/api/cash/shifts?branchId={branchId}&page=1&pageSize=10", ct);
            if (shifts != null)
            {
                var openShifts = shifts.Where(s => s.Status.ToString() == "Open" || s.Status.ToString() == "Active").ToList();
                if (openShifts.Any())
                {
                    return (false, $"Cannot deactivate branch: There are {openShifts.Count} open register shift(s) currently running at this location. Please close all shifts first.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Branch deactivation dependency check failed gracefully for branch {BranchId}", branchId);
        }

        return (true, null);
    }
}
