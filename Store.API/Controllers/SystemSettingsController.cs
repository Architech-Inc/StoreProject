using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/settings")]
[ApiController]
[Authorize(Policy = PermissionKeys.AdminSettings)]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingService _systemSettings;

    public SystemSettingsController(ISystemSettingService systemSettings)
    {
        _systemSettings = systemSettings;
    }

    [HttpGet("{*key}")]
    public async Task<IActionResult> GetSetting(string key, CancellationToken ct)
    {
        // For keys containing colons like "Auth:PasswordRecoveryMethod", ASP.NET core routing might need encoded values, 
        // but typically it handles them fine. If issues arise, we can pass it as a query param or body.
        // Actually, {*key} is safer for keys with colons or slashes.
        var value = await _systemSettings.GetSettingAsync(key, ct);
        if (value == null) return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Setting not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<string>.Ok(value));
    }

    [HttpPut("{*key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest(ApiErrorResponse.From(ErrorCode.InvalidRequest, "Value is required.", traceId: HttpContext.TraceIdentifier));
        }

        var result = await _systemSettings.UpdateSettingAsync(key, request.Value, ct);
        if (!result.Success)
        {
            // Surface the actual reason (constraint violation, missing key,
            // connection failure, etc.) so the operator can act on it.
            return BadRequest(ApiErrorResponse.From(
                code: "update_failed",
                message: $"Failed to update setting '{key}': {result.FailureReason}",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<string>.Ok(request.Value, "Setting updated successfully."));
    }
}

public class UpdateSettingRequest
{
    public string Value { get; set; } = string.Empty;
}
