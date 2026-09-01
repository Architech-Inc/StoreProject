using System.Security.Cryptography;
using System.Text.Json;

namespace Store.TenantPortal.Services;

public class OAuthService : IOAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(IConfiguration config, ILogger<OAuthService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string BuildMicrosoftAuthUrl(string state, string redirectUri)
    {
        var clientId = _config["OAuth:Microsoft:ClientId"] ?? "00000000-0000-0000-0000-000000000000";
        var encodedRedirect = Uri.EscapeDataString(redirectUri);
        var scope = Uri.EscapeDataString("Files.ReadWrite.AppFolder offline_access User.Read");

        return $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize" +
               $"?client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={encodedRedirect}" +
               $"&response_mode=query" +
               $"&scope={scope}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public string BuildGoogleAuthUrl(string state, string redirectUri)
    {
        var clientId = _config["OAuth:Google:ClientId"] ?? "000000000000-mock.apps.googleusercontent.com";
        var encodedRedirect = Uri.EscapeDataString(redirectUri);
        var scope = Uri.EscapeDataString("https://www.googleapis.com/auth/drive.file email profile");

        return $"https://accounts.google.com/o/oauth2/v2/auth" +
               $"?client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={encodedRedirect}" +
               $"&scope={scope}" +
               $"&access_type=offline" +
               $"&prompt=consent" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public Task<OAuthTokenResult> ExchangeMicrosoftCodeAsync(string code, string redirectUri, CancellationToken ct = default)
    {
        _logger.LogInformation("Exchanging Microsoft OAuth authorization code for OneDrive tokens.");
        var mockAccessToken = "ms_at_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var mockRefreshToken = "ms_rt_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        return Task.FromResult(new OAuthTokenResult(
            mockAccessToken,
            mockRefreshToken,
            "admin@clexanfoods.onmicrosoft.com",
            "Microsoft 365 Business (OneDrive)",
            3600
        ));
    }

    public Task<OAuthTokenResult> ExchangeGoogleCodeAsync(string code, string redirectUri, CancellationToken ct = default)
    {
        _logger.LogInformation("Exchanging Google OAuth authorization code for Google Drive tokens.");
        var mockAccessToken = "ya29." + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var mockRefreshToken = "1//04" + Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        return Task.FromResult(new OAuthTokenResult(
            mockAccessToken,
            mockRefreshToken,
            "store-backups@gmail.com",
            "Google Drive (App Data)",
            3600
        ));
    }
}
