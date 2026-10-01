using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

using Store.API.Attributes;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = PermissionKeys.LoyaltyRead)]
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
        if (dto is null) return NotFound(ApiErrorResponse.From(ErrorCode.CampaignNotFound, "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveForSegment([FromQuery] string segment, CancellationToken ct)
    {
        var list = await _campaignService.GetActiveCampaignsForSegmentAsync(segment, ct);
        return Ok(ApiResponse<IEnumerable<LoyaltyCampaignDto>>.Ok(list));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.LoyaltyWrite)]
    [Audit("Create Loyalty Campaign", Category = "Loyalty")]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiErrorResponse.From(ErrorCode.ValidationError, "Invalid request.", traceId: HttpContext.TraceIdentifier));

        if (request.EndDate <= request.StartDate)
            return BadRequest(ApiErrorResponse.From(ErrorCode.InvalidDates, "EndDate must be after StartDate.", traceId: HttpContext.TraceIdentifier));

        var dto = await _campaignService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.LoyaltyCampaignId }, ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.LoyaltyWrite)]
    [Audit("Update Loyalty Campaign", Category = "Loyalty")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCampaignRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiErrorResponse.From(ErrorCode.ValidationError, "Invalid request.", traceId: HttpContext.TraceIdentifier));

        var dto = await _campaignService.UpdateAsync(id, request, ct);
        if (dto is null) return NotFound(ApiErrorResponse.From(ErrorCode.CampaignNotFound, "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<LoyaltyCampaignDto>.Ok(dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.LoyaltyWrite)]
    [Audit("Delete Loyalty Campaign", Category = "Loyalty")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var success = await _campaignService.DeleteAsync(id, ct);
        if (!success) return NotFound(ApiErrorResponse.From(ErrorCode.CampaignNotFound, "Campaign not found.", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}
