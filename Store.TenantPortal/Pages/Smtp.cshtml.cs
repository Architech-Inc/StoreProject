using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.Billing;
using Store.Models.Common;
using Store.Models.DTOs.Tenant;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

[Authorize]
public class SmtpModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<SmtpModel> _logger;

    public SmtpModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<SmtpModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public TenantSmtpConfigDto? SmtpConfig { get; set; }
    public TenantDetailDto? TenantDetail { get; set; }
    public bool IsEnterprise { get; set; }
    public string? FeedbackMessage { get; set; }
    public bool IsError { get; set; }
    public TestSmtpResponse? TestResult { get; set; }

    [BindProperty]
    public UpdateTenantSmtpRequest SmtpInput { get; set; } = new();

    [BindProperty]
    public string TestRecipientEmail { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.HasTenant)
        {
            return RedirectToPage("/Onboarding");
        }

        await LoadTenantStateAsync(session.TenantId!.Value, session.TenantSlug!, ct);

        if (SmtpConfig != null)
        {
            SmtpInput = new UpdateTenantSmtpRequest
            {
                Host = SmtpConfig.Host,
                Port = SmtpConfig.Port,
                Username = SmtpConfig.Username,
                FromEmail = SmtpConfig.FromEmail,
                FromName = SmtpConfig.FromName,
                EnableSsl = SmtpConfig.EnableSsl,
                IsEnabled = SmtpConfig.IsEnabled
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await LoadTenantStateAsync(session.TenantId!.Value, session.TenantSlug!, ct);

        if (!IsEnterprise)
        {
            FeedbackMessage = "Custom SMTP Relay is an Enterprise plan feature. Please upgrade your plan tier to enable custom mail servers.";
            IsError = true;
            return Page();
        }

        if (!ModelState.IsValid)
        {
            FeedbackMessage = "Please review the form for errors.";
            IsError = true;
            return Page();
        }

        try
        {
            SmtpConfig = await _cpClient.UpdateSmtpConfigAsync(session.TenantId!.Value, SmtpInput, ct);
            FeedbackMessage = "Custom SMTP configuration saved successfully.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = SafeErrorMessage.From(ex, _logger, "SMTP Configuration");
            IsError = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostTestAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        await LoadTenantStateAsync(session.TenantId!.Value, session.TenantSlug!, ct);

        if (!IsEnterprise)
        {
            FeedbackMessage = "Custom SMTP Relay is an Enterprise plan feature.";
            IsError = true;
            return Page();
        }

        if (string.IsNullOrWhiteSpace(TestRecipientEmail) || !TestRecipientEmail.Contains('@'))
        {
            FeedbackMessage = "Please provide a valid recipient email address for testing.";
            IsError = true;
            return Page();
        }

        try
        {
            TestResult = await _cpClient.TestSmtpConfigAsync(session.TenantId!.Value, new TestSmtpRequest { RecipientEmail = TestRecipientEmail.Trim() }, ct);
            FeedbackMessage = TestResult.Message;
            IsError = !TestResult.Success;
            SmtpConfig = await _cpClient.GetSmtpConfigAsync(session.TenantId!.Value, ct);
        }
        catch (Exception ex)
        {
            FeedbackMessage = SafeErrorMessage.From(ex, _logger, "SMTP Test");
            IsError = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostResetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true) return RedirectToPage("/Onboarding");

        try
        {
            await _cpClient.ResetSmtpConfigAsync(session.TenantId!.Value, ct);
            FeedbackMessage = "Custom SMTP configuration reset to platform defaults.";
            IsError = false;
        }
        catch (Exception ex)
        {
            FeedbackMessage = SafeErrorMessage.From(ex, _logger, "SMTP Reset");
            IsError = true;
        }

        await LoadTenantStateAsync(session.TenantId!.Value, session.TenantSlug!, ct);
        return Page();
    }

    private async Task LoadTenantStateAsync(Guid tenantId, string slug, CancellationToken ct)
    {
        TenantDetail = await _cpClient.GetTenantAsync(slug, ct);
        if (TenantDetail != null && Enum.TryParse<TenantTier>(TenantDetail.PlanTier, true, out var tier))
        {
            IsEnterprise = PlanCatalog.IsFeatureEnabled(tier, PlanFeature.CustomSmtp);
        }
        else
        {
            IsEnterprise = false;
        }
        SmtpConfig = await _cpClient.GetSmtpConfigAsync(tenantId, ct);
    }
}
