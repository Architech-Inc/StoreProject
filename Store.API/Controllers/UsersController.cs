using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Application.Abstractions;
using Store.API.Application.Users.Requests;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
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
    private readonly IUserService _userService;

    public UsersController(IRequestDispatcher dispatcher, ISystemSettingService systemSettings, IUserService userService)
    {
        _dispatcher = dispatcher;
        _systemSettings = systemSettings;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
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

    [HttpGet("{id:guid}/360")]
    public async Task<IActionResult> Get360ById(Guid id, CancellationToken ct)
    {
        var user360 = await _userService.Get360ByIdAsync(id, ct);
        if (user360 is null)
        {
            return NotFound(ApiErrorResponse.From("not_found", "User not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<User360Dto>.Ok(user360));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var user = await _dispatcher.SendAsync(new CreateUserCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = user.UserId }, ApiResponse<UserDto>.Ok(user, "User created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
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
    [Authorize(Policy = PermissionKeys.AdminUsers)]
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
    [Authorize(Policy = PermissionKeys.AdminUsers)]
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

    [HttpPost("profile/2fa/disable")]
    public async Task<IActionResult> Disable2FA(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        var success = await _dispatcher.SendAsync(new Disable2FACommand(userId), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From("disable_2fa_failed", "Failed to disable 2FA.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "2FA disabled successfully."));
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

    [HttpPost("{id}/sessions/revoke")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> RevokeUserSessions(Guid id, CancellationToken ct)
    {
        var success = await _dispatcher.SendAsync(new RevokeAllSessionsCommand(id), ct);
        if (!success)
        {
            return BadRequest(ApiErrorResponse.From("revoke_failed", "Failed to revoke sessions.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<object>.Ok(null!, "All sessions revoked successfully."));
    }

    // --- Contact Change Endpoints ---
    [HttpPost("profile/contact-change")]
    public async Task<IActionResult> RequestContactChange([FromBody] CreateContactChangeDto request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiErrorResponse.From("unauthorized", "Unauthorized.", traceId: HttpContext.TraceIdentifier));

        try
        {
            var result = await _userService.RequestContactChangeAsync(userId, request, ct);
            return Ok(ApiResponse<ContactChangeRequestDto>.Ok(result, "Contact change requested. Please verify your new contact info."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiErrorResponse.From("pending_request_exists", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    [AllowAnonymous]
    [HttpGet("profile/contact-change/verify")]
    public async Task<IActionResult> VerifyContactChange([FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length < 10)
            return BadRequest(ApiErrorResponse.From("invalid_token", "Verification token is required."));

        var success = await _userService.VerifyContactChangeAsync(token.Trim(), ct);
        if (!success)
            return BadRequest(ApiErrorResponse.From("verification_failed", "Invalid or expired verification token."));

        return Ok(ApiResponse<object>.Ok(null!, "Contact information verified. Waiting for administrator approval."));
    }

    [HttpGet("contact-changes/pending")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> GetPendingContactChanges(CancellationToken ct)
    {
        var requests = await _userService.GetPendingContactChangesAsync(ct);
        return Ok(ApiResponse<IReadOnlyCollection<ContactChangeRequestDto>>.Ok(requests));
    }

    [HttpPost("contact-changes/{id}/approve")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> ApproveContactChange(Guid id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var adminId))
            return Unauthorized();

        var success = await _userService.ApproveContactChangeAsync(id, adminId, ct);
        if (!success)
            return BadRequest(ApiErrorResponse.From("approve_failed", "Failed to approve request or request not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Contact change approved successfully."));
    }

    [HttpPost("contact-changes/{id}/reject")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> RejectContactChange(Guid id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var adminId))
            return Unauthorized();

        var success = await _userService.RejectContactChangeAsync(id, adminId, ct);
        if (!success)
            return BadRequest(ApiErrorResponse.From("reject_failed", "Failed to reject request or request not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Contact change rejected successfully."));
    }

    [HttpPost("contact-changes/{id}/cancel")]
    public async Task<IActionResult> CancelContactChange(Guid id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var success = await _userService.CancelContactChangeAsync(id, userId, ct);
        if (!success)
            return BadRequest(ApiErrorResponse.From("cancel_failed", "Failed to cancel request or request not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Contact change cancelled successfully."));
    }

    [HttpGet("contact-changes/history")]
    [Authorize(Policy = PermissionKeys.AdminUsers)]
    public async Task<IActionResult> GetContactChangeHistory(CancellationToken ct)
    {
        var requests = await _userService.GetContactChangeHistoryAsync(ct);
        return Ok(ApiResponse<IReadOnlyCollection<ContactChangeRequestDto>>.Ok(requests));
    }
}
