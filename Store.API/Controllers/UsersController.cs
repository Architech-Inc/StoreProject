using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Application.Abstractions;
using Store.API.Application.Users.Requests;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Users;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IRequestDispatcher _dispatcher;

    public UsersController(IRequestDispatcher dispatcher) => _dispatcher = dispatcher;

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
}
