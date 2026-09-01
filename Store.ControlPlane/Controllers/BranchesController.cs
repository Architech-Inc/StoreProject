using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants/{id:guid}/branches")]
public class BranchesController : ControllerBase
{
    private readonly ITenantOrchestrator _orchestrator;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(ITenantOrchestrator orchestrator, ILogger<BranchesController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches(Guid id, CancellationToken ct)
    {
        var branches = await _orchestrator.GetBranchesAsync(id, ct);
        return Ok(ApiResponse<IReadOnlyList<BranchDto>>.Ok(branches));
    }

    [HttpPost]
    public async Task<IActionResult> AddBranch(Guid id, [FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid branch configuration."));
        }

        try
        {
            var branch = await _orchestrator.AddBranchAsync(id, request, ct);
            return StatusCode(201, ApiResponse<BranchDto>.Ok(branch, "Branch subdomain created successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating branch for tenant {TenantId}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to create branch."));
        }
    }

    [HttpPost("{branchId:guid}/verify")]
    public async Task<IActionResult> VerifyBranch(Guid id, Guid branchId, CancellationToken ct)
    {
        try
        {
            var result = await _orchestrator.VerifyBranchAsync(id, branchId, ct);
            return Ok(ApiResponse<VerifyDomainResponse>.Ok(result, result.Message ?? "Branch DNS verified."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying branch {BranchId} for tenant {TenantId}", branchId, id);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to verify branch DNS."));
        }
    }

    [HttpDelete("{branchId:guid}")]
    public async Task<IActionResult> RemoveBranch(Guid id, Guid branchId, CancellationToken ct)
    {
        var success = await _orchestrator.RemoveBranchAsync(id, branchId, ct);
        if (!success)
        {
            return NotFound(ApiResponse<object>.Fail("Branch or tenant not found."));
        }
        return Ok(ApiResponse<object>.Ok(null!, "Branch mapping removed successfully."));
    }
}
