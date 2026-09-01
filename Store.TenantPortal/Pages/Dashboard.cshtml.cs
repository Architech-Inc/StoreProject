using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<DashboardModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public TenantDetailDto Tenant { get; set; } = null!;
    public PortalSession Session { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null)
        {
            return RedirectToPage("/Login");
        }

        Session = session;

        if (!session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        var tenantDetails = await _cpClient.GetTenantDetailsAsync(session.TenantId!.Value, ct);
        if (tenantDetails == null)
        {
            _logger.LogWarning("Tenant with ID {TenantId} not found in Control Plane", session.TenantId);
            return RedirectToPage("/Onboarding");
        }

        Tenant = tenantDetails;
        return Page();
    }

    public async Task<IActionResult> OnPostCheckHealthAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant == true)
        {
            await _cpClient.CheckTenantHealthAsync(session.TenantId!.Value, ct);
        }

        return RedirectToPage();
    }
}
