using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

/// <summary>
/// MT-07 — public-facing tenant status page.
///
/// Anonymous visitors (no login required) can hit <c>/Status/{slug}</c> to
/// see whether a tenant is healthy and what's planned for upcoming /
/// in-progress maintenance. Operator-only maintenance-window CRUD lives
/// behind authentication in the dashboard.
/// </summary>
[AllowAnonymous]
public class StatusModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly ILogger<StatusModel> _logger;

    public StatusModel(IControlPlaneClient cpClient, ILogger<StatusModel> logger)
    {
        _cpClient = cpClient;
        _logger = logger;
    }

    /// <summary>The slug from the route — what the visitor is asking about.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Public payload from /api/public/tenants/{slug}/status. NULL when not found.</summary>
    public TenantStatusDto? Status { get; private set; }

    public bool TenantNotFound { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        Slug = slug ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Slug))
        {
            TenantNotFound = true;
            return Page();
        }

        Status = await _cpClient.GetTenantPublicStatusAsync(Slug, ct);
        if (Status is null)
        {
            TenantNotFound = true;
            return Page();
        }

        return Page();
    }
}