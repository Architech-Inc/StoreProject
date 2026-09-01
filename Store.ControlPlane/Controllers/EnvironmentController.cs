using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants/{id:guid}/environment")]
public class EnvironmentController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;
    private readonly ILogger<EnvironmentController> _logger;

    public EnvironmentController(ITenantOrchestrator orchestrator, ILogger<EnvironmentController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var status = await _orchestrator.GetEnvironmentStatusAsync(id, ct);
        if (status == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<EnvironmentStatusDto>.Ok(status));
    }

    [HttpPost("restart/{service}")]
    public async Task<IActionResult> RestartService(Guid id, string service, CancellationToken ct)
    {
        try
        {
            bool success;
            if (string.Equals(service, "all", StringComparison.OrdinalIgnoreCase))
            {
                success = await _orchestrator.RestartAllContainersAsync(id, ct);
            }
            else
            {
                success = await _orchestrator.RestartContainerAsync(id, service, ct);
            }

            if (!success)
            {
                return NotFound(ApiResponse<object>.Fail("Tenant not found."));
            }

            return Ok(ApiResponse<object>.Ok(null!, $"Restart command dispatched for service '{service}'."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting service {Service} for tenant {TenantId}", service, id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to restart service."));
        }
    }

    [HttpPost("suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.SuspendTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Silo suspended successfully."));
    }

    [HttpPost("resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.ResumeTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Silo resumed successfully."));
    }
}
