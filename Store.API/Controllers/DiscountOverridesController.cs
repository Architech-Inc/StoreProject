using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/discount-overrides")]
[ApiController]
[Authorize]
public class DiscountOverridesController : ControllerBase
{
    private readonly IDiscountOverrideService _overrideService;

    public DiscountOverridesController(IDiscountOverrideService overrideService)
        => _overrideService = overrideService;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var list = await _overrideService.GetAllAsync(status);
        return Ok(ApiResponse<List<DiscountOverrideDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _overrideService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Override request not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountOverrideRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _overrideService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.DiscountOverrideRequestId },
            ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewDiscountOverrideRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _overrideService.ReviewAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Override request is not in Pending state or does not exist",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var ok = await _overrideService.CancelAsync(id, userId);
        if (!ok)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Override request is not in Pending state or does not exist",
                traceId: HttpContext.TraceIdentifier));

        return NoContent();
    }
}
