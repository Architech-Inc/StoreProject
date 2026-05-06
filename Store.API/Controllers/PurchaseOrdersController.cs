using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/purchase-orders")]
[ApiController]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _poService;

    public PurchaseOrdersController(IPurchaseOrderService poService)
        => _poService = poService;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] Guid? supplierId)
    {
        PurchaseOrderStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PurchaseOrderStatus>(status, ignoreCase: true, out var s))
            parsed = s;

        var list = await _poService.GetAllAsync(parsed, supplierId);
        return Ok(ApiResponse<List<PurchaseOrderDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _poService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Purchase order not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.PurchaseOrderId },
            ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Submit(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.SubmitAsync(id, userId);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Purchase order must be in Draft status to submit",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Approve(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.ApproveAsync(id, userId);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Purchase order must be in Submitted status to approve",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost("{id:int}/receive")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Receive(int id, [FromBody] ReceivePurchaseOrderRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.ReceiveAsync(id, request, userId);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Purchase order must be Approved or PartiallyReceived to receive goods",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.CancelAsync(id, userId);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Only Draft or Submitted purchase orders can be cancelled",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }
}
