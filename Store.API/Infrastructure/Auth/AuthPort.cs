using Store.API.Application.Auth.Ports;
using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;

namespace Store.API.Infrastructure.Auth;

public class AuthPort : IAuthPort
{
    private readonly IAuthenticationService _authService;

    public AuthPort(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => _authService.LoginAsync(request, ct);

    public Task<LoginResponse?> LoginWithEmailAsync(LoginWithEmailRequest request, CancellationToken ct = default)
        => _authService.LoginWithEmailAsync(request, ct);

    public Task<LoginResponse?> LoginWithPhoneAsync(LoginWithPhoneRequest request, CancellationToken ct = default)
        => _authService.LoginWithPhoneAsync(request, ct);

    public Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        => _authService.RefreshTokenAsync(request, ct);

    public Task LogoutAsync(Guid userId, CancellationToken ct = default)
        => _authService.LogoutAsync(userId, ct);

    public Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => _authService.ResetPasswordAsync(request, ct);
}
