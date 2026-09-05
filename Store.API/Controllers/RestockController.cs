using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DbServices.Services;
using Store.Models.Enums;
using System.Security.Claims;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RestockController : ControllerBase
{
    private readonly IDemandForecastingService _demandForecastingService;
    private readonly ILogger<RestockController> _logger;

    public RestockController(IDemandForecastingService demandForecastingService, ILogger<RestockController> logger)
    {
        _demandForecastingService = demandForecastingService;
        _logger = logger;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRecommendations([FromQuery] int? branchId, CancellationToken ct)
    {
        try
        {
            var recommendations = await _demandForecastingService.GetPendingRecommendationsAsync(branchId, ct);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending recommendations");
            return StatusCode(500, "An error occurred while retrieving recommendations.");
        }
    }

    [HttpPost("{recommendationId:guid}/convert-to-transfer")]
    public async Task<IActionResult> ConvertToTransfer(Guid recommendationId, CancellationToken ct)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var success = await _demandForecastingService.ConvertToStockTransferAsync(recommendationId, userId, ct);
            if (!success)
                return BadRequest("Failed to convert recommendation to stock transfer. Check if branch has an internal supplying warehouse or if it's already processed.");

            return Ok(new { Message = "Successfully created stock transfer request." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting recommendation {RecommendationId} to transfer", recommendationId);
            return StatusCode(500, "An error occurred while converting the recommendation.");
        }
    }

    [HttpPost("{recommendationId:guid}/convert-to-po")]
    public async Task<IActionResult> ConvertToPurchaseOrder(Guid recommendationId, CancellationToken ct)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var success = await _demandForecastingService.ConvertToPurchaseOrderAsync(recommendationId, userId, ct);
            if (!success)
                return BadRequest("Failed to convert recommendation to purchase order. Ensure supplier is configured or it's not already processed.");

            return Ok(new { Message = "Successfully created draft purchase order." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting recommendation {RecommendationId} to PO", recommendationId);
            return StatusCode(500, "An error occurred while converting the recommendation.");
        }
    }

    [HttpPost("{recommendationId:guid}/dismiss")]
    public async Task<IActionResult> DismissRecommendation(Guid recommendationId, CancellationToken ct)
    {
        try
        {
            var success = await _demandForecastingService.DismissRecommendationAsync(recommendationId, ct);
            if (!success)
                return BadRequest("Failed to dismiss recommendation.");

            return Ok(new { Message = "Recommendation dismissed." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing recommendation {RecommendationId}", recommendationId);
            return StatusCode(500, "An error occurred while dismissing the recommendation.");
        }
    }
}
