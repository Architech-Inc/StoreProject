using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class BranchesModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public BranchesModel(IControlPlaneClient cpClient, IPortalSessionService sessionService)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    }

    public IReadOnlyList<BranchDto> Branches { get; set; } = Array.Empty<BranchDto>();
    public PortalSession? Session { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        Session = session;
        Branches = await _cpClient.GetBranchesAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAddBranchAsync(string branchName, string branchSlug, string domainType, string? customSubdomain, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        Session = session;

        try
        {
            var req = new CreateBranchRequest(branchName, branchSlug, domainType, customSubdomain);
            await _cpClient.AddBranchAsync(session.TenantId!.Value, req, ct);
            FeedbackMessage = $"Branch '{branchName}' mapped successfully.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = ex.Message;
            IsError = true;
        }

        Branches = await _cpClient.GetBranchesAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyBranchAsync(Guid branchId, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        Session = session;
        var result = await _cpClient.VerifyBranchAsync(session.TenantId!.Value, branchId, ct);
        FeedbackMessage = result.Message;
        IsError = !result.IsVerified;

        Branches = await _cpClient.GetBranchesAsync(session.TenantId!.Value, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveBranchAsync(Guid branchId, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        Session = session;
        await _cpClient.RemoveBranchAsync(session.TenantId!.Value, branchId, ct);
        FeedbackMessage = "Branch mapping removed.";
        IsError = false;

        Branches = await _cpClient.GetBranchesAsync(session.TenantId!.Value, ct);
        return Page();
    }
}
