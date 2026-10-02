using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Services;
using Store.Models.Common;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Tenant;

namespace Store.ControlPlane.Controllers;

/// <summary>
/// MT-04 — Controller for managing per-tenant custom SMTP mail relay credentials, rotation, and live delivery testing.
/// </summary>
[ApiController]
[Route("api/control/tenants/{id:guid}/smtp")]
public class SmtpController : ControllerBase
{
    private readonly ITenantSmtpService _smtpService;
    private readonly ILogger<SmtpController> _logger;

    public SmtpController(ITenantSmtpService smtpService, ILogger<SmtpController> logger)
    {
        _smtpService = smtpService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSmtpConfig(Guid id, CancellationToken ct)
    {
        var config = await _smtpService.GetSmtpConfigAsync(id, ct);
        if (config == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }

        return Ok(ApiResponse<TenantSmtpConfigDto>.Ok(config));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSmtpConfig(Guid id, [FromBody] UpdateTenantSmtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid SMTP configuration payload."));
        }

        try
        {
            var updated = await _smtpService.UpdateSmtpConfigAsync(id, request, ct);
            return Ok(ApiResponse<TenantSmtpConfigDto>.Ok(updated, "Tenant SMTP configuration updated successfully."));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Quota / tier validation failure updating SMTP for tenant {TenantId}", id);
            return StatusCode(402, ApiErrorResponse.From(ErrorCode.QuotaExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update SMTP configuration for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to update SMTP configuration."));
        }
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestSmtpConnection(Guid id, [FromBody] TestSmtpRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid recipient email for test."));
        }

        try
        {
            var result = await _smtpService.TestSmtpConnectionAsync(id, request, ct);
            if (!result.Success)
            {
                return BadRequest(ApiResponse<TestSmtpResponse>.Fail(result.Message));
            }

            return Ok(ApiResponse<TestSmtpResponse>.Ok(result, "SMTP connection verified successfully."));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(402, ApiErrorResponse.From(ErrorCode.QuotaExceeded, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMTP connection for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to test SMTP connection."));
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ResetSmtpConfig(Guid id, CancellationToken ct)
    {
        var success = await _smtpService.ResetSmtpConfigAsync(id, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }

        return Ok(ApiResponse<object>.Ok(null!, "SMTP configuration reset to platform defaults."));
    }
}
