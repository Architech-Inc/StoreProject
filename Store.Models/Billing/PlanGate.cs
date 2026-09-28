using Store.Models.Billing;

namespace Store.Models.Billing;

/// <summary>
/// MT-02 — feature flags each plan tier includes.
///
/// New features added to the catalog must be wired through the
/// <see cref="PlanCatalog"/> mapping below so every tier's affordance list
/// is explicit. Adding a feature here forces every implementor to decide
/// which tiers get it — no silent feature creep.
/// </summary>
public enum PlanFeature
{
    /// <summary>Single-branch operation.</summary>
    SingleBranch,
    /// <summary>Up to N branches with per-branch dashboards and stock transfers.</summary>
    MultiBranch,
    /// <summary>Scheduled automatic backups (Wave 14 / MT-06).</summary>
    AutomatedBackups,
    /// <summary>Per-tenant SMTP credentials (MT-04 — pending).</summary>
    CustomSmtp,
    /// <summary>Sandbox / preview environments (MT-03 family).</summary>
    SandboxEnvironments,
    /// <summary>Advanced reporting (PromotionEffectiveness, etc.).</summary>
    AdvancedReports,
    /// <summary>External API access (machine-readable JSON endpoints).</summary>
    ExternalApiAccess,
    /// <summary>Phone / chat support tier.</summary>
    PrioritySupport
}

/// <summary>
/// MT-02 — static catalog mapping <see cref="TenantTier"/> to the set of
/// <see cref="PlanFeature"/> values each tier unlocks.
///
/// When you add a new feature to <see cref="PlanFeature"/>, you must
/// decide which tier(s) get it here — every option is explicit so no
/// tier gets or loses a feature by accident.
/// </summary>
public static class PlanCatalog
{
    private static readonly Dictionary<TenantTier, HashSet<PlanFeature>> Map = new()
    {
        [TenantTier.Starter] = new()
        {
            PlanFeature.SingleBranch,
            PlanFeature.MultiBranch,        // 1 branch allowed
            PlanFeature.ExternalApiAccess,
        },
        [TenantTier.Professional] = new()
        {
            PlanFeature.SingleBranch,
            PlanFeature.MultiBranch,        // up to 5 branches
            PlanFeature.AutomatedBackups,
            PlanFeature.AdvancedReports,
            PlanFeature.ExternalApiAccess,
        },
        [TenantTier.Enterprise] = new()
        {
            PlanFeature.SingleBranch,
            PlanFeature.MultiBranch,        // unlimited
            PlanFeature.AutomatedBackups,
            PlanFeature.CustomSmtp,
            PlanFeature.SandboxEnvironments,
            PlanFeature.AdvancedReports,
            PlanFeature.ExternalApiAccess,
            PlanFeature.PrioritySupport
        }
    };

    /// <summary>Human-readable limits per tier (branches, storage, seats).</summary>
    public static PlanLimits GetLimits(TenantTier tier) => tier switch
    {
        TenantTier.Starter => new PlanLimits(1, 5, 7, 500) { GracePeriodDays = 3 },
        TenantTier.Professional => new PlanLimits(5, 25, 30, 5000) { GracePeriodDays = 7 },
        // Wave 19 — Enterprise: unlimited branches + seats, 365-day retention, unlimited invoices.
        TenantTier.Enterprise => new PlanLimits(int.MaxValue, int.MaxValue, 365, int.MaxValue) { GracePeriodDays = 14 },
        _ => new PlanLimits(1, 5, 7, 500) { GracePeriodDays = 3 }
    };

    /// <summary>
    /// Wave 18 — convert a plan id ("starter" / "professional" / "enterprise")
    /// to the matching <see cref="TenantTier"/>. Unknown ids return null so
    /// the IPN handler can reject malformed payloads cleanly.
    /// </summary>
    public static TenantTier? FromPlanId(string? planId)
    {
        if (string.IsNullOrWhiteSpace(planId)) return null;
        return planId.Trim().ToLowerInvariant() switch
        {
            "starter" => TenantTier.Starter,
            "professional" => TenantTier.Professional,
            "enterprise" => TenantTier.Enterprise,
            _ => null
        };
    }

    /// <summary>
    /// Wave 18 — compute the next billing window for a successful payment.
    /// Defaults to 30 days; aligned with PayDunya's monthly cadence.
    /// </summary>
    public static DateTime ComputeNextBillingAtUtc(DateTime paidAtUtc)
        => DateTime.SpecifyKind(paidAtUtc, DateTimeKind.Utc).AddDays(30);

    public static bool IsFeatureEnabled(TenantTier tier, PlanFeature feature)
    {
        return Map.TryGetValue(tier, out var set) && set.Contains(feature);
    }

    /// <summary>Returns the features unlocked at the given tier (useful for UI display).</summary>
    public static IReadOnlyCollection<PlanFeature> GetFeatures(TenantTier tier)
    {
        return Map.TryGetValue(tier, out var set)
            ? set.ToArray()
            : Array.Empty<PlanFeature>();
    }
}

public record PlanLimits(int MaxBranches, int MaxUsers, int BackupRetentionDays, int MonthlyInvoices)
{
    /// <summary>Grace period (days) granted when a subscription payment fails.</summary>
    public int GracePeriodDays { get; init; } = 3;

    /// <summary>Format for the pricing UI — uses K/M for large numbers.</summary>
    public string InvoicesDisplay => MonthlyInvoices == int.MaxValue
        ? "Unlimited"
        : MonthlyInvoices >= 1_000_000 ? $"{MonthlyInvoices / 1_000_000}M+"
        : MonthlyInvoices >= 1_000 ? $"{MonthlyInvoices / 1_000}K"
        : MonthlyInvoices.ToString();
}