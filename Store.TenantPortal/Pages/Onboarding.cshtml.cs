using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Models.ViewModels;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class OnboardingModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<OnboardingModel> _logger;

    public OnboardingModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<OnboardingModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    [BindProperty]
    public OnboardingVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant == true)
        {
            return RedirectToPage("/Dashboard");
        }
        return Page();
    }

    public async Task<IActionResult> OnGetCheckSlugAsync(string slug, CancellationToken ct)
    {
        var result = await _cpClient.CheckSlugAvailabilityAsync(slug, ct);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null)
        {
            return RedirectToPage("/Login");
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the form errors before submitting.";
            return Page();
        }

        try
        {
            var provisionReq = new ProvisionTenantDto(
                Name: Input.StoreName.Trim(),
                Slug: Input.StoreSlug.Trim().ToLowerInvariant(),
                AdminEmail: session.Email,
                AdminUsername: Input.AdminUsername.Trim(),
                AdminPassword: Input.AdminPassword,
                Currency: Input.Currency,
                PlanTier: Input.PlanTier,
                CustomDomain: Input.DomainChoice == "Custom" ? Input.CustomDomain?.Trim() : null
            );

            _logger.LogInformation("Triggering tenant provisioning for account {Email}, slug {Slug}", session.Email, provisionReq.Slug);

            var tenantSummary = await _cpClient.ProvisionTenantAsync(provisionReq, ct);

            // Link account to the newly provisioned tenant
            await _cpClient.LinkAccountToTenantAsync(session.AccountId, tenantSummary.TenantId, ct);

            // Update cookie session claims
            await _sessionService.UpdateTenantInfoAsync(HttpContext, tenantSummary.TenantId, tenantSummary.Slug, tenantSummary.Name);

            _logger.LogInformation("Tenant {Slug} ({TenantId}) provisioned successfully!", tenantSummary.Slug, tenantSummary.TenantId);

            return RedirectToPage("/Dashboard");
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant stack");
            ErrorMessage = "An unexpected error occurred during container deployment. Please try again or contact support.";
            return Page();
        }
    }
}
