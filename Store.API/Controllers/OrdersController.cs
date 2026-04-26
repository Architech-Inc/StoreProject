using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Orders;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _orderService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, ct);
        if (order is null) return NotFound(ApiResponse<object>.Fail("Order not found."));
        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        var order = await _orderService.CreateAsync(request, userId == Guid.Empty ? null : userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, ApiResponse<OrderDto>.Ok(order, "Order created."));
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Receive(Guid id, CancellationToken ct)
    {
        var success = await _orderService.ReceiveOrderAsync(id, ct);
        if (!success) return NotFound(ApiResponse<object>.Fail("Order not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Order received. Stock updated."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var success = await _orderService.CancelOrderAsync(id, ct);
        if (!success) return BadRequest(ApiResponse<object>.Fail("Cannot cancel this order."));
        return Ok(ApiResponse<object>.Ok(null!, "Order cancelled."));
    }
}
