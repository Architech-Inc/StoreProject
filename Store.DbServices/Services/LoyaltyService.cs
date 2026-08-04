using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly IUnitOfWork _uow;

    // Configurable standard tier thresholds
    public const int SilverThreshold = 500;
    public const int GoldThreshold = 2000;
    public const decimal XafPerPoint = 5.0m; // 1 Point = 5 XAF

    public LoyaltyService(IUnitOfWork uow) => _uow = uow;

    public async Task<CustomerLoyaltyAccount?> GetAccountAsync(Guid customerId, CancellationToken ct = default) =>
        await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .Include(a => a.Transactions)
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

    public async Task<CustomerLoyaltyAccount> GetOrCreateAccountAsync(Guid customerId, CancellationToken ct = default)
    {
        var account = await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

        if (account is not null) return account;

        account = new CustomerLoyaltyAccount
        {
            CustomerId = customerId,
            Points = 0,
            Tier = LoyaltyTier.Bronze
        };
        await _uow.Repository<CustomerLoyaltyAccount>().AddAsync(account, ct);
        await _uow.SaveChangesAsync(ct);
        return account;
    }

    public async Task<LoyaltyTransaction> EarnPointsAsync(Guid customerId, int points, Guid? invoiceId, string? note, CancellationToken ct = default)
    {
        if (points <= 0) throw new ArgumentOutOfRangeException(nameof(points), "Points must be positive.");
        var account = await GetOrCreateAccountAsync(customerId, ct);

        // Update current points balance
        account.Points += points;

        // Calculate lifetime qualifying earned points to accurately determine Tier
        var pastEarned = await _uow.Repository<LoyaltyTransaction>()
            .Query()
            .Where(t => t.LoyaltyAccountId == account.LoyaltyAccountId && t.TransactionType == LoyaltyTransactionType.Earn)
            .SumAsync(t => (int?)t.Points, ct) ?? 0;

        var lifetimeQualifyingPoints = pastEarned + points;
        account.Tier = ComputeTier(lifetimeQualifyingPoints);

        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            InvoiceId = invoiceId,
            Points = points,
            TransactionType = LoyaltyTransactionType.Earn,
            Note = string.IsNullOrWhiteSpace(note) ? "Purchase Reward Earned" : note.Trim()
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
            throw new InvalidOperationException($"Insufficient points. Available balance: {account.Points} pts, requested redemption: {points} pts.");

        // Deduct spendable points balance
        account.Points -= points;

        // Tier is PRESERVED on redemption (VIP status is based on lifetime earned points, not spendable balance)
        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            Points = -points,
            TransactionType = LoyaltyTransactionType.Redeem,
            Note = string.IsNullOrWhiteSpace(note) ? $"Redeemed {points} points ({points * XafPerPoint:N0} XAF voucher)" : note.Trim()
        };
        await _uow.Repository<LoyaltyTransaction>().AddAsync(txn, ct);
        await _uow.SaveChangesAsync(ct);
        return txn;
    }

    public async Task<LoyaltyTransaction> AdjustPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default)
    {
        var account = await GetOrCreateAccountAsync(customerId, ct);
        account.Points = Math.Max(0, account.Points + points);

        if (points > 0)
        {
            var pastEarned = await _uow.Repository<LoyaltyTransaction>()
                .Query()
                .Where(t => t.LoyaltyAccountId == account.LoyaltyAccountId && t.TransactionType == LoyaltyTransactionType.Earn)
                .SumAsync(t => (int?)t.Points, ct) ?? 0;

            account.Tier = ComputeTier(pastEarned + points);
        }

        _uow.Repository<CustomerLoyaltyAccount>().Update(account);

        var txn = new LoyaltyTransaction
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            Points = points,
            TransactionType = LoyaltyTransactionType.Adjust,
            Note = string.IsNullOrWhiteSpace(note) ? "Administrative Points Adjustment" : note.Trim()
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

    public async Task<LoyaltyMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var accounts = await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .ToListAsync(ct);

        var totalMembers = accounts.Count;
        var totalPointsLiability = accounts.Sum(a => a.Points);
        var bronzeCount = accounts.Count(a => a.Tier == LoyaltyTier.Bronze);
        var silverCount = accounts.Count(a => a.Tier == LoyaltyTier.Silver);
        var goldCount = accounts.Count(a => a.Tier == LoyaltyTier.Gold);

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthTxns = await _uow.Repository<LoyaltyTransaction>()
            .Query()
            .AsNoTracking()
            .Where(t => t.DateCreated >= startOfMonth)
            .ToListAsync(ct);

        var pointsEarnedMonth = monthTxns.Where(t => t.TransactionType == LoyaltyTransactionType.Earn || (t.TransactionType == LoyaltyTransactionType.Adjust && t.Points > 0)).Sum(t => t.Points);
        var pointsRedeemedMonth = monthTxns.Where(t => t.TransactionType == LoyaltyTransactionType.Redeem).Sum(t => Math.Abs(t.Points));

        var activeMembersCount = accounts.Count(a => a.Points > 0);

        return new LoyaltyMetricsDto
        {
            TotalMembers = totalMembers,
            ActiveMembers = activeMembersCount,
            TotalPointsLiability = totalPointsLiability,
            PointsLiabilityValueXaf = totalPointsLiability * XafPerPoint,
            PointsEarnedThisMonth = pointsEarnedMonth,
            PointsRedeemedThisMonth = pointsRedeemedMonth,
            BronzeCount = bronzeCount,
            SilverCount = silverCount,
            GoldCount = goldCount,
            VipTierRatio = totalMembers > 0 ? Math.Round(((silverCount + goldCount) / (double)totalMembers) * 100, 1) : 0
        };
    }

    public async Task<PagedResult<LoyaltyMemberDto>> GetAllMembersAsync(
        string? search = null,
        string? tier = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .Include(a => a.Customer).ThenInclude(c => c!.Phones).ThenInclude(p => p.Phone)
            .Include(a => a.Customer).ThenInclude(c => c!.Emails).ThenInclude(e => e.Email)
            .Include(a => a.Transactions)
            .AsQueryable();

        // 1. Tier filter
        if (!string.IsNullOrWhiteSpace(tier) && Enum.TryParse<LoyaltyTier>(tier, ignoreCase: true, out var targetTier))
        {
            query = query.Where(a => a.Tier == targetTier);
        }

        // 2. Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            var isGuid = Guid.TryParse(s, out var searchGuid);

            query = query.Where(a =>
                (a.Customer != null && (
                    a.Customer.FirstName.ToLower().Contains(s) ||
                    a.Customer.LastName.ToLower().Contains(s) ||
                    (a.Customer.MiddleName != null && a.Customer.MiddleName.ToLower().Contains(s)) ||
                    a.Customer.Phones.Any(p => p.Phone.Number.ToLower().Contains(s)) ||
                    a.Customer.Emails.Any(e => e.Email.Address.ToLower().Contains(s)) ||
                    (isGuid && a.Customer.CustomerId == searchGuid)
                )) ||
                (isGuid && a.CustomerId == searchGuid));
        }

        // 3. Sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "points_asc" => query.OrderBy(a => a.Points),
            "name_asc" => query.OrderBy(a => a.Customer != null ? a.Customer.LastName : "").ThenBy(a => a.Customer != null ? a.Customer.FirstName : ""),
            "name_desc" => query.OrderByDescending(a => a.Customer != null ? a.Customer.LastName : "").ThenByDescending(a => a.Customer != null ? a.Customer.FirstName : ""),
            "tier_desc" => query.OrderByDescending(a => a.Tier).ThenByDescending(a => a.Points),
            "date_asc" => query.OrderBy(a => a.DateCreated),
            "date_desc" => query.OrderByDescending(a => a.DateCreated),
            _ => query.OrderByDescending(a => a.Points) // Default: points_desc
        };

        var totalItems = await query.CountAsync(ct);

        var pagedAccounts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var memberDtos = pagedAccounts.Select(a =>
        {
            var cust = a.Customer;
            var fullName = cust != null ? $"{cust.FirstName} {cust.LastName}".Trim() : "Unknown Customer";
            var phone = cust?.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
                        ?? cust?.Phones?.FirstOrDefault()?.Phone?.Number;
            var email = cust?.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email?.Address
                        ?? cust?.Emails?.FirstOrDefault()?.Email?.Address;

            var lifetimeEarned = a.Transactions.Where(t => t.TransactionType == LoyaltyTransactionType.Earn).Sum(t => t.Points);
            var totalRedeemed = Math.Abs(a.Transactions.Where(t => t.TransactionType == LoyaltyTransactionType.Redeem).Sum(t => t.Points));
            var lastTxnDate = a.Transactions.OrderByDescending(t => t.DateCreated).FirstOrDefault()?.DateCreated;

            // Tier progression
            var (progressPct, nextThreshold, pointsNeeded) = CalculateTierProgress(a.Points, a.Tier);

            return new LoyaltyMemberDto
            {
                LoyaltyAccountId = a.LoyaltyAccountId,
                CustomerId = a.CustomerId,
                FullName = fullName,
                FirstName = cust?.FirstName,
                LastName = cust?.LastName,
                PrimaryPhone = phone,
                PrimaryEmail = email,
                ThumbnailUrl = cust?.ThumbnailUrl,
                FullImageUrl = cust?.FullImageUrl,
                Segment = cust?.Segment.ToString() ?? "Regular",
                Points = a.Points,
                Tier = a.Tier.ToString(),
                LifetimePointsEarned = lifetimeEarned > 0 ? lifetimeEarned : a.Points,
                TotalPointsRedeemed = totalRedeemed,
                EstimatedMonetaryValue = a.Points * XafPerPoint,
                TierProgressPercentage = progressPct,
                NextTierThreshold = nextThreshold,
                PointsNeededForNextTier = pointsNeeded,
                LastTransactionDate = lastTxnDate,
                DateEnrolled = a.DateCreated
            };
        }).ToList();

        return new PagedResult<LoyaltyMemberDto>(memberDtos, totalItems, page, pageSize);
    }

    public async Task<LoyaltyMemberProfileDto?> GetMemberProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        var account = await _uow.Repository<CustomerLoyaltyAccount>()
            .Query()
            .AsNoTracking()
            .Include(a => a.Customer).ThenInclude(c => c!.Phones).ThenInclude(p => p.Phone)
            .Include(a => a.Customer).ThenInclude(c => c!.Emails).ThenInclude(e => e.Email)
            .Include(a => a.Customer).ThenInclude(c => c!.Invoices)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.CustomerId == customerId, ct);

        if (account is null) return null;

        var cust = account.Customer;
        var fullName = cust != null ? $"{cust.FirstName} {cust.LastName}".Trim() : "Unknown Customer";
        var phone = cust?.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
                    ?? cust?.Phones?.FirstOrDefault()?.Phone?.Number;
        var email = cust?.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email?.Address
                    ?? cust?.Emails?.FirstOrDefault()?.Email?.Address;

        var lifetimeEarned = account.Transactions.Where(t => t.TransactionType == LoyaltyTransactionType.Earn).Sum(t => t.Points);
        var totalRedeemed = Math.Abs(account.Transactions.Where(t => t.TransactionType == LoyaltyTransactionType.Redeem).Sum(t => t.Points));
        var lastTxnDate = account.Transactions.OrderByDescending(t => t.DateCreated).FirstOrDefault()?.DateCreated;
        var (progressPct, nextThreshold, pointsNeeded) = CalculateTierProgress(account.Points, account.Tier);

        var memberDto = new LoyaltyMemberDto
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            CustomerId = account.CustomerId,
            FullName = fullName,
            FirstName = cust?.FirstName,
            LastName = cust?.LastName,
            PrimaryPhone = phone,
            PrimaryEmail = email,
            ThumbnailUrl = cust?.ThumbnailUrl,
            FullImageUrl = cust?.FullImageUrl,
            Segment = cust?.Segment.ToString() ?? "Regular",
            Points = account.Points,
            Tier = account.Tier.ToString(),
            LifetimePointsEarned = lifetimeEarned > 0 ? lifetimeEarned : account.Points,
            TotalPointsRedeemed = totalRedeemed,
            EstimatedMonetaryValue = account.Points * XafPerPoint,
            TierProgressPercentage = progressPct,
            NextTierThreshold = nextThreshold,
            PointsNeededForNextTier = pointsNeeded,
            LastTransactionDate = lastTxnDate,
            DateEnrolled = account.DateCreated
        };

        var txns = account.Transactions
            .OrderByDescending(t => t.DateCreated)
            .Take(30)
            .Select(t => new LoyaltyTransactionDto
            {
                LoyaltyTransactionId = t.LoyaltyTransactionId,
                Points = t.Points,
                TransactionType = t.TransactionType.ToString(),
                InvoiceId = t.InvoiceId,
                Note = t.Note,
                DateCreated = t.DateCreated
            }).ToList();

        // Query active campaigns for customer's segment
        var now = DateTime.UtcNow;
        var seg = cust?.Segment;
        var campaigns = await _uow.Repository<LoyaltyCampaign>()
            .Query()
            .AsNoTracking()
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now &&
                        (c.TargetSegment == null || (seg.HasValue && c.TargetSegment == seg.Value)))
            .ToListAsync(ct);

        var campaignDtos = campaigns.Select(c => new LoyaltyCampaignDto
        {
            LoyaltyCampaignId = c.LoyaltyCampaignId,
            Name = c.Name,
            Description = c.Description,
            CampaignType = c.CampaignType.ToString(),
            TargetSegment = c.TargetSegment?.ToString(),
            MultiplierFactor = c.MultiplierFactor,
            BonusPoints = c.BonusPoints,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            IsRunning = true
        }).ToList();

        var paidInvoices = cust?.Invoices?.Where(i => i.IsPaid).ToList() ?? new List<Invoice>();
        var lifetimeSpend = paidInvoices.Sum(i => i.TotalAmount);
        var totalOrders = cust?.Invoices?.Count ?? 0;

        return new LoyaltyMemberProfileDto
        {
            Member = memberDto,
            RecentTransactions = txns,
            ActiveCampaigns = campaignDtos,
            LifetimeSpend = lifetimeSpend,
            TotalOrders = totalOrders
        };
    }

    public async Task<IEnumerable<GlobalLoyaltyTransactionDto>> GetGlobalTransactionsAsync(
        string? search = null,
        string? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _uow.Repository<LoyaltyTransaction>()
            .Query()
            .AsNoTracking()
            .Include(t => t.LoyaltyAccount).ThenInclude(a => a!.Customer).ThenInclude(c => c!.Phones).ThenInclude(p => p.Phone)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(transactionType) && Enum.TryParse<LoyaltyTransactionType>(transactionType, ignoreCase: true, out var tType))
        {
            query = query.Where(t => t.TransactionType == tType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.DateCreated >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.DateCreated <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t =>
                (t.Note != null && t.Note.ToLower().Contains(s)) ||
                (t.LoyaltyAccount != null && t.LoyaltyAccount.Customer != null && (
                    t.LoyaltyAccount.Customer.FirstName.ToLower().Contains(s) ||
                    t.LoyaltyAccount.Customer.LastName.ToLower().Contains(s) ||
                    t.LoyaltyAccount.Customer.Phones.Any(p => p.Phone.Number.ToLower().Contains(s))
                )));
        }

        var results = await query
            .OrderByDescending(t => t.DateCreated)
            .Take(Math.Min(take, 200))
            .ToListAsync(ct);

        return results.Select(t =>
        {
            var cust = t.LoyaltyAccount?.Customer;
            var custName = cust != null ? $"{cust.FirstName} {cust.LastName}".Trim() : "Unknown Member";
            var phone = cust?.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
                        ?? cust?.Phones?.FirstOrDefault()?.Phone?.Number;

            return new GlobalLoyaltyTransactionDto
            {
                LoyaltyTransactionId = t.LoyaltyTransactionId,
                CustomerId = t.LoyaltyAccount?.CustomerId ?? Guid.Empty,
                CustomerName = custName,
                CustomerPhone = phone,
                Points = t.Points,
                TransactionType = t.TransactionType.ToString(),
                InvoiceId = t.InvoiceId,
                Note = t.Note,
                DateCreated = t.DateCreated
            };
        });
    }

    private static LoyaltyTier ComputeTier(int qualifyingPoints) => qualifyingPoints switch
    {
        >= GoldThreshold => LoyaltyTier.Gold,
        >= SilverThreshold => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };

    private static (int progressPct, int nextThreshold, int pointsNeeded) CalculateTierProgress(int points, LoyaltyTier tier)
    {
        switch (tier)
        {
            case LoyaltyTier.Bronze:
                var bPct = Math.Min(100, (int)Math.Round((points / (double)SilverThreshold) * 100));
                return (bPct, SilverThreshold, Math.Max(0, SilverThreshold - points));

            case LoyaltyTier.Silver:
                var sDiff = points - SilverThreshold;
                var sRange = GoldThreshold - SilverThreshold;
                var sPct = Math.Min(100, Math.Max(0, (int)Math.Round((sDiff / (double)sRange) * 100)));
                return (sPct, GoldThreshold, Math.Max(0, GoldThreshold - points));

            case LoyaltyTier.Gold:
            default:
                return (100, GoldThreshold, 0);
        }
    }
}
