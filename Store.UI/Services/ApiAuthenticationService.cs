using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

/// <summary>
/// Wraps IAuthenticationService to call the Store.API /api/auth endpoints
/// </summary>
public class ApiAuthenticationService : IAuthenticationService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiAuthenticationService> _logger;

    public ApiAuthenticationService(IApiClientService client, ILogger<ApiAuthenticationService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Logging in user: {Username}", request.Username);
        return await _client.PostAsync<LoginResponse>("/api/auth/login", request, ct);
    }

    public async Task<LoginResponse?> LoginWithEmailAsync(LoginWithEmailRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Logging in with email: {Email}", request.Email);
        return await _client.PostAsync<LoginResponse>("/api/auth/login/email", request, ct);
    }

    public async Task<LoginResponse?> LoginWithPhoneAsync(LoginWithPhoneRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Logging in with phone: {Phone}", request.Phone);
        return await _client.PostAsync<LoginResponse>("/api/auth/login/phone", request, ct);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Refreshing authentication token");
        return await _client.PostAsync<LoginResponse>("/api/auth/refresh", request, ct);
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("User {UserId} logging out", userId);
        return await _client.PostAsync("/api/auth/logout", null, ct);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Resetting password for user");
        var result = await _client.PostAsync<bool?>("/api/auth/reset-password", request, ct);
        return result.HasValue && result.Value;
    }
}
