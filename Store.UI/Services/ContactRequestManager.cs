using Store.Models.DTOs.Users;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ContactRequestManager : IContactRequestManager
{
    private readonly IUserService _userService;

    public ContactRequestManager(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IReadOnlyCollection<ContactChangeRequestDto>> GetPendingAsync(CancellationToken ct = default)
    {
        var all = await _userService.GetPendingContactChangesAsync(ct);
        return all.Where(p => p.Status == ContactChangeStatus.PendingApproval || p.Status == ContactChangeStatus.PendingVerification).ToList();
    }

    public async Task<IReadOnlyCollection<ContactChangeRequestDto>> GetHistoryAsync(CancellationToken ct = default)
    {
        return await _userService.GetContactChangeHistoryAsync(ct);
    }

    public async Task<ContactRequestMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var pending = await _userService.GetPendingContactChangesAsync(ct);
        var history = await _userService.GetContactChangeHistoryAsync(ct);

        return new ContactRequestMetricsDto
        {
            TotalPendingCount = pending.Count,
            PendingVerificationCount = pending.Count(p => p.Status == ContactChangeStatus.PendingVerification),
            PendingApprovalCount = pending.Count(p => p.Status == ContactChangeStatus.PendingApproval),
            TotalApprovedCount = history.Count(h => h.Status == ContactChangeStatus.Approved),
            TotalRejectedCount = history.Count(h => h.Status == ContactChangeStatus.Rejected)
        };
    }

    public async Task<bool> ApproveAsync(Guid requestId, Guid adminId, CancellationToken ct = default)
    {
        return await _userService.ApproveContactChangeAsync(requestId, adminId, ct);
    }

    public async Task<bool> RejectAsync(Guid requestId, Guid adminId, CancellationToken ct = default)
    {
        return await _userService.RejectContactChangeAsync(requestId, adminId, ct);
    }
}
