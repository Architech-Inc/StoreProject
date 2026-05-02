using Store.Models.DTOs.Auth;

namespace Store.API.Application.Auth.Ports;

public interface IAuthPort
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<LoginResponse?> LoginWithEmailAsync(LoginWithEmailRequest request, CancellationToken ct = default);
    Task<LoginResponse?> LoginWithPhoneAsync(LoginWithPhoneRequest request, CancellationToken ct = default);
    Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}
