using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants")]
public class TenantsController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;

    public TenantsController(ITenantOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _orchestrator.GetAllTenantsAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TenantDto>>.Ok(list));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var summary = await _orchestrator.GetHealthSummaryAsync(ct);
        return Ok(ApiResponse<TenantHealthSummaryDto>.Ok(summary));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.GetTenantDetailsAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDetailDto>.Ok(tenant));
    }

    [HttpPost("provision")]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantRequest request, CancellationToken ct)
    {
        try
        {
            var tenant = await _orchestrator.ProvisionTenantAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = tenant.TenantId }, ApiResponse<TenantDto>.Ok(tenant, "Tenant stack provisioned successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Fail($"Internal error during provisioning: {ex.Message}"));
        }
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.SuspendTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Tenant stack suspended."));
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var tenant = await _orchestrator.ResumeTenantAsync(id, ct);
        if (tenant == null)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<TenantDto>.Ok(tenant, "Tenant stack resumed."));
    }

    [HttpPost("{id:guid}/health")]
    public async Task<IActionResult> CheckHealth(Guid id, CancellationToken ct)
    {
        var isHealthy = await _orchestrator.CheckTenantHealthAsync(id, ct);
        return Ok(ApiResponse<bool>.Ok(isHealthy, isHealthy ? "Tenant healthy." : "Tenant check failed."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deprovision(Guid id, CancellationToken ct)
    {
        var success = await _orchestrator.DeprovisionTenantAsync(id, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Tenant not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Tenant stack deprovisioned successfully."));
    }
}
