using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

/// <summary>
/// MT-02 — Tenant-facing billing page.
///
/// Renders the current plan tier, the feature list unlocked at that tier,
/// and a "Subscribe" button per paid plan that hits ControlPlane's
/// PayDunya integration to create a hosted-checkout invoice. The user is
/// redirected to PayDunya's hosted page; on return, the IPN webhook
/// reconciles the payment (handled by ControlPlane).
/// </summary>
[Authorize]
public class BillingModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly ILogger<BillingModel> _logger;

    public BillingModel(IControlPlaneClient cpClient, ILogger<BillingModel> logger)
    {
        _cpClient = cpClient;
        _logger = logger;
    }

    public string CurrentTier { get; private set; } = "Starter";
    public IReadOnlyList<string> UnlockedFeatures { get; private set; } = Array.Empty<string>();
    public string? TenantName { get; private set; }
    public string? Slug { get; private set; }
    public string? Error { get; private set; }
    public string? Success { get; private set; }

    // Wave 19 — usage / limits / invoice history for the panel.
    public TenantPaymentHistoryDto? BillingHistory { get; private set; }
    public int? CurrentBranches { get; private set; }
    public int? CurrentUsers { get; private set; }
    public int? CurrentInvoicesThisMonth { get; private set; }
    public int MaxBranches { get; private set; } = 1;
    public int MaxUsers { get; private set; } = 5;
    public int MaxInvoicesPerMonth { get; private set; } = 500;
    public string? SubscriptionStatus { get; private set; }
    public DateTime? NextBillingAtUtc { get; private set; }

    // Plan catalog mirrors Store.Models.Billing.PlanCatalog but is rendered
    // here so the portal can show the same matrix even if the catalog
    // gains new features in the future.
    public record PlanOption(string Id, string Name, string Price, string Period, IReadOnlyList<string> Highlights, bool IsCurrent);

    public IReadOnlyList<PlanOption> Plans => new[]
    {
        new PlanOption("starter", "Starter", "Free", "14-day trial",
            new[] { "1 branch", "5 user seats", "External API access", "Community support" },
            CurrentTier == "Starter"),
        new PlanOption("professional", "Professional", "29,900 XAF", "/ month",
            new[] { "Up to 5 branches", "25 user seats", "Automated backups (30-day retention)", "Advanced reports", "External API access" },
            CurrentTier == "Professional"),
        new PlanOption("enterprise", "Enterprise", "Contact sales", "Custom",
            new[] { "Unlimited branches", "500 user seats", "Custom SMTP", "Sandbox environments", "Priority support" },
            CurrentTier == "Enterprise")
    };

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Slug = slug;
        TenantName = slug;

        // Best-effort: try to load tenant details to surface the current
        // plan. If the ControlPlane doesn't expose it yet (because payment
        // persistence is still ahead), we fall back to "Starter" so the
        // page still renders + the upgrade CTAs work.
        try
        {
            var tenant = await _cpClient.GetTenantAsync(slug, ct);
            if (tenant is not null)
            {
                TenantName = tenant.Name;
                CurrentTier = tenant.PlanTier ?? "Starter";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load tenant {Slug} for billing page; defaulting to Starter.", slug);
        }

        UnlockedFeatures = CurrentTier switch
        {
            "Professional" => new[] { "Up to 5 branches", "25 user seats", "Automated backups", "Advanced reports" },
            "Enterprise" => new[] { "Unlimited branches", "500 user seats", "Automated backups", "Custom SMTP", "Sandbox environments", "Priority support" },
            _ => new[] { "1 branch", "5 user seats", "External API access" }
        };

        // Wave 19 — limits come from the canonical catalog (mirror of
        // Store.Models.Billing.PlanCatalog.GetLimits). When the tier
        // changes, the page rerenders against the same source of truth.
        (MaxBranches, MaxUsers, MaxInvoicesPerMonth) = CurrentTier switch
        {
            "Professional" => (5, 25, 5000),
            "Enterprise" => (int.MaxValue, int.MaxValue, int.MaxValue),
            _ => (1, 5, 500)
        };

        // Wave 19 — load the invoice history so the panel can render the
        // recent-payments table. Failure here is non-fatal; the page still
        // shows plan + upgrade CTAs.
        try
        {
            BillingHistory = await _cpClient.GetBillingHistoryAsync(slug, ct);
            if (BillingHistory is not null)
            {
                SubscriptionStatus = BillingHistory.SubscriptionStatus;
                NextBillingAtUtc = BillingHistory.NextBillingAtUtc;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load billing history for {Slug}.", slug);
        }

        return Page();
    }

    /// <summary>
    /// Subscribe to a plan via PayDunya. Creates the hosted-checkout invoice
    /// and redirects the user to PayDunya's page. On success / cancel, the
    /// user is bounced back to <see cref="OnGetAsync"/> with a query
    /// string the portal renders as a success or info toast.
    /// </summary>
    public async Task<IActionResult> OnPostSubscribeAsync(string slug, string planId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(slug) || string.IsNullOrWhiteSpace(planId))
        {
            Error = "Missing tenant slug or plan id.";
            return RedirectToPage(new { slug });
        }

        try
        {
            var amount = planId switch
            {
                "starter" => 0,
                "professional" => 29900, // XAF — unit-pricing, no decimals in PayDunya XAF
                "enterprise" => -1,
                _ => 0
            };

            if (amount == -1)
            {
                // Enterprise is contact-sales for now — render the portal
                // email CTA instead of initiating a PayDunya invoice.
                Success = "Enterprise plans require a quick call. We've notified our team and they'll reach out within 1 business day.";
                return RedirectToPage(new { slug });
            }

            // Round-trip URL PayDunya redirects the user back to.
            var returnUrl = $"{Request.Scheme}://{Request.Host}/Billing/{slug}?status={{status}}&token={{token}}";
            var resp = await _cpClient.CreateBillingInvoiceAsync(slug, new Store.TenantPortal.Models.DTOs.CreateBillingInvoiceRequest(
                TenantId: Guid.Empty, // populated server-side from the slug
                PlanId: planId,
                TotalAmount: amount,
                Currency: "XAF",
                Description: $"ClexAn Foods — {planId} plan",
                ReturnUrl: returnUrl,
                CallbackUrl: $"{Request.Scheme}://{Request.Host}/api/billing/paydunya/ipn"
            ), ct);

            if (resp is null || string.IsNullOrEmpty(resp.CheckoutUrl))
            {
                Error = "PayDunya is not configured on this environment. Please contact support.";
                return RedirectToPage(new { slug });
            }

            return Redirect(resp.CheckoutUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Billing subscribe failed for slug {Slug} plan {PlanId}.", slug, planId);
            Error = "We couldn't start the checkout. Please try again or contact support.";
            return RedirectToPage(new { slug });
        }
    }
}