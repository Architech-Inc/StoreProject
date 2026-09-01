using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

using Microsoft.AspNetCore.RateLimiting;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants/{id:guid}/backups")]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly ILogger<BackupsController> _logger;

    public BackupsController(IBackupService backupService, ILogger<BackupsController> logger)
    {
        _backupService = backupService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var summary = await _backupService.GetBackupSummaryAsync(id, ct);
        if (summary == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<BackupSummaryDto>.Ok(summary));
    }

    [HttpPost("trigger")]
    [EnableRateLimiting("BackupTrigger")]
    public async Task<IActionResult> TriggerBackup(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _backupService.TriggerBackupNowAsync(id, ct);
            return Ok(ApiResponse<TriggerBackupResponse>.Ok(result, result.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering backup for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to execute snapshot backup."));
        }
    }

    [HttpPost("providers/s3")]
    public async Task<IActionResult> ConfigureS3(Guid id, [FromBody] ConfigureS3Request request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid S3 configuration."));
        }

        try
        {
            var result = await _backupService.ConfigureS3ProviderAsync(id, request, ct);
            return Ok(ApiResponse<BackupProviderDto>.Ok(result, "S3 storage provider connected successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring S3 for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to configure S3 provider."));
        }
    }

    [HttpPost("providers/oauth")]
    public async Task<IActionResult> SaveOAuthTokens(Guid id, [FromBody] SaveOAuthTokensRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid OAuth tokens."));
        }

        try
        {
            var result = await _backupService.SaveOAuthProviderAsync(id, request, ct);
            return Ok(ApiResponse<BackupProviderDto>.Ok(result, $"{request.ProviderType} connected successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving OAuth tokens for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to save OAuth provider."));
        }
    }

    [HttpDelete("providers/{provider}")]
    public async Task<IActionResult> DisconnectProvider(Guid id, string provider, CancellationToken ct)
    {
        var success = await _backupService.DisconnectProviderAsync(id, provider, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Provider not found or not connected."));
        }
        return Ok(ApiResponse<object>.Ok(null!, $"{provider} disconnected successfully."));
    }

    [HttpPut("schedule")]
    public async Task<IActionResult> UpdateSchedule(Guid id, [FromBody] UpdateScheduleRequest request, CancellationToken ct)
    {
        try
        {
            var schedule = await _backupService.UpdateScheduleAsync(id, request, ct);
            return Ok(ApiResponse<BackupScheduleDto>.Ok(schedule, "Backup schedule updated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating backup schedule for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to update backup schedule."));
        }
    }
}
