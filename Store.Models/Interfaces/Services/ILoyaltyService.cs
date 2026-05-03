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
}
