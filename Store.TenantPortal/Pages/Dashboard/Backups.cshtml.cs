using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages.Dashboard;

[Authorize]
public class BackupsModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<BackupsModel> _logger;

    public BackupsModel(IControlPlaneClient cpClient, IPortalSessionService sessionService, ILogger<BackupsModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public BackupSummaryDto? BackupSummary { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.TenantId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        BackupSummary = await _cpClient.GetBackupSummaryAsync(session.TenantId.Value, ct);

        if (BackupSummary == null)
        {
            _logger.LogWarning("Failed to retrieve backup summary for tenant {TenantId}", session.TenantId.Value);
            // We'll still render the page but the UI will show an error/empty state
        }

        return Page();
    }
}
