using Store.ControlPlane.Models;
using Store.ControlPlane.Repositories;
using Store.Models.Billing;

namespace Store.ControlPlane.Services;

/// <summary>
/// Wave 18 — payment reconciliation. Called by the BillingController IPN
/// handler once a PayDunya webhook passes signature validation.
///
/// Updates <see cref="Tenant.PlanTier"/>, subscription lifecycle fields,
/// and appends a <see cref="TenantPayment"/> to the tenant's payment
/// history. Idempotent on the payment token — repeated IPNs for the same
/// token don't create duplicate history entries.
///
/// Side-effects are limited to the tenant row. We do NOT touch Docker
/// stacks, branches, or backups here — quota enforcement reads these
/// values lazily through the API middleware.
/// </summary>
public class SubscriptionReconciler
{
    private readonly ITenantRepository _tenantRepo;
    private readonly ILogger<SubscriptionReconciler> _logger;

    public SubscriptionReconciler(ITenantRepository tenantRepo, ILogger<SubscriptionReconciler> logger)
    {
        _tenantRepo = tenantRepo;
        _logger = logger;
    }

    public async Task<bool> ReconcileAsync(
        string? providerToken,
        string? status,
        string? responseCode,
        int? amount,
        string? currency,
        string? channel,
        Guid? tenantId,
        string? planId,
        DateTime? completedAtUtc,
        string? failureReason,
        CancellationToken ct = default)
    {
        if (tenantId is null || tenantId == Guid.Empty)
        {
            _logger.LogWarning("IPN payload missing tenant id — cannot reconcile.");
            return false;
        }

        var tenant = await _tenantRepo.GetByIdAsync(tenantId.Value, ct);
        if (tenant is null)
        {
            _logger.LogWarning("IPN for unknown tenant {TenantId}; ignoring.", tenantId);
            return false;
        }

        // Idempotency — if we've already recorded this exact provider token
        // with the same status, skip the redundant write.
        if (!string.IsNullOrWhiteSpace(providerToken) &&
            tenant.Payments.Any(p => p.ProviderToken == providerToken && p.Status == status))
        {
            _logger.LogInformation(
                "IPN for tenant {Tenant} token {Token} already reconciled; skipping.",
                tenant.Slug, providerToken);
            return true;
        }

        var now = DateTime.UtcNow;
        var completed = completedAtUtc ?? now;

        var payment = new TenantPayment
        {
            Provider = "paydunya",
            ProviderToken = providerToken ?? string.Empty,
            Channel = channel,
            Amount = amount ?? 0,
            Currency = currency ?? tenant.Currency ?? "XAF",
            Status = status ?? "unknown",
            PlanId = planId,
            CreatedAtUtc = now,
            CompletedAtUtc = string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase) ? completed : null,
            FailureReason = failureReason
        };
        tenant.Payments.Add(payment);

        // Only successful completions upgrade the plan.
        if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(planId))
        {
            var newTier = PlanCatalog.FromPlanId(planId);
            if (newTier.HasValue)
            {
                var limits = PlanCatalog.GetLimits(newTier.Value);
                var prevTier = tenant.PlanTier;

                tenant.PlanTier = newTier.Value;
                tenant.SubscriptionPlanId = planId;
                tenant.SubscriptionStatus = SubscriptionStatus.Active;
                tenant.SubscriptionStartUtc = completed;
                tenant.SubscriptionEndUtc = PlanCatalog.ComputeNextBillingAtUtc(completed);
                tenant.NextBillingAtUtc = tenant.SubscriptionEndUtc;
                tenant.GracePeriodUntilUtc = null;
                tenant.LastPaymentToken = providerToken;

                tenant.AuditTrail.Add(new TenantAuditRecord
                {
                    TenantId = tenant.TenantId,
                    ActionType = "SubscriptionUpgraded",
                    Details = $"Plan upgraded from {prevTier} to {newTier.Value} via PayDunya (token {providerToken}, {amount} {payment.Currency}). Limits: {limits.MaxBranches} branches / {limits.MaxUsers} users."
                });
            }
            else
            {
                tenant.AuditTrail.Add(new TenantAuditRecord
                {
                    TenantId = tenant.TenantId,
                    ActionType = "PaymentUnknownPlan",
                    Details = $"Received completed payment for unknown plan id '{planId}'."
                });
            }
        }
        else if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase)
              || string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase))
        {
            var limits = PlanCatalog.GetLimits(tenant.PlanTier);
            var graceUntil = now.AddDays(limits.GracePeriodDays);

            tenant.SubscriptionStatus = SubscriptionStatus.GracePeriod;
            tenant.GracePeriodUntilUtc = graceUntil;

            tenant.AuditTrail.Add(new TenantAuditRecord
            {
                TenantId = tenant.TenantId,
                ActionType = "SubscriptionFailed",
                Details = $"Payment {status} (response {responseCode}); grace period until {graceUntil:O}. Reason: {failureReason ?? "—"}"
            });
        }

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation(
            "Reconciled PayDunya IPN for tenant {Slug}: plan={PlanId} status={Status} amount={Amount} {Currency}",
            tenant.Slug, planId, status, amount, currency);
        return true;
    }
}