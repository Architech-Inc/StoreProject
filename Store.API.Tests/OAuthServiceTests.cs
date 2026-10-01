using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Store.TenantPortal.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 35 — Security unit tests for <see cref="OAuthService"/> (SEC-10).
/// Verifies cryptographically signed OAuth states, server-side registry tracking,
/// single-use replay protection, and HttpContext session cookie anti-CSRF binding.
/// </summary>
public class OAuthServiceTests : IDisposable
{
    private readonly IConfiguration _config;
    private readonly OAuthService _service;
    private const string TestKey = "TestSecretKeyForOAuthStateSigningAtLeast32Bytes!";

    public OAuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "OAuth:StateSigningKey", TestKey },
            { "OAuth:Microsoft:ClientId", "ms-client-123" },
            { "OAuth:Google:ClientId", "google-client-456" }
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _service = new OAuthService(_config, NullLogger<OAuthService>.Instance);
        OAuthService.ClearActiveStates();
    }

    public void Dispose()
    {
        OAuthService.ClearActiveStates();
    }

    [Fact]
    public void GenerateSignedState_Generates4PartState_WithValidNonceAndSignature()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var state = _service.GenerateSignedState(tenantId);

        // Assert
        Assert.NotNull(state);
        var parts = state.Split(':');
        Assert.Equal(4, parts.Length);

        Assert.True(Guid.TryParse(parts[0], out var parsedTenant));
        Assert.Equal(tenantId, parsedTenant);

        Assert.True(long.TryParse(parts[1], out var timestamp));
        Assert.True(timestamp > 0);

        Assert.Equal(32, parts[2].Length); // 16 bytes = 32 hex chars
        Assert.Equal(64, parts[3].Length); // HMAC-SHA256 = 64 hex chars
    }

    [Fact]
    public void GenerateSignedState_WithHttpContext_AppendsBindingCookie()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();

        // Act
        var state = _service.GenerateSignedState(tenantId, httpContext);

        // Assert
        var parts = state.Split(':');
        var nonce = parts[2];

        var setCookieHeaders = httpContext.Response.Headers.SetCookie.ToString();
        Assert.Contains(OAuthService.StateCookieName, setCookieHeaders);
        Assert.Contains(nonce, setCookieHeaders);
        Assert.Contains("samesite=lax", setCookieHeaders, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", setCookieHeaders, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateSignedState_ValidCookieAndServerRecord_ReturnsTrue_AndConsumesNonce()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var state = _service.GenerateSignedState(tenantId, httpContext);

        // Extract nonce from response cookie and set on request cookie to simulate browser redirect
        var parts = state.Split(':');
        var nonce = parts[2];
        httpContext.Request.Headers.Cookie = $"{OAuthService.StateCookieName}={nonce}";

        // Act - First validation succeeds
        var isValid = _service.ValidateSignedState(state, httpContext, out var validatedTenant);

        // Assert
        Assert.True(isValid);
        Assert.Equal(tenantId, validatedTenant);

        // Act - Replay attempt must fail because nonce was consumed
        var isReplayValid = _service.ValidateSignedState(state, httpContext, out _);
        Assert.False(isReplayValid);
    }

    [Fact]
    public void ValidateSignedState_TamperedSignature_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var state = _service.GenerateSignedState(tenantId, httpContext);
        var parts = state.Split(':');
        var nonce = parts[2];
        httpContext.Request.Headers.Cookie = $"{OAuthService.StateCookieName}={nonce}";

        var tamperedSignatureState = $"{parts[0]}:{parts[1]}:{parts[2]}:0000000000000000000000000000000000000000000000000000000000000000";

        // Act
        var isValid = _service.ValidateSignedState(tamperedSignatureState, httpContext, out _);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignedState_MissingOrMismatchedCookie_ReturnsFalse_PreventingCsrf()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var state = _service.GenerateSignedState(tenantId, httpContext);

        // Case 1: Missing cookie
        var isValidWithoutCookie = _service.ValidateSignedState(state, httpContext, out _);
        Assert.False(isValidWithoutCookie);

        // Case 2: Mismatched cookie (attacker initiated flow on different machine)
        httpContext.Request.Headers.Cookie = $"{OAuthService.StateCookieName}=attacker_stolen_nonce_1234567890";
        var isValidWithMismatchedCookie = _service.ValidateSignedState(state, httpContext, out _);
        Assert.False(isValidWithMismatchedCookie);
    }

    [Fact]
    public void ValidateSignedState_ForgedStateNotIssuedByServer_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var forgedNonce = "deadbeefcafebabe1122334455667788";

        // Even with valid HMAC signature
        var payload = $"{tenantId:D}:{timestamp}:{forgedNonce}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestKey));
        var sig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        var forgedState = $"{payload}:{sig}";

        httpContext.Request.Headers.Cookie = $"{OAuthService.StateCookieName}={forgedNonce}";

        // Act
        var isValid = _service.ValidateSignedState(forgedState, httpContext, out _);

        // Assert - must fail because forgedNonce was never registered in _activeStates
        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSignedState_ExpiredTimestamp_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var httpContext = new DefaultHttpContext();
        var expiredTimestamp = DateTimeOffset.UtcNow.AddMinutes(-15).ToUnixTimeSeconds();
        var nonce = "1234567890abcdef1234567890abcdef";

        var payload = $"{tenantId:D}:{expiredTimestamp}:{nonce}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestKey));
        var sig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        var expiredState = $"{payload}:{sig}";

        httpContext.Request.Headers.Cookie = $"{OAuthService.StateCookieName}={nonce}";

        // Act
        var isValid = _service.ValidateSignedState(expiredState, httpContext, out _);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void BuildMicrosoftAuthUrl_IncludesClientIdRedirectUriAndState()
    {
        // Act
        var url = _service.BuildMicrosoftAuthUrl("sample_state", "https://portal.example.com/callback");

        // Assert
        Assert.Contains("login.microsoftonline.com", url);
        Assert.Contains("client_id=ms-client-123", url);
        Assert.Contains("state=sample_state", url);
        Assert.Contains("redirect_uri=https%3A%2F%2Fportal.example.com%2Fcallback", url);
    }

    [Fact]
    public void BuildGoogleAuthUrl_IncludesClientIdRedirectUriAndState()
    {
        // Act
        var url = _service.BuildGoogleAuthUrl("sample_state", "https://portal.example.com/callback");

        // Assert
        Assert.Contains("accounts.google.com", url);
        Assert.Contains("client_id=google-client-456", url);
        Assert.Contains("state=sample_state", url);
        Assert.Contains("redirect_uri=https%3A%2F%2Fportal.example.com%2Fcallback", url);
    }
}
