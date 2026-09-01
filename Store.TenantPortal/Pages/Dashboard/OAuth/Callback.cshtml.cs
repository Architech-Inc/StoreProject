using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages.Dashboard.OAuth;

[Authorize]
public class CallbackModel : PageModel
{
    private readonly IOAuthService _oAuthService;
    private readonly IPortalSessionService _sessionService;
    private readonly IControlPlaneClient _cpClient;
    private readonly ILogger<CallbackModel> _logger;

    public CallbackModel(
        IOAuthService oAuthService, 
        IPortalSessionService sessionService, 
        IControlPlaneClient cpClient,
        ILogger<CallbackModel> logger)
    {
        _oAuthService = oAuthService;
        _sessionService = sessionService;
        _cpClient = cpClient;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Code { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? State { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Error { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? Provider { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.TenantId.HasValue) return RedirectToPage("/Login");

        if (!string.IsNullOrEmpty(Error))
        {
            _logger.LogWarning("OAuth callback returned error: {Error}", Error);
            ErrorMessage = $"Authentication failed: {Error}";
            return Page();
        }

        if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(State) || string.IsNullOrEmpty(Provider))
        {
            ErrorMessage = "Invalid OAuth callback response.";
            return Page();
        }

        if (!_oAuthService.ValidateSignedState(State, out var tenantId) || tenantId != session.TenantId.Value)
        {
            _logger.LogWarning("CSRF state validation failed or mismatched tenant in OAuth callback.");
            ErrorMessage = "Security validation failed. Please try again.";
            return Page();
        }

        try
        {
            var redirectUri = Url.PageLink("/Dashboard/OAuth/Callback", values: new { provider = Provider })!;
            
            OAuthTokenResult tokenResult;
            if (Provider.Equals("onedrive", StringComparison.OrdinalIgnoreCase))
            {
                tokenResult = await _oAuthService.ExchangeMicrosoftCodeAsync(Code, redirectUri, ct);
            }
            else if (Provider.Equals("googledrive", StringComparison.OrdinalIgnoreCase))
            {
                tokenResult = await _oAuthService.ExchangeGoogleCodeAsync(Code, redirectUri, ct);
            }
            else
            {
                ErrorMessage = "Unsupported OAuth provider.";
                return Page();
            }

            var request = new SaveOAuthTokensRequest(
                Provider,
                tokenResult.AccessToken,
                tokenResult.RefreshToken,
                tokenResult.AccountEmail,
                tokenResult.AccountName,
                tokenResult.ExpiresInSeconds
            );

            await _cpClient.SaveOAuthTokensAsync(session.TenantId.Value, request, ct);

            // Redirect back to backups on success
            return RedirectToPage("/Backups");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process OAuth callback for provider {Provider}", Provider);
            ErrorMessage = "An error occurred while connecting the storage provider. Please try again.";
            return Page();
        }
    }
}
