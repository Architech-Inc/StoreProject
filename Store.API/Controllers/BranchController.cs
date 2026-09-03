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

    [HttpGet("{id:int}/performance")]
    public async Task<IActionResult> GetPerformance(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var fromDate = from?.ToUniversalTime() ?? DateTime.UtcNow.AddDays(-30);
        var toDate = to?.ToUniversalTime() ?? DateTime.UtcNow;
        try
        {
            var result = await _ops.GetBranchPerformanceAsync(id, fromDate, toDate, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> TransferPersonnel([FromBody] TransferEmployeeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userIdStr = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _ = Guid.TryParse(userIdStr, out var actingUserId);

        try
        {
            var result = await _ops.TransferPersonnelAsync(request, actingUserId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("transfers")]
    public async Task<IActionResult> GetTransfers([FromQuery] Guid? employeeId, [FromQuery] int? branchId, CancellationToken ct)
        => Ok(await _ops.GetPersonnelTransfersAsync(employeeId, branchId, ct));

    [HttpGet("{id:int}/stock")]
    public async Task<IActionResult> GetBranchStock(int id, CancellationToken ct)
    {
        try
        {
            var stocks = await _ops.GetBranchStocksAsync(id, ct);
            return Ok(stocks);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/stock")]
    public async Task<IActionResult> UpdateBranchStock(int id, [FromBody] UpdateBranchStockRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        request.BranchId = id;
        var userIdStr = User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _ = Guid.TryParse(userIdStr, out var actingUserId);

        try
        {
            var result = await _ops.UpdateBranchStockAsync(request, actingUserId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
