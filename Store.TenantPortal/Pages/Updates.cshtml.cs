using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class UpdatesModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<UpdatesModel> _logger;

    public UpdatesModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<UpdatesModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public TenantSdlcStatusDto? SdlcStatus { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant || string.IsNullOrEmpty(session.TenantSlug))
        {
            return RedirectToPage("/Onboarding");
        }

        SdlcStatus = await _cpClient.GetSdlcStatusAsync(session.TenantSlug, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostUpgradeAsync(Guid releaseId, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true || string.IsNullOrEmpty(session.TenantSlug))
        {
            return RedirectToPage("/Onboarding");
        }

        var success = await _cpClient.UpgradeTenantAsync(session.TenantSlug, releaseId, ct);
        if (success)
        {
            FeedbackMessage = "System upgrade initiated. Pre-flight snapshot captured and containers are restarting with the target release.";
            IsError = false;
        }
        else
        {
            FeedbackMessage = "Failed to upgrade tenant. Please check system logs or try again.";
            IsError = true;
        }

        SdlcStatus = await _cpClient.GetSdlcStatusAsync(session.TenantSlug, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRollbackAsync(Guid snapshotId, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true || string.IsNullOrEmpty(session.TenantSlug))
        {
            return RedirectToPage("/Onboarding");
        }

        var success = await _cpClient.RollbackTenantAsync(session.TenantSlug, snapshotId, ct);
        if (success)
        {
            FeedbackMessage = "Rollback successful. Database state restored from snapshot and containers reverted to the previous release.";
            IsError = false;
        }
        else
        {
            FeedbackMessage = "Failed to perform rollback. Please ensure snapshot is accessible.";
            IsError = true;
        }

        SdlcStatus = await _cpClient.GetSdlcStatusAsync(session.TenantSlug, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateSandboxAsync(Guid releaseId, bool maskData, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true || string.IsNullOrEmpty(session.TenantSlug))
        {
            return RedirectToPage("/Onboarding");
        }

        var sandbox = await _cpClient.CreateSandboxAsync(session.TenantSlug, releaseId, maskData, ct);
        if (sandbox != null)
        {
            FeedbackMessage = $"Preview sandbox '{sandbox.Slug}' provisioned successfully! Live database cloned and isolated with {(maskData ? "data masking active" : "full copy")}.";
            IsError = false;
        }
        else
        {
            FeedbackMessage = "Failed to provision preview sandbox. Verify system resource limits or try again.";
            IsError = true;
        }

        SdlcStatus = await _cpClient.GetSdlcStatusAsync(session.TenantSlug, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteSandboxAsync(string sandboxSlug, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true || string.IsNullOrEmpty(session.TenantSlug))
        {
            return RedirectToPage("/Onboarding");
        }

        var success = await _cpClient.DeleteSandboxAsync(session.TenantSlug, sandboxSlug, ct);
        if (success)
        {
            FeedbackMessage = $"Sandbox '{sandboxSlug}' was successfully decommissioned and removed.";
            IsError = false;
        }
        else
        {
            FeedbackMessage = $"Failed to decommission sandbox '{sandboxSlug}'.";
            IsError = true;
        }

        SdlcStatus = await _cpClient.GetSdlcStatusAsync(session.TenantSlug, ct);
        return Page();
    }
}
