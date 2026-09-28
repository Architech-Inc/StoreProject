using Store.ControlPlane.Repositories;
using Store.DbServices.Services;
using Store.Models.Billing;

namespace Store.ControlPlane.Services;

/// <summary>
/// Wave 20 — ControlPlane-side implementation of
/// <see cref="IQuotaEnforcementHandler"/>. Resolves the tenant by id,
/// reads the relevant usage count, then defers to the pure
/// <see cref="PlanQuotaGate"/>.
///
/// Today only <see cref="TenantQuota.Branches"/> has a backing count
/// (the tenant's <c>Branches</c> collection). Users and MonthlyInvoices
/// are placeholders that read <c>0</c> until a per-tenant counter
/// exists in the ControlPlane DB (currently those counts live in the
/// store DB which the ControlPlane doesn't see).
/// </summary>
public class ControlPlaneQuotaHandler : IQuotaEnforcementHandler
{
    private readonly ITenantRepository _tenantRepo;
    private readonly ILogger<ControlPlaneQuotaHandler> _logger;

    public ControlPlaneQuotaHandler(
        ITenantRepository tenantRepo,
        ILogger<ControlPlaneQuotaHandler> logger)
    {
        _tenantRepo = tenantRepo;
        _logger = logger;
    }

    public async Task<QuotaCheck> CheckAsync(
        Guid tenantId,
        TenantQuota quota,
        QuotaUsageSource usageSource,
        CancellationToken ct)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant is null)
        {
            // Tenant not found — let the endpoint return 404. The filter
            // never blocks when the entity doesn't exist.
            return QuotaCheck.Ok(quota.ToString().ToLowerInvariant(), 0, int.MaxValue);
        }

        // Per-tenant gate: rebuild with a tier resolver that locks to this
        // tenant. Cheap (PlanQuotaGate is a pure-function service).
        var gate = new PlanQuotaGate(() => tenant.PlanTier);

        var plus = usageSource == QuotaUsageSource.CurrentPlusOne ? 1 : 0;

        return quota switch
        {
            TenantQuota.Branches => gate.CheckBranchCreation((tenant.Branches?.Count ?? 0) + plus),
            TenantQuota.Users => gate.CheckUserSeat(0 + plus),
            TenantQuota.MonthlyInvoices => gate.CheckMonthlyInvoice(0 + plus),
            _ => QuotaCheck.Ok(quota.ToString().ToLowerInvariant(), 0, int.MaxValue)
        };
    }
}