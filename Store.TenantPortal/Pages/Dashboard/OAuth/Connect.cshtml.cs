using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages.Dashboard.OAuth;

[Authorize]
public class ConnectModel : PageModel
{
    private readonly IOAuthService _oAuthService;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<ConnectModel> _logger;

    public ConnectModel(IOAuthService oAuthService, IPortalSessionService sessionService, ILogger<ConnectModel> logger)
    {
        _oAuthService = oAuthService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public IActionResult OnGet(string provider)
    {
        var session = _sessionService.GetCurrentSession(User);
        if (session == null || !session.TenantId.HasValue) return RedirectToPage("/Login");

        var state = _oAuthService.GenerateSignedState(session.TenantId.Value);
        // We embed the provider in the state or we rely on the callback route. 
        // We can just embed provider inside a cookie or use specific redirect URIs.
        // But our callback will be /Dashboard/OAuth/Callback/{provider}
        
        var redirectUri = Url.PageLink("/Dashboard/OAuth/Callback", values: new { provider = provider })!;

        string authUrl = provider?.ToLowerInvariant() switch
        {
            "onedrive" => _oAuthService.BuildMicrosoftAuthUrl(state, redirectUri),
            "googledrive" => _oAuthService.BuildGoogleAuthUrl(state, redirectUri),
            _ => null
        };

        if (authUrl == null)
        {
            return BadRequest("Invalid provider specified.");
        }

        _logger.LogInformation("Redirecting user to {Provider} OAuth authorization endpoint.", provider);
        return Redirect(authUrl);
    }
}
