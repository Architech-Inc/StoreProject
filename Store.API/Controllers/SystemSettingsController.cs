using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/settings")]
[ApiController]
[Authorize(Roles = "Admin")]
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
        if (value == null) return NotFound(ApiErrorResponse.From("not_found", "Setting not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<string>.Ok(value));
    }

    [HttpPut("{*key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Value)) return BadRequest(ApiErrorResponse.From("invalid_request", "Value is required.", traceId: HttpContext.TraceIdentifier));

        var success = await _systemSettings.UpdateSettingAsync(key, request.Value, ct);
        if (!success) return BadRequest(ApiErrorResponse.From("error", "Failed to update setting.", traceId: HttpContext.TraceIdentifier));
        
        return Ok(ApiResponse<string>.Ok(request.Value, "Setting updated successfully."));
    }
}

public class UpdateSettingRequest
{
    public string Value { get; set; } = string.Empty;
}
