using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class EnvironmentModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public EnvironmentModel(IControlPlaneClient cpClient, IPortalSessionService sessionService)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    }

    public EnvironmentStatusDto? EnvStatus { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        EnvStatus = await _cpClient.GetEnvironmentStatusAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRestartServiceAsync(string service, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        var success = await _cpClient.RestartServiceAsync(session.TenantId!.Value, service, ct);
        if (success)
        {
            FeedbackMessage = $"Service '{service}' restarted successfully.";
            IsError = false;
        }
        else
        {
            FeedbackMessage = $"Failed to restart service '{service}'.";
            IsError = true;
        }

        EnvStatus = await _cpClient.GetEnvironmentStatusAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostSuspendSiloAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await _cpClient.SuspendTenantAsync(session.TenantId!.Value, ct);
        FeedbackMessage = "Store silo suspended.";
        IsError = false;

        EnvStatus = await _cpClient.GetEnvironmentStatusAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostResumeSiloAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await _cpClient.ResumeTenantAsync(session.TenantId!.Value, ct);
        FeedbackMessage = "Store silo resumed.";
        IsError = false;

        EnvStatus = await _cpClient.GetEnvironmentStatusAsync(session.TenantId!.Value, ct);
        return Page();
    }
}
