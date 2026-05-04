using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Transfers;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _transferService;

    public StockTransfersController(IStockTransferService transferService)
        => _transferService = transferService;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId, [FromQuery] string? status)
    {
        var list = await _transferService.GetAllAsync(branchId, status);
        return Ok(ApiResponse<List<StockTransferDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _transferService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Transfer not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<StockTransferDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (request.FromBranchId == request.ToBranchId)
            return BadRequest(ApiErrorResponse.From("bad_request", "Source and destination branches must differ", traceId: HttpContext.TraceIdentifier));

        var dto = await _transferService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.StockTransferId }, ApiResponse<StockTransferDto>.Ok(dto));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveTransferRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _transferService.ApproveAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request", "Transfer cannot be approved in its current state", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<StockTransferDto>.Ok(dto));
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectTransferRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var ok = await _transferService.RejectAsync(id, userId, request);
        if (!ok)
            return BadRequest(ApiErrorResponse.From("bad_request", "Transfer cannot be rejected in its current state", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }

    [HttpPost("{id:int}/dispatch")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Dispatch(int id, [FromBody] DispatchTransferRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _transferService.DispatchAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request", "Transfer must be approved before dispatching", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<StockTransferDto>.Ok(dto));
    }

    [HttpPost("{id:int}/receive")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Receive(int id, [FromBody] ReceiveTransferRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _transferService.ReceiveAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request", "Transfer must be dispatched before receiving", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<StockTransferDto>.Ok(dto));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Cancel(int id, [FromBody] string? reason)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var ok = await _transferService.CancelAsync(id, userId, reason);
        if (!ok)
            return BadRequest(ApiErrorResponse.From("bad_request", "Transfer cannot be cancelled in its current state", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}
