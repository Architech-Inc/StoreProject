using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Store.API.Controllers;

[Route("api/auth/recovery")]
[ApiController]
[AllowAnonymous]
[EnableRateLimiting("password-recovery")]
public class PasswordRecoveryController : ControllerBase
{
    private readonly IPasswordRecoveryService _passwordRecoveryService;
    private readonly ILogger<PasswordRecoveryController> _logger;
    private readonly ISystemSettingService _systemSettings;

    public PasswordRecoveryController(IPasswordRecoveryService passwordRecoveryService, ILogger<PasswordRecoveryController> logger, ISystemSettingService systemSettings)
    {
        _passwordRecoveryService = passwordRecoveryService;
        _logger = logger;
        _systemSettings = systemSettings;
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var method = await _systemSettings.GetSettingAsync("Auth:PasswordRecoveryMethod", ct) ?? "Both";
        if (method.Equals("TempPassword", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { success = false, message = "OTP recovery is disabled." });
        }

        var result = await _passwordRecoveryService.RequestOtpAsync(request.Username, ct);
        // We always return Ok to prevent user enumeration attacks
        return Ok(new { success = true, message = "If the username exists, an OTP has been sent." });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var token = await _passwordRecoveryService.VerifyOtpAsync(request.Username, request.OtpCode, ct);
        
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { success = false, message = "Invalid or expired OTP." });
        }

        return Ok(new { success = true, resetToken = token });
    }

    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword([FromBody] RecoverPasswordWithTokenRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var success = await _passwordRecoveryService.ResetPasswordWithTokenAsync(request, ct);
        
        if (!success)
        {
            return BadRequest(new { success = false, message = "Invalid or expired token." });
        }

        return Ok(new { success = true, message = "Password reset successfully." });
    }
}
