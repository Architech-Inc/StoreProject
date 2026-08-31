using Store.Models.DTOs.Users;

namespace StoreUI.Services;

public class ContactRequestMetricsDto
{
    public int TotalPendingCount { get; set; }
    public int PendingVerificationCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int TotalApprovedCount { get; set; }
    public int TotalRejectedCount { get; set; }
}

public interface IContactRequestManager
{
    Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingAsync(CancellationToken ct = default);
    Task<IReadOnlyCollection<ContactChangeRequestDto>> GetHistoryAsync(CancellationToken ct = default);
    Task<ContactRequestMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<bool> ApproveAsync(Guid requestId, Guid adminId, CancellationToken ct = default);
    Task<bool> RejectAsync(Guid requestId, Guid adminId, CancellationToken ct = default);
}
