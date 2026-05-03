using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly IUnitOfWork _uow;

    // Tier thresholds
    private const int SilverThreshold = 500;
    private const int GoldThreshold = 2000;

    public LoyaltyService(IUnitOfWork uow) => _uow = uow;

    public async Task<CustomerLoyaltyAccount?> GetAccountAsync(Guid customerId, CancellationToken ct = default) =>
        await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

    public async Task<CustomerLoyaltyAccount> GetOrCreateAccountAsync(Guid customerId, CancellationToken ct = default)
    {
        var account = await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

        if (account is not null) return account;

        account = new CustomerLoyaltyAccount { CustomerId = customerId };
        await _uow.Repository<CustomerLoyaltyAccount>().AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);
        return account;
    }

    public async Task<LoyaltyTransaction> EarnPointsAsync(Guid customerId, int points, Guid? invoiceId, string? note, CancellationToken ct = default)
    {
        if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");
        var account = await GetOrCreateAccountAsync(customerId, ct);
        account.Points += points;
        account.Tier = ComputeTier(account.Points);
        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            InvoiceId = invoiceId,
            Points = points,
            TransactionType = LoyaltyTransactionType.Earn,
            Note = note
        };
        await _uow.Repository<LoyaltyTransaction>().AddAsync(txn, ct);
        await _uow.SaveChangesAsync(ct);
        return txn;
    }

    public async Task<LoyaltyTransaction> RedeemPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default)
    {
        if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");
        var account = await GetOrCreateAccountAsync(customerId, ct);
        if (account.Points < points)
            throw new InvalidOperationException($"Insufficient points. Available: {account.Points}, requested: {points}.");

        account.Points -= points;
        account.Tier = ComputeTier(account.Points);
        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            Points = -points,
            TransactionType = LoyaltyTransactionType.Redeem,
            Note = note
        };
        await _uow.Repository<LoyaltyTransaction>().AddAsync(txn, ct);
        await _uow.SaveChangesAsync(ct);
        return txn;
    }

    public async Task<LoyaltyTransaction> AdjustPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default)
    {
        var account = await GetOrCreateAccountAsync(customerId, ct);
        account.Points = Math.Max(0, account.Points + points);
        account.Tier = ComputeTier(account.Points);
        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            Points = points,
            TransactionType = LoyaltyTransactionType.Adjust,
            Note = note
        };
        await _uow.Repository<LoyaltyTransaction>().AddAsync(txn, ct);
        await _uow.SaveChangesAsync(ct);
        return txn;
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetTransactionsAsync(Guid customerId, int take = 50, CancellationToken ct = default)
    {
        var account = await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

        if (account is null) return Enumerable.Empty<LoyaltyTransaction>();

        return await _uow.Repository<LoyaltyTransaction>()
            .Query()
            .AsNoTracking()
            .Where(t => t.LoyaltyAccountId == account.LoyaltyAccountId)
            .OrderByDescending(t => t.DateCreated)
            .Take(take)
            .ToListAsync(ct);
    }

    private static LoyaltyTier ComputeTier(int points) => points switch
    {
        >= GoldThreshold => LoyaltyTier.Gold,
        >= SilverThreshold => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };
}
