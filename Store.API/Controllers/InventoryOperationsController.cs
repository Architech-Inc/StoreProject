using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryOperationsController : ControllerBase
{
    private readonly IStoreOperationsService _ops;

    public InventoryOperationsController(IStoreOperationsService ops)
    {
        _ops = ops;
    }

    [HttpGet("movements")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetMovements([FromQuery] StockMovementFilterRequest request, CancellationToken ct = default)
    {
        var result = await _ops.GetStockMovementsPagedAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct = default)
    {
        var metrics = await _ops.GetInventoryMetricsAsync(ct);
        return Ok(metrics);
    }

    [HttpPost("receive")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> ReceiveGoods([FromBody] GoodsReceiptRequest request, CancellationToken ct)
    {
        var uid = TryGetUserId();
        var result = await _ops.ReceiveGoodsAsync(request, uid, ct);
        return Ok(result);
    }

    [HttpPost("return")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> ProcessReturn([FromBody] StockReturnRequest request, CancellationToken ct)
    {
        var uid = TryGetUserId();
        var result = await _ops.ProcessReturnAsync(request, uid, ct);
        return Ok(result);
    }

    [HttpPost("adjust")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentAuditRequest request, CancellationToken ct)
    {
        var uid = TryGetUserId();
        var result = await _ops.AdjustStockAsync(request, uid, ct);
        return Ok(result);
    }

    [HttpGet("reorder")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> ReorderSuggestions(CancellationToken ct)
    {
        var rows = await _ops.GetLowStockReorderSuggestionsAsync(ct);
        return Ok(rows);
    }

    private Guid? TryGetUserId()
    {
        var claim = User.FindFirst("uid")?.Value;
        return Guid.TryParse(claim, out var uid) ? uid : null;
    }
}
