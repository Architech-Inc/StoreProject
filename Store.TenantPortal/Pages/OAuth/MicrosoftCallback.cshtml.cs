using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages.OAuth;

[Authorize]
public class MicrosoftCallbackModel : PageModel
{
    private readonly IOAuthService _oauthService;
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<MicrosoftCallbackModel> _logger;

    public MicrosoftCallbackModel(
        IOAuthService oauthService,
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<MicrosoftCallbackModel> logger)
    {
        _oauthService = oauthService;
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? code, string? state, string? error, string? error_description, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true)
        {
            return RedirectToPage("/Onboarding");
        }

        if (!string.IsNullOrEmpty(error))
        {
            IsSuccess = false;
            Message = $"Microsoft OAuth error: {error_description ?? error}";
            return Page();
        }

        if (string.IsNullOrEmpty(code))
        {
            IsSuccess = false;
            Message = "No authorization code was returned by Microsoft.";
            return Page();
        }

        if (string.IsNullOrEmpty(state) || !_oauthService.ValidateSignedState(state, out var stateTenantId) || stateTenantId != session.TenantId!.Value)
        {
            IsSuccess = false;
            Message = "Invalid or expired anti-CSRF state token.";
            return Page();
        }

        try
        {
            var redirectUri = $"{Request.Scheme}://{Request.Host}/oauth/microsoft/callback";
            var tokens = await _oauthService.ExchangeMicrosoftCodeAsync(code, redirectUri, ct);

            await _cpClient.SaveOAuthTokensAsync(session.TenantId!.Value, new SaveOAuthTokensRequest(
                ProviderType: "OneDrive",
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken,
                AccountEmail: tokens.AccountEmail,
                AccountName: tokens.AccountName,
                ExpiresInSeconds: tokens.ExpiresInSeconds
            ), ct);

            IsSuccess = true;
            Message = $"Successfully linked Microsoft OneDrive account ({tokens.AccountEmail}). Automated snapshots will be uploaded to your secure AppFolder.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete Microsoft OAuth exchange.");
            IsSuccess = false;
            Message = $"Failed to connect OneDrive: {ex.Message}";
            return Page();
        }
    }
}
