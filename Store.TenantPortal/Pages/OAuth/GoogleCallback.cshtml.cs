using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages.OAuth;

[Authorize]
public class GoogleCallbackModel : PageModel
{
    private readonly IOAuthService _oauthService;
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<GoogleCallbackModel> _logger;

    public GoogleCallbackModel(
        IOAuthService oauthService,
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<GoogleCallbackModel> logger)
    {
        _oauthService = oauthService;
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? code, string? state, string? error, CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session?.HasTenant != true)
        {
            return RedirectToPage("/Onboarding");
        }

        if (!string.IsNullOrEmpty(error))
        {
            IsSuccess = false;
            Message = $"Google OAuth error: {error}";
            return Page();
        }

        if (string.IsNullOrEmpty(code))
        {
            IsSuccess = false;
            Message = "No authorization code was returned by Google.";
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
            var redirectUri = $"{Request.Scheme}://{Request.Host}/oauth/google/callback";
            var tokens = await _oauthService.ExchangeGoogleCodeAsync(code, redirectUri, ct);

            await _cpClient.SaveOAuthTokensAsync(session.TenantId!.Value, new SaveOAuthTokensRequest(
                ProviderType: "GoogleDrive",
                AccessToken: tokens.AccessToken,
                RefreshToken: tokens.RefreshToken,
                AccountEmail: tokens.AccountEmail,
                AccountName: tokens.AccountName,
                ExpiresInSeconds: tokens.ExpiresInSeconds
            ), ct);

            IsSuccess = true;
            Message = $"Successfully linked Google Drive account ({tokens.AccountEmail}). Automated snapshots will be uploaded to your dedicated 'ClexAn Backups' folder.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete Google OAuth exchange.");
            IsSuccess = false;
            Message = $"Failed to connect Google Drive: {ex.Message}";
            return Page();
        }
    }
}
