using Microsoft.AspNetCore.Mvc;
using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Requests;
using Store.API.Contracts;
using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRequestDispatcher _dispatcher;

    public AuthController(IRequestDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new LoginCommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                "invalid_credentials",
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("login/email")]
    public async Task<IActionResult> LoginWithEmail([FromBody] LoginWithEmailRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new LoginWithEmailCommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                "invalid_credentials",
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("login/phone")]
    public async Task<IActionResult> LoginWithPhone([FromBody] LoginWithPhoneRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new LoginWithPhoneCommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                "invalid_credentials",
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new RefreshTokenCommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                "invalid_refresh_token",
                "Invalid or expired refresh token.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));
        }

        await _dispatcher.SendAsync(new LogoutCommand(userId), ct);
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
    }

    [HttpPost("reset-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var success = await _dispatcher.SendAsync(new ResetPasswordCommand(request), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From(
                "invalid_credentials",
                "Current password is incorrect.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
    }
}
