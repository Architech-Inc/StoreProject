using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface IBranchManager
{
    Task<List<BranchDto>> GetBranchesAsync(CancellationToken ct = default);
    Task<BranchDto?> UpsertBranchAsync(UpsertBranchRequest request, CancellationToken ct = default);
    Task<List<UserBranchRoleDto>> GetAssignmentsAsync(int? branchId = null, Guid? userId = null, CancellationToken ct = default);
    Task<UserBranchRoleDto?> AssignUserAsync(AssignUserBranchRoleRequest request, CancellationToken ct = default);
    Task<bool> RevokeAssignmentAsync(long assignmentId, CancellationToken ct = default);
    Task<BranchPerformanceDto?> GetPerformanceAsync(int branchId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    Task<(bool CanDeactivate, string? Reason)> ValidateDeactivationAsync(int branchId, CancellationToken ct = default);
}
