using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

using Store.Models.Common;
namespace Store.TenantPortal.Pages;

/// <summary>
/// MT-01 — Async provisioning status page. Polls the ControlPlane every 3
/// seconds until the queued job reaches a terminal state (Completed/Failed),
/// then redirects to Dashboard on success or shows a Retry button on failure.
/// </summary>
[Authorize]
public class OnboardingStatusModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<OnboardingStatusModel> _logger;

    public OnboardingStatusModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<OnboardingStatusModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid JobId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Slug { get; set; }

    /// <summary>Status returned by the most recent poll. Defaults to Pending.</summary>
    public ProvisioningJobResponse? Job { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null)
        {
            return RedirectToPage("/Login");
        }

        // Recover the JobId from TempData if the page was hit without a query string.
        if (JobId == Guid.Empty)
        {
            var fromTemp = TempData["ProvisioningJobId"] as string;
            if (Guid.TryParse(fromTemp, out var g))
            {
                JobId = g;
                Slug ??= TempData["ProvisioningSlug"] as string;
            }
            else
            {
                ErrorMessage = "No provisioning job is associated with this session. Please start from the Onboarding form.";
                return Page();
            }
        }

        if (JobId == Guid.Empty)
        {
            ErrorMessage = "Missing job id.";
            return Page();
        }

        Job = await _cpClient.GetProvisioningJobAsync(JobId, ct);

        if (Job is null)
        {
            ErrorMessage = "Provisioning job not found (it may have been cleaned up). Please retry from Onboarding.";
            return Page();
        }

        // Terminal state → redirect to Dashboard (or stay on this page with retry).
        if (Job.Status == "Completed" && Job.TenantId.HasValue)
        {
            // Link account to tenant is already done by the hosted service,
            // but we also need to update the cookie session so the next page
            // request lands in the tenant's UI.
            await _sessionService.UpdateTenantInfoAsync(
                HttpContext,
                Job.TenantId.Value,
                Job.TenantSlug ?? Slug ?? "tenant",
                Job.TenantSlug ?? Slug ?? "Tenant");

            _logger.LogInformation("Provisioning job {JobId} completed for tenant {TenantId}", JobId, Job.TenantId);
            TempData.Remove("ProvisioningJobId");
            TempData.Remove("ProvisioningSlug");
            return RedirectToPage("/Dashboard");
        }

        return Page();
    }

    /// <summary>Auto-poll handler called by JS every 3 seconds.</summary>
    public async Task<JsonResult> OnGetPollAsync(CancellationToken ct)
    {
        if (JobId == Guid.Empty)
        {
            return new JsonResult(new { status = "NotFound", detail = "Missing job id." });
        }

        var job = await _cpClient.GetProvisioningJobAsync(JobId, ct);
        if (job is null)
        {
            return new JsonResult(new { status = "NotFound" });
        }

        return new JsonResult(new
        {
            status = job.Status,
            detail = job.StatusDetail,
            failureReason = job.FailureReason,
            tenantId = job.TenantId,
            tenantSlug = job.TenantSlug
        });
    }

    /// <summary>Reset a Failed job back to Pending so the hosted service picks it up again.</summary>
    public async Task<IActionResult> OnPostRetryAsync(CancellationToken ct)
    {
        if (JobId == Guid.Empty) return RedirectToPage("/Onboarding");

        var ok = await _cpClient.RetryProvisioningJobAsync(JobId, ct);
        if (ok)
        {
            TempData["ProvisioningJobId"] = JobId.ToString();
            TempData["ProvisioningSlug"] = Slug;
            return RedirectToPage("/OnboardingStatus", new { jobId = JobId, slug = Slug });
        }

        ErrorMessage = "Retry is only valid for Failed jobs.";
        return Page();
    }
}
