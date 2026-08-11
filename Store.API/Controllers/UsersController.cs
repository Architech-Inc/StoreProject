using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Application.Abstractions;
using Store.API.Application.Users.Requests;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IRequestDispatcher _dispatcher;
    private readonly ISystemSettingService _systemSettings;

    public UsersController(IRequestDispatcher dispatcher, ISystemSettingService systemSettings)
    {
        _dispatcher = dispatcher;
        _systemSettings = systemSettings;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _dispatcher.SendAsync(new GetUsersQuery(request), ct);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _dispatcher.SendAsync(new GetUserByIdQuery(id), ct);
        if (user is null)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await _dispatcher.SendAsync(new CreateUserCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, ApiResponse<UserDto>.Ok(user, "User created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await _dispatcher.SendAsync(new UpdateUserCommand(id, request), ct);
        if (user is null)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _dispatcher.SendAsync(new DeleteUserCommand(id), ct);
        if (!deleted)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "User deactivated."));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var success = await _dispatcher.SendAsync(new ChangeUserPasswordCommand(userId, request), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From(
                "invalid_credentials",
                "Current password is incorrect.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Password changed."));
    }

    [HttpPut("profile/avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var updateRequest = new UpdateUserRequest
        {
            ThumbnailUrl = request.ThumbnailUrl,
            FullImageUrl = request.FullImageUrl
        };

        var user = await _dispatcher.SendAsync(new UpdateUserCommand(userId, updateRequest), ct);
        if (user is null)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<UserDto>.Ok(user, "Avatar updated."));
    }

    [HttpPut("profile/contacts")]
    public async Task<IActionResult> UpdateContacts([FromBody] UpdateUserContactsRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var success = await _dispatcher.SendAsync(new UpdateUserContactsCommand(userId, request), ct);
        if (!success)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "Contacts updated."));
    }

    [HttpPost("{id:guid}/issue-temp-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> IssueTempPassword(Guid id, CancellationToken ct)
    {
        var method = await _systemSettings.GetSettingAsync("Auth:PasswordRecoveryMethod", ct) ?? "Both";
        if (method.Equals("OTP", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiErrorResponse.From("disabled", "Temporary password issuing is disabled by system settings.", traceId: HttpContext.TraceIdentifier));
        }

        var tempPassword = await _dispatcher.SendAsync(new IssueTempPasswordCommand(id), ct);
        if (string.IsNullOrEmpty(tempPassword))
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found or unable to issue password.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<string>.Ok(tempPassword, "Temporary password issued. User must change it on next login."));
    }

    [HttpPost("profile/2fa/enable")]
    public async Task<IActionResult> Enable2FA(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var result = await _dispatcher.SendAsync(new Enable2FACommand(userId), ct);
        return Ok(ApiResponse<Enable2FAResponse>.Ok(result, "2FA setup initiated."));
    }

    [HttpPost("profile/2fa/verify")]
    public async Task<IActionResult> Verify2FA([FromBody] Verify2FARequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var success = await _dispatcher.SendAsync(new Verify2FACommand(userId, request), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From("invalid_code", "Invalid 2FA code.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "2FA verified and enabled successfully."));
    }

    [HttpGet("profile/activity")]
    public async Task<IActionResult> GetRecentActivity(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var result = await _dispatcher.SendAsync(new GetRecentActivityQuery(userId), ct);
        return Ok(ApiResponse<IReadOnlyCollection<AuditLogDto>>.Ok(result));
    }

    [HttpPost("profile/sessions/revoke")]
    public async Task<IActionResult> RevokeAllSessions(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var success = await _dispatcher.SendAsync(new RevokeAllSessionsCommand(userId), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From("revoke_failed", "Failed to revoke sessions.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "All sessions revoked successfully."));
    }
}
