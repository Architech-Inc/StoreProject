namespace Store.Models.Billing;

/// <summary>
/// Wave 19 — quota gate. Resolves the requesting tenant's plan tier,
/// compares the supplied usage counters against
/// <see cref="PlanCatalog.GetLimits"/>, and returns a verdict.
///
/// Endpoints call <see cref="IQuotaGate"/> before mutating state. When the
/// verdict is "blocked" the endpoint returns HTTP 402 Payment Required with
/// the reason — callers get a clear upgrade CTA in the response body.
/// </summary>
public interface IQuotaGate
{
    /// <summary>
    /// Resolve the current tenant from the active HTTP context's JWT
    /// (claim <c>tenant</c>) or the <c>X-Tenant-Id</c> header. Returns
    /// null when no tenant context is present (system callers can skip the
    /// gate — they're not bound to a plan).
    /// </summary>
    TenantTier? GetCurrentTier();

    /// <summary>Branch creation check. <paramref name="currentBranches"/> includes the proposed one.</summary>
    QuotaCheck CheckBranchCreation(int currentBranches);

    /// <summary>User seat check. <paramref name="currentUsers"/> includes the proposed one.</summary>
    QuotaCheck CheckUserSeat(int currentUsers);

    /// <summary>Monthly invoice check. <paramref name="currentMonthInvoices"/> includes the proposed one.</summary>
    QuotaCheck CheckMonthlyInvoice(int currentMonthInvoices);

    /// <summary>Generic check for ad-hoc plan-tier features.</summary>
    QuotaCheck CheckFeature(PlanFeature feature);
}

public record QuotaCheck(
    bool Allowed,
    string Quota,
    int Current,
    int Limit,
    string UpgradeTo,
    string Reason)
{
    public static QuotaCheck Ok(string quota, int current, int limit) =>
        new(true, quota, current, limit, string.Empty, string.Empty);

    public static QuotaCheck Blocked(string quota, int current, int limit, TenantTier upgradeTo, string reason) =>
        new(false, quota, current, limit, upgradeTo.ToString(), reason);
}