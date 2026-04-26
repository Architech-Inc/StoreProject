using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _auth;

    public AuthController(IAuthenticationService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        if (result is null) return Unauthorized(ApiResponse<object>.Fail("Invalid credentials."));
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("login/email")]
    public async Task<IActionResult> LoginWithEmail([FromBody] LoginWithEmailRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginWithEmailAsync(request, ct);
        if (result is null) return Unauthorized(ApiResponse<object>.Fail("Invalid credentials."));
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("login/phone")]
    public async Task<IActionResult> LoginWithPhone([FromBody] LoginWithPhoneRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginWithPhoneAsync(request, ct);
        if (result is null) return Unauthorized(ApiResponse<object>.Fail("Invalid credentials."));
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _auth.RefreshTokenAsync(request, ct);
        if (result is null) return Unauthorized(ApiResponse<object>.Fail("Invalid or expired refresh token."));
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _auth.LogoutAsync(userId, ct);
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
    }

    [HttpPost("reset-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var success = await _auth.ResetPasswordAsync(request, ct);
        if (!success) return BadRequest(ApiResponse<object>.Fail("Current password is incorrect."));
        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
    }
}
