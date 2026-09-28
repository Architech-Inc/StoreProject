using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

using Store.Models.Common;
namespace Store.TenantPortal.Pages;

[Authorize]
public class DomainsModel : PageModel
{
    private readonly ILogger<DomainsModel> _logger;
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public DomainsModel(IControlPlaneClient cpClient, IPortalSessionService sessionService, ILogger<DomainsModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    
        _logger = logger;}

    public TenantDomainDto? DomainConfig { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        DomainConfig = await _cpClient.GetDomainConfigAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostSetCustomDomainAsync(string domain, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        try
        {
            DomainConfig = await _cpClient.SetCustomDomainAsync(session.TenantId!.Value, domain, ct);
            FeedbackMessage = "Custom domain registered. Please configure the DNS TXT record below to complete verification.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = SafeErrorMessage.From(ex, _logger, "Domains operation");
            IsError = true;
            DomainConfig = await _cpClient.GetDomainConfigAsync(session.TenantId!.Value, ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostVerifyDomainAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        var result = await _cpClient.VerifyCustomDomainAsync(session.TenantId!.Value, ct);
        FeedbackMessage = result.Message;
        IsError = !result.IsVerified;

        DomainConfig = await _cpClient.GetDomainConfigAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveCustomDomainAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await _cpClient.RemoveCustomDomainAsync(session.TenantId!.Value, ct);
        FeedbackMessage = "Custom domain removed.";
        IsError = false;

        DomainConfig = await _cpClient.GetDomainConfigAsync(session.TenantId!.Value, ct);
        return Page();
    }
}
