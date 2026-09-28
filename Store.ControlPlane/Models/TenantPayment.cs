namespace Store.ControlPlane.Models;

/// <summary>
/// Wave 18 — payment record persisted on <see cref="Tenant.Payments"/>.
/// One row per PayDunya IPN we accept. Stored as JSON for parity with
/// AuditTrail / BackupHistory; surfaced to the portal via the Billing page
/// and to operators via the ControlPlane admin surface.
/// </summary>
public class TenantPayment
{
    public Guid PaymentId { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = "paydunya";
    public string ProviderToken { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; } = "XAF";
    public string Status { get; set; } = string.Empty; // completed | pending | failed | cancelled
    public string? PlanId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
}

public enum SubscriptionStatus
{
    /// <summary>Default — trial or first-period active.</summary>
    Active = 0,
    /// <summary>Period ended but inside the grace window; still serving traffic.</summary>
    GracePeriod = 1,
    /// <summary>Period ended, grace expired; plan downgraded to Starter.</summary>
    Expired = 2,
    /// <summary>Operator-cancelled. Won't auto-renew.</summary>
    Cancelled = 3
}