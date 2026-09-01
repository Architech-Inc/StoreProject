using System.Security.Cryptography;
using System.Text;

namespace Store.TenantPortal.Services;

public class OAuthService : IOAuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<OAuthService> _logger;
    private readonly byte[] _stateSecretKey;

    public OAuthService(IConfiguration config, ILogger<OAuthService> logger)
    {
        _config = config;
        _logger = logger;
        var masterSecret = _config["OAuth:StateSigningKey"] ?? "ClexAnFoodsOAuthAntiCsrfSecretKey2026";
        _stateSecretKey = Encoding.UTF8.GetBytes(masterSecret);
    }

    public string GenerateSignedState(Guid tenantId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"{tenantId:D}:{timestamp}";
        using var hmac = new HMACSHA256(_stateSecretKey);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();
        return $"{payload}:{signature}";
    }

    public bool ValidateSignedState(string state, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(state)) return false;

        var parts = state.Split(':');
        if (parts.Length != 3) return false;

        if (!Guid.TryParse(parts[0], out tenantId)) return false;
        if (!long.TryParse(parts[1], out var timestamp)) return false;

        var payload = $"{tenantId:D}:{timestamp}";
        using var hmac = new HMACSHA256(_stateSecretKey);
        var expectedSigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSig = Convert.ToHexString(expectedSigBytes).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(parts[2].ToLowerInvariant()),
            Encoding.UTF8.GetBytes(expectedSig)))
        {
            _logger.LogWarning("OAuth state HMAC signature mismatch for tenant {TenantId}", tenantId);
            return false;
        }

        // Validate timestamp expiration (10 minutes)
        var stateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        if (DateTimeOffset.UtcNow - stateTime > TimeSpan.FromMinutes(10))
        {
            _logger.LogWarning("OAuth state expired for tenant {TenantId} (created at {Time})", tenantId, stateTime);
            return false;
        }

        return true;
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
            "Google Drive (App Space)",
            3600
        ));
    }
}
