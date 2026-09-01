namespace Store.TenantPortal.Services;

public interface IOAuthService
{
    string GenerateSignedState(Guid tenantId);
    bool ValidateSignedState(string state, out Guid tenantId);
    string BuildMicrosoftAuthUrl(string state, string redirectUri);
    string BuildGoogleAuthUrl(string state, string redirectUri);
    Task<OAuthTokenResult> ExchangeMicrosoftCodeAsync(string code, string redirectUri, CancellationToken ct = default);
    Task<OAuthTokenResult> ExchangeGoogleCodeAsync(string code, string redirectUri, CancellationToken ct = default);
}

public record OAuthTokenResult(
    string AccessToken,
    string RefreshToken,
    string AccountEmail,
    string? AccountName,
    int ExpiresInSeconds
);
