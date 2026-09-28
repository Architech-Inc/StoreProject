using Store.Models.Billing;

namespace Store.DbServices.Services;

/// <summary>
/// Wave 19 — default <see cref="IQuotaGate"/> implementation.
///
/// Pure-function: it doesn't touch the database or HttpContext. The
/// active tier is supplied by the caller (the middleware or endpoint
/// resolves the tenant from JWT/header and looks up its tier). This
/// keeps the gate testable without HttpContext mocking and lets it run
/// inside batch jobs too.
///
/// The "blocked" verdict tells the caller which tier to upgrade to.
/// We pick the next higher tier — Starter → Professional → Enterprise.
/// Already on Enterprise with unlimited quota = always allowed.
/// </summary>
public class PlanQuotaGate : IQuotaGate
{
    private readonly Func<TenantTier?> _tierResolver;
    private TenantTier? _cachedTier;

    public PlanQuotaGate(Func<TenantTier?> tierResolver)
    {
        _tierResolver = tierResolver;
    }

    /// <summary>
    /// Allow constructor for callers that resolve tier on first use and
    /// cache it (e.g., from a request-scoped service). The active tier
    /// resolves lazily on first call.
    /// </summary>
    public PlanQuotaGate()
        : this(() => TenantTier.Starter) // safe default — gate always permits
    {
    }

    public TenantTier? GetCurrentTier()
    {
        if (_cachedTier is null)
        {
            _cachedTier = _tierResolver();
        }
        return _cachedTier;
    }

    public QuotaCheck CheckBranchCreation(int currentBranches)
    {
        var tier = GetCurrentTier() ?? TenantTier.Starter;
        var limits = PlanCatalog.GetLimits(tier);
        if (limits.MaxBranches == int.MaxValue)
        {
            return QuotaCheck.Ok("branches", currentBranches, int.MaxValue);
        }
        if (currentBranches > limits.MaxBranches)
        {
            return QuotaCheck.Blocked(
                quota: "branches",
                current: currentBranches,
                limit: limits.MaxBranches,
                upgradeTo: NextTierAbove(tier),
                reason: $"Your {tier} plan allows up to {limits.MaxBranches} branches. Upgrade to create more.");
        }
        return QuotaCheck.Ok("branches", currentBranches, limits.MaxBranches);
    }

    public QuotaCheck CheckUserSeat(int currentUsers)
    {
        var tier = GetCurrentTier() ?? TenantTier.Starter;
        var limits = PlanCatalog.GetLimits(tier);
        if (limits.MaxUsers == int.MaxValue)
        {
            return QuotaCheck.Ok("users", currentUsers, int.MaxValue);
        }
        if (currentUsers > limits.MaxUsers)
        {
            return QuotaCheck.Blocked(
                quota: "users",
                current: currentUsers,
                limit: limits.MaxUsers,
                upgradeTo: NextTierAbove(tier),
                reason: $"Your {tier} plan includes {limits.MaxUsers} user seats. Upgrade to add more.");
        }
        return QuotaCheck.Ok("users", currentUsers, limits.MaxUsers);
    }

    public QuotaCheck CheckMonthlyInvoice(int currentMonthInvoices)
    {
        var tier = GetCurrentTier() ?? TenantTier.Starter;
        var limits = PlanCatalog.GetLimits(tier);
        if (limits.MonthlyInvoices == int.MaxValue)
        {
            return QuotaCheck.Ok("invoices", currentMonthInvoices, int.MaxValue);
        }
        if (currentMonthInvoices > limits.MonthlyInvoices)
        {
            return QuotaCheck.Blocked(
                quota: "invoices",
                current: currentMonthInvoices,
                limit: limits.MonthlyInvoices,
                upgradeTo: NextTierAbove(tier),
                reason: $"Your {tier} plan caps invoices at {limits.MonthlyInvoices} per month. Upgrade for more headroom.");
        }
        return QuotaCheck.Ok("invoices", currentMonthInvoices, limits.MonthlyInvoices);
    }

    public QuotaCheck CheckFeature(PlanFeature feature)
    {
        var tier = GetCurrentTier() ?? TenantTier.Starter;
        if (PlanCatalog.IsFeatureEnabled(tier, feature))
        {
            return QuotaCheck.Ok(feature.ToString(), 1, 1);
        }
        return QuotaCheck.Blocked(
            quota: feature.ToString(),
            current: 0,
            limit: 0,
            upgradeTo: NextTierAbove(tier),
            reason: $"Your {tier} plan doesn't include {feature}. Upgrade to enable it.");
    }

    private static TenantTier NextTierAbove(TenantTier current) => current switch
    {
        TenantTier.Starter => TenantTier.Professional,
        TenantTier.Professional => TenantTier.Enterprise,
        _ => TenantTier.Enterprise
    };
}