using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;

namespace StoreUI.Services;

public interface IApiPasswordRecoveryService
{
    Task<bool> RequestOtpAsync(string username, CancellationToken ct = default);
    Task<string?> VerifyOtpAsync(string username, string otpCode, CancellationToken ct = default);
    Task<bool> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken ct = default);
}

public class ApiPasswordRecoveryService : IApiPasswordRecoveryService
{
    private readonly IApiClientService _client;

    public ApiPasswordRecoveryService(IApiClientService client)
    {
        _client = client;
    }

    public async Task<bool> RequestOtpAsync(string username, CancellationToken ct = default)
    {
        var request = new RequestOtpRequest { Username = username };
        try 
        {
            await _client.PostAsync<object>("/api/auth/recovery/request", request, ct);
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public async Task<string?> VerifyOtpAsync(string username, string otpCode, CancellationToken ct = default)
    {
        var request = new VerifyOtpRequest { Username = username, OtpCode = otpCode };
        try 
        {
            // The API returns { success: true, resetToken: "..." }
            // Since we need just the token, we can use a dynamic or specific class.
            var response = await _client.PostAsync<VerifyOtpResponse>("/api/auth/recovery/verify", request, ct);
            return response?.ResetToken;
        }
        catch 
        {
            return null;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, string confirmPassword, CancellationToken ct = default)
    {
        var request = new RecoverPasswordWithTokenRequest 
        { 
            Token = token, 
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };
        try 
        {
            await _client.PostAsync<object>("/api/auth/recovery/reset", request, ct);
            return true;
        }
        catch 
        {
            return false;
        }
    }
}

public class VerifyOtpResponse 
{
    public bool Success { get; set; }
    public string ResetToken { get; set; } = string.Empty;
}
