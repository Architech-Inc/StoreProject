using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Notifications;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/purchase-orders")]
[ApiController]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _poService;
    private readonly IRealTimeNotificationService _notifications;

    public PurchaseOrdersController(IPurchaseOrderService poService, IRealTimeNotificationService notifications)
    {
        _poService = poService;
        _notifications = notifications;
    }

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _poService.GetPurchaseOrderMetricsAsync(ct);
        return Ok(ApiResponse<PurchaseOrderMetricsDto>.Ok(metrics));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetPaged([FromQuery] PurchaseOrderFilterRequest request, CancellationToken ct)
    {
        var result = await _poService.GetPurchaseOrdersPagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<PurchaseOrderDto>>.Ok(result));
    }

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
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Purchase order not found",
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
            return BadRequest(ApiErrorResponse.From(ErrorCode.BadRequest,
                "Purchase order must be in Draft status to submit",
                traceId: HttpContext.TraceIdentifier));

        await TryNotifyPurchaseOrderAsync(dto, "was submitted for approval", $"PO #{dto.ReferenceNumber ?? dto.PurchaseOrderId.ToString()} submitted for approval by {dto.RequestedByUser}.");

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
            return BadRequest(ApiErrorResponse.From(ErrorCode.BadRequest,
                "Purchase order must be in Submitted status to approve",
                traceId: HttpContext.TraceIdentifier));

        await TryNotifyPurchaseOrderAsync(dto, "was approved", $"PO #{dto.ReferenceNumber ?? dto.PurchaseOrderId.ToString()} approved by {dto.ApprovedByUser ?? "Manager"}. Ready for receiving.");

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
            return BadRequest(ApiErrorResponse.From(ErrorCode.BadRequest,
                "Purchase order must be Approved or PartiallyReceived to receive goods",
                traceId: HttpContext.TraceIdentifier));

        await TryNotifyPurchaseOrderAsync(dto, $"goods received ({dto.Status})", $"PO #{dto.ReferenceNumber ?? dto.PurchaseOrderId.ToString()} goods received. Status: {dto.Status}.");

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
            return BadRequest(ApiErrorResponse.From(ErrorCode.BadRequest,
                "Only Draft or Submitted purchase orders can be cancelled",
                traceId: HttpContext.TraceIdentifier));

        await TryNotifyPurchaseOrderAsync(dto, "was cancelled", $"PO #{dto.ReferenceNumber ?? dto.PurchaseOrderId.ToString()} was cancelled.");

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    private async Task TryNotifyPurchaseOrderAsync(PurchaseOrderDto dto, string action, string? message = null)
    {
        try
        {
            var notifDto = new PurchaseOrderNotificationDto
            {
                PurchaseOrderId = dto.PurchaseOrderId,
                OrderNumber = !string.IsNullOrWhiteSpace(dto.ReferenceNumber) ? dto.ReferenceNumber : dto.PurchaseOrderId.ToString(),
                SupplierName = dto.SupplierName,
                TotalAmount = dto.TotalValuation,
                Status = dto.Status,
                RequestedByName = dto.RequestedByUser,
                ApprovedByName = dto.ApprovedByUser,
                BranchId = dto.BranchId,
                BranchName = dto.BranchName,
                Message = message ?? $"Purchase order #{dto.ReferenceNumber ?? dto.PurchaseOrderId.ToString()} ({dto.SupplierName}) {action}.",
                DateCreated = DateTime.UtcNow
            };

            await _notifications.NotifyPurchaseOrderUpdateAsync(notifDto);
        }
        catch
        {
            // Real-time broadcast failure must never abort primary business transaction
        }
    }

    [HttpPost("{id:int}/pay")]
    [Authorize(Policy = PermissionKeys.CashWrite)] 
    public async Task<IActionResult> Pay(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _poService.PayAsync(id, userId);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From(ErrorCode.BadRequest,
                "Purchase order must be received to be paid, or it is already paid.",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(dto));
    }

    [HttpPost("auto-reorder/trigger")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> TriggerAutoReorder(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        var result = await _poService.ExecuteAutomatedReorderAsync(userId == Guid.Empty ? null : userId, ct);
        return Ok(ApiResponse<AutomatedReorderResultDto>.Ok(result, result.Message));
    }

    [HttpPost("auto-reorder/thresholds")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> TriggerThresholdReorder(
        [FromServices] IProcurementAutomationService procurementService,
        CancellationToken ct)
    {
        var count = await procurementService.EvaluateInventoryThresholdsAsync(ct);
        return Ok(ApiResponse<object>.Ok(new { generatedPurchaseOrdersCount = count },
            $"Automated threshold evaluation complete. Generated {count} draft purchase order(s)."));
    }
}
