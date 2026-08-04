using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface ILoyaltyService
{
    Task<CustomerLoyaltyAccount?> GetAccountAsync(Guid customerId, CancellationToken ct = default);
    Task<CustomerLoyaltyAccount> GetOrCreateAccountAsync(Guid customerId, CancellationToken ct = default);
    Task<LoyaltyTransaction> EarnPointsAsync(Guid customerId, int points, Guid? invoiceId, string? note, CancellationToken ct = default);
    Task<LoyaltyTransaction> RedeemPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default);
    Task<LoyaltyTransaction> AdjustPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default);
    Task<IEnumerable<LoyaltyTransaction>> GetTransactionsAsync(Guid customerId, int take = 50, CancellationToken ct = default);

    // Modernized Hub Operations
    Task<LoyaltyMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<LoyaltyMemberDto>> GetAllMembersAsync(string? search = null, string? tier = null, string? sortBy = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<LoyaltyMemberProfileDto?> GetMemberProfileAsync(Guid customerId, CancellationToken ct = default);
    Task<IEnumerable<GlobalLoyaltyTransactionDto>> GetGlobalTransactionsAsync(string? search = null, string? transactionType = null, DateTime? fromDate = null, DateTime? toDate = null, int take = 50, CancellationToken ct = default);
}
