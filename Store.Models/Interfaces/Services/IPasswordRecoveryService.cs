using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Users;

namespace Store.Models.Interfaces.Services;

public interface IPasswordRecoveryService
{
    /// <summary>
    /// Generates an OTP for the given username and sends it (Method A).
    /// </summary>
    Task<bool> RequestOtpAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Validates an OTP and returns a signed PasswordResetToken string if successful.
    /// </summary>
    Task<string?> VerifyOtpAsync(string username, string otpCode, CancellationToken ct = default);

    /// <summary>
    /// Issues a temporary password for the user and forces a password change on next login (Method B).
    /// </summary>
    Task<string> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Resets the user's password using a valid PasswordResetToken string.
    /// </summary>
    Task<bool> ResetPasswordWithTokenAsync(RecoverPasswordWithTokenRequest request, CancellationToken ct = default);
}
