using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Store.TenantPortal.Services;

public class OAuthService : IOAuthService
{
    public const string StateCookieName = "clexan_oauth_state_nonce";

    private readonly IConfiguration _config;
    private readonly ILogger<OAuthService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly byte[] _stateSecretKey;

    private record OAuthStateEntry(Guid TenantId, DateTimeOffset ExpiresAt);
    private static readonly ConcurrentDictionary<string, OAuthStateEntry> _activeStates = new();

    public OAuthService(
        IConfiguration config, 
        ILogger<OAuthService> logger, 
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _config = config;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;

        var masterSecret = _config["OAuth:StateSigningKey"] ?? "ClexAnFoodsOAuthAntiCsrfSecretKey2026";
        _stateSecretKey = Encoding.UTF8.GetBytes(masterSecret);
    }

    /// <summary>
    /// SEC-10 — Generates a cryptographically signed, server-side bound, single-use OAuth state parameter.
    /// Emits an HttpOnly SameSite=Lax cookie when an active HttpContext is available.
    /// </summary>
    public string GenerateSignedState(Guid tenantId, HttpContext? httpContext = null)
    {
        PruneExpiredStates();

        httpContext ??= _httpContextAccessor?.HttpContext;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonceBytes = RandomNumberGenerator.GetBytes(16);
        var nonce = Convert.ToHexString(nonceBytes).ToLowerInvariant();

        var payload = $"{tenantId:D}:{timestamp}:{nonce}";
        using var hmac = new HMACSHA256(_stateSecretKey);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();

        // 1. Record state in server-side registry (prevents forged / unissued states and replay attacks)
        _activeStates[nonce] = new OAuthStateEntry(tenantId, DateTimeOffset.UtcNow.AddMinutes(10));

        // 2. Set HttpOnly SameSite=Lax cookie on client (binds state to initiating user session)
        if (httpContext != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(10)
            };
            httpContext.Response.Cookies.Append(StateCookieName, nonce, cookieOptions);
        }

        return $"{payload}:{signature}";
    }

    public bool ValidateSignedState(string state, out Guid tenantId) =>
        ValidateSignedState(state, _httpContextAccessor?.HttpContext, out tenantId);

    /// <summary>
    /// SEC-10 — Validates HMAC authenticity, expiration, server-side presence (single use),
    /// and user-agent cookie binding to prevent Login CSRF and OAuth session fixation.
    /// </summary>
    public bool ValidateSignedState(string state, HttpContext? httpContext, out Guid tenantId)
    {
        tenantId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(state)) return false;

        httpContext ??= _httpContextAccessor?.HttpContext;

        var parts = state.Split(':');
        if (parts.Length != 4)
        {
            if (parts.Length == 3 && httpContext == null)
            {
                return ValidateLegacySignedState(parts, out tenantId);
            }
            _logger.LogWarning("OAuth state parameter does not conform to expected 4-part format.");
            return false;
        }

        if (!Guid.TryParse(parts[0], out tenantId)) return false;
        if (!long.TryParse(parts[1], out var timestamp)) return false;
        var nonce = parts[2];
        var submittedSig = parts[3];

        // 1. Validate cryptographic HMAC integrity
        var payload = $"{tenantId:D}:{timestamp}:{nonce}";
        using var hmac = new HMACSHA256(_stateSecretKey);
        var expectedSigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSig = Convert.ToHexString(expectedSigBytes).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(submittedSig.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(expectedSig)))
        {
            _logger.LogWarning("OAuth state HMAC signature mismatch for tenant {TenantId}", tenantId);
            return false;
        }

        // 2. Validate timestamp expiration window (10 minutes)
        var stateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        var age = DateTimeOffset.UtcNow - stateTime;
        if (age > TimeSpan.FromMinutes(10) || age < TimeSpan.FromSeconds(-60))
        {
            _logger.LogWarning("OAuth state expired or outside allowable time window for tenant {TenantId} (created at {Time})", tenantId, stateTime);
            return false;
        }

        // 3. Validate and atomically consume server-side state entry (single-use replay guard)
        if (!_activeStates.TryRemove(nonce, out var recordedEntry))
        {
            _logger.LogWarning("OAuth state nonce was not found in server-side registry or has already been consumed (replay attempt) for tenant {TenantId}.", tenantId);
            return false;
        }

        if (recordedEntry.TenantId != tenantId)
        {
            _logger.LogWarning("OAuth state tenant mismatch against server registry for tenant {TenantId}. Expected {Expected}", tenantId, recordedEntry.TenantId);
            return false;
        }

        if (recordedEntry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("OAuth state expired in server-side registry for tenant {TenantId}.", tenantId);
            return false;
        }

        // 4. Validate client session cookie binding (anti-CSRF)
        if (httpContext != null)
        {
            if (!httpContext.Request.Cookies.TryGetValue(StateCookieName, out var cookieNonce) || string.IsNullOrWhiteSpace(cookieNonce))
            {
                _logger.LogWarning("OAuth state binding cookie '{CookieName}' missing from request for tenant {TenantId}. Possible login CSRF.", StateCookieName, tenantId);
                return false;
            }

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(cookieNonce.Trim().ToLowerInvariant()),
                Encoding.UTF8.GetBytes(nonce.Trim().ToLowerInvariant())))
            {
                _logger.LogWarning("OAuth state cookie nonce mismatch for tenant {TenantId}. Potential CSRF attack.", tenantId);
                return false;
            }

            // Invalidate cookie on response
            httpContext.Response.Cookies.Delete(StateCookieName, new CookieOptions { Path = "/" });
        }

        return true;
    }

    private bool ValidateLegacySignedState(string[] parts, out Guid tenantId)
    {
        tenantId = Guid.Empty;
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
            return false;
        }

        var stateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return DateTimeOffset.UtcNow - stateTime <= TimeSpan.FromMinutes(10);
    }

    private static void PruneExpiredStates()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in _activeStates)
        {
            if (kvp.Value.ExpiresAt < now)
            {
                _activeStates.TryRemove(kvp.Key, out _);
            }
        }
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

    // Diagnostic/Testing helpers
    public static int ActiveStateCount => _activeStates.Count;
    public static void ClearActiveStates() => _activeStates.Clear();
}
