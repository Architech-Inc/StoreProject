using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/admin/branches")]
[Authorize(Policy = PermissionKeys.AdminBranches)]
public class BranchController : ControllerBase
{
    private readonly IStoreOperationsService _ops;

    public BranchController(IStoreOperationsService ops)
    {
        _ops = ops;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches(CancellationToken ct)
        => Ok(await _ops.GetBranchesAsync(ct));

    [HttpPost]
    public async Task<IActionResult> UpsertBranch([FromBody] UpsertBranchRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _ops.UpsertBranchAsync(request, ct));
    }

    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] int? branchId, [FromQuery] Guid? userId, CancellationToken ct)
        => Ok(await _ops.GetUserBranchRolesAsync(branchId, userId, ct));

    [HttpPost("assignments")]
    public async Task<IActionResult> AssignUserBranchRole([FromBody] AssignUserBranchRoleRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _ops.AssignUserBranchRoleAsync(request, ct));
    }

    [HttpDelete("assignments/{id:long}")]
    public async Task<IActionResult> RemoveAssignment(long id, CancellationToken ct)
    {
        var removed = await _ops.RemoveUserBranchRoleAsync(id, ct);
        return removed ? NoContent() : NotFound();
    }
}
