using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LoyaltyCampaignsController : ControllerBase
{
    private readonly ILoyaltyCampaignService _campaignService;

    public LoyaltyCampaignsController(ILoyaltyCampaignService campaignService)
        => _campaignService = campaignService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var list = await _campaignService.GetAllAsync(activeOnly, ct);
        return Ok(ApiResponse<IEnumerable<LoyaltyCampaignDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await _campaignService.GetByIdAsync(id, ct);
        if (dto is null) return NotFound(ApiErrorResponse.From("CAMPAIGN_NOT_FOUND", "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveForSegment([FromQuery] string segment, CancellationToken ct)
    {
        var list = await _campaignService.GetActiveCampaignsForSegmentAsync(segment, ct);
        return Ok(ApiResponse<IEnumerable<LoyaltyCampaignDto>>.Ok(list));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiErrorResponse.From("VALIDATION_ERROR", "Invalid request.", traceId: HttpContext.TraceIdentifier));

        if (request.EndDate <= request.StartDate)
            return BadRequest(ApiErrorResponse.From("INVALID_DATES", "EndDate must be after StartDate.", traceId: HttpContext.TraceIdentifier));

        var dto = await _campaignService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.LoyaltyCampaignId }, ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiErrorResponse.From("VALIDATION_ERROR", "Invalid request.", traceId: HttpContext.TraceIdentifier));

        var dto = await _campaignService.UpdateAsync(id, request, ct);
        if (dto is null) return NotFound(ApiErrorResponse.From("CAMPAIGN_NOT_FOUND", "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var success = await _campaignService.DeleteAsync(id, ct);
        if (!success) return NotFound(ApiErrorResponse.From("CAMPAIGN_NOT_FOUND", "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}
