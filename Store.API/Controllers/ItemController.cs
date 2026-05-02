using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/item")]
[Route("api/items")]
[Authorize]
public class ItemController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService) => _itemService = itemService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _itemService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<ItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _itemService.GetByIdAsync(id, ct);
        if (item is null) return NotFound(ApiResponse<object>.Fail("Item not found."));
        return Ok(ApiResponse<ItemDto>.Ok(item));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        var items = await _itemService.GetLowStockAsync(ct);
        return Ok(ApiResponse<IEnumerable<ItemDto>>.Ok(items));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateItemRequest request, CancellationToken ct)
    {
        var item = await _itemService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = item.ItemId }, ApiResponse<ItemDto>.Ok(item, "Item created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        var item = await _itemService.UpdateAsync(id, request, ct);
        if (item is null) return NotFound(ApiResponse<object>.Fail("Item not found."));
        return Ok(ApiResponse<ItemDto>.Ok(item));
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var success = await _itemService.AdjustStockAsync(id, request, ct);
        if (!success) return NotFound(ApiResponse<object>.Fail("Item not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Stock adjusted."));
    }

    [HttpPost("{id:guid}/adjust-stock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AdjustStockCompat(Guid id, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var success = await _itemService.AdjustStockAsync(id, request, ct);
        if (!success) return NotFound(ApiResponse<object>.Fail("Item not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Stock adjusted."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _itemService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Item not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Item deactivated."));
    }
}
