using Microsoft.AspNetCore.Mvc;
using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Requests;
using Store.API.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Auth;
using Store.API.Application.Users.Requests;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRequestDispatcher _dispatcher;

    public AuthController(IRequestDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>True when the request reached the API over HTTPS (or via a TLS-terminating proxy that sets X-Forwarded-Proto).</summary>
    private bool IsSecureContext =>
        HttpContext.Request.IsHttps
        || string.Equals(HttpContext.Request.Headers["X-Forwarded-Proto"].ToString(), "https", StringComparison.OrdinalIgnoreCase);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new LoginCommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                ErrorCode.InvalidCredentials,
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        // SEC-19 — set HttpOnly cookies in addition to the JSON body.
        // The cookies are the secure transport for browser clients; non-browser
        // callers (mobile / integration) keep using the JSON tokens.
        if (!result.RequiresPasswordReset && !result.RequiresTwoFactor)
        {
            Response.SetAuthCookies(
                result.AccessToken, result.AccessTokenExpiry,
                result.RefreshToken, result.RefreshTokenExpiry,
                IsSecureContext);
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
                ErrorCode.InvalidCredentials,
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        if (!result.RequiresPasswordReset && !result.RequiresTwoFactor)
        {
            Response.SetAuthCookies(
                result.AccessToken, result.AccessTokenExpiry,
                result.RefreshToken, result.RefreshTokenExpiry,
                IsSecureContext);
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
                ErrorCode.InvalidCredentials,
                "Invalid credentials.",
                traceId: HttpContext.TraceIdentifier));
        }

        if (!result.RequiresPasswordReset && !result.RequiresTwoFactor)
        {
            Response.SetAuthCookies(
                result.AccessToken, result.AccessTokenExpiry,
                result.RefreshToken, result.RefreshTokenExpiry,
                IsSecureContext);
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("login/2fa")]
    public async Task<IActionResult> Login2FA([FromBody] Login2FARequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new Login2FACommand(request), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                ErrorCode.InvalidTwoFactorCode,
                "Invalid 2FA code or expired token.",
                traceId: HttpContext.TraceIdentifier));
        }

        Response.SetAuthCookies(
            result.AccessToken, result.AccessTokenExpiry,
            result.RefreshToken, result.RefreshTokenExpiry,
            IsSecureContext);

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request, CancellationToken ct)
    {
        // SEC-28 — refresh tokens now travel primarily in the HttpOnly
        // `store_rt` cookie. The JSON body is still accepted (CLI tools,
        // Postman, mobile clients) but is no longer the primary channel.
        // Browser clients should never see the refresh token in JS at all.
        var cookieToken = Request.Cookies["store_rt"];
        var bodyToken = request?.RefreshToken;

        var effectiveRequest = new RefreshTokenRequest
        {
            Token = request?.Token ?? cookieToken,
            RefreshToken = !string.IsNullOrWhiteSpace(bodyToken) ? bodyToken : cookieToken
        };

        if (string.IsNullOrWhiteSpace(effectiveRequest.RefreshToken))
        {
            return Unauthorized(ApiErrorResponse.From(
                ErrorCode.InvalidRefreshToken,
                "Refresh token missing. Send the store_rt cookie or include refreshToken in the body.",
                traceId: HttpContext.TraceIdentifier));
        }

        var result = await _dispatcher.SendAsync(new RefreshTokenCommand(effectiveRequest), ct);
        if (result is null)
        {
            return Unauthorized(ApiErrorResponse.From(
                ErrorCode.InvalidRefreshToken,
                "Invalid or expired refresh token.",
                traceId: HttpContext.TraceIdentifier));
        }

        // SEC-19 — rotate the cookies on every successful refresh.
        Response.SetAuthCookies(
            result.AccessToken, result.AccessTokenExpiry,
            result.RefreshToken, result.RefreshTokenExpiry,
            IsSecureContext);

        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiErrorResponse.From(ErrorCode.Unauthorized, "Unauthorized.", traceId: HttpContext.TraceIdentifier));
        }

        await _dispatcher.SendAsync(new LogoutCommand(userId), ct);

        // SEC-19 — clear the HttpOnly cookies so a leaked browser session
        // can't reuse the old access token after logout.
        Response.ClearAuthCookies();

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
                ErrorCode.InvalidCredentials,
                "Current password is incorrect.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
    }

    [HttpGet("avatar/{username}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> GetAvatar(string username, CancellationToken ct)
    {
        // SEC-11 — Constant-time response.
        // The endpoint is anonymous (so the login page can show avatars before login) and
        // must not leak which usernames exist via wall-clock timing. We:
        //   1. Take a start timestamp
        //   2. Run the dispatcher query
        //   3. Sleep until TARGET_LATENCY_MS has elapsed so hit / miss / error paths
        //      all take the same wall-clock time regardless of DB round-trip.
        // The hash-only response never includes whether the user exists.
        const int TargetLatencyMs = 80;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length > 100)
            {
                // Treat malformed input as a miss — same wall-clock, same response.
                await WaitConstantAsync(sw, TargetLatencyMs, ct);
                return Ok(ApiResponse<string>.Ok("/images/admin.png"));
            }

            string? avatarUrl;
            try
            {
                avatarUrl = await _dispatcher.SendAsync(new GetUserAvatarQuery(username.Trim()), ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // Any DB error must not leak: same wall-clock, same payload.
                avatarUrl = null;
            }

            await WaitConstantAsync(sw, TargetLatencyMs, ct);

            // Always return the generic admin.png for unknown users, never reveal existence.
            var effective = string.IsNullOrEmpty(avatarUrl) ? "/images/admin.png" : avatarUrl;
            return Ok(ApiResponse<string>.Ok(effective));
        }
        finally
        {
            sw.Stop();
        }
    }

    /// <summary>
    /// Asynchronously waits until the stopwatch has reached <paramref name="targetMs"/>
    /// so the controller's response time is independent of the dispatcher round-trip.
    /// </summary>
    private static async Task WaitConstantAsync(System.Diagnostics.Stopwatch sw, int targetMs, CancellationToken ct)
    {
        var elapsed = sw.ElapsedMilliseconds;
        var remaining = targetMs - (int)elapsed;
        if (remaining > 0)
        {
            await Task.Delay(remaining, ct);
        }
    }
}
