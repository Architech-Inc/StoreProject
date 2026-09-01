using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class BackupsModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IOAuthService _oauthService;
    private readonly IPortalSessionService _sessionService;

    public BackupsModel(
        IControlPlaneClient cpClient,
        IOAuthService oauthService,
        IPortalSessionService sessionService)
    {
        _cpClient = cpClient;
        _oauthService = oauthService;
        _sessionService = sessionService;
    }

    public BackupSummaryDto? Summary { get; set; }
    public string MicrosoftAuthUrl { get; set; } = string.Empty;
    public string GoogleAuthUrl { get; set; } = string.Empty;
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        Summary = await _cpClient.GetBackupSummaryAsync(session.TenantId!.Value, ct);

        var msRedirect = $"{Request.Scheme}://{Request.Host}/oauth/microsoft/callback";
        var googleRedirect = $"{Request.Scheme}://{Request.Host}/oauth/google/callback";

        var signedState = _oauthService.GenerateSignedState(session.TenantId.Value);
        MicrosoftAuthUrl = _oauthService.BuildMicrosoftAuthUrl(signedState, msRedirect);
        GoogleAuthUrl = _oauthService.BuildGoogleAuthUrl(signedState, googleRedirect);

        return Page();
    }

    public async Task<IActionResult> OnPostTriggerBackupAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        try
        {
            var res = await _cpClient.TriggerBackupAsync(session.TenantId!.Value, ct);
            FeedbackMessage = res.Message;
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = ex.Message;
            IsError = true;
        }

        return await OnGetAsync(ct);
    }

    public async Task<IActionResult> OnPostConfigureS3Async(ConfigureS3Request request, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        try
        {
            await _cpClient.ConfigureS3ProviderAsync(session.TenantId!.Value, request, ct);
            FeedbackMessage = $"S3 bucket '{request.BucketName}' connected successfully.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = ex.Message;
            IsError = true;
        }

        return await OnGetAsync(ct);
    }

    public async Task<IActionResult> OnPostDisconnectProviderAsync(string provider, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await _cpClient.DisconnectBackupProviderAsync(session.TenantId!.Value, provider, ct);
        FeedbackMessage = $"{provider} storage disconnected.";
        IsError = false;

        return await OnGetAsync(ct);
    }

    public async Task<IActionResult> OnPostUpdateScheduleAsync(UpdateScheduleRequest request, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        try
        {
            await _cpClient.UpdateBackupScheduleAsync(session.TenantId!.Value, request, ct);
            FeedbackMessage = "Backup schedule & retention policy updated successfully.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = ex.Message;
            IsError = true;
        }

        return await OnGetAsync(ct);
    }
}
