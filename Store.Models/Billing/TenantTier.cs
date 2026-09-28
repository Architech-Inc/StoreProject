namespace Store.Models.Billing;

/// <summary>
/// MT-02 — tenant plan tier. Mirrored from
/// <c>Store.ControlPlane.Models.TenantTier</c> so library code (plan
/// catalog, billing DTOs, JWT claims) doesn't have to depend on the
/// ControlPlane assembly. The ControlPlane copy stays as the persistence
/// enum and this is the public-facing catalog tier.
/// </summary>
public enum TenantTier
{
    Starter,
    Professional,
    Enterprise
}