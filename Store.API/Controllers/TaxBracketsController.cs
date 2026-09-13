using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.DbServices.Services.Interfaces;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.HR;
using Store.Models.DTOs.Operations;
using Store.Models.Entities.HR;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/payroll/tax-brackets")]
[Authorize]
public class TaxBracketsController : ControllerBase
{
    private readonly ITaxBracketService _taxBracketService;

    public TaxBracketsController(ITaxBracketService taxBracketService)
    {
        _taxBracketService = taxBracketService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.TaxBracketsRead)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var brackets = await _taxBracketService.GetAllAsync(ct);
        var dtos = brackets.Select(b => new TaxBracketDto
        {
            TaxBracketId = b.TaxBracketId,
            MinAmount = b.MinAmount,
            MaxAmount = b.MaxAmount,
            TaxPercentage = b.TaxPercentage,
            FixedTaxAmount = b.FixedTaxAmount,
            IsActive = b.IsActive
        });
        return Ok(ApiResponse<IEnumerable<TaxBracketDto>>.Ok(dtos));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.TaxBracketsRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var bracket = await _taxBracketService.GetByIdAsync(id, ct);
        if (bracket is null) return NotFound(ApiResponse<object>.Fail("Tax bracket not found."));

        var dto = new TaxBracketDto
        {
            TaxBracketId = bracket.TaxBracketId,
            MinAmount = bracket.MinAmount,
            MaxAmount = bracket.MaxAmount,
            TaxPercentage = bracket.TaxPercentage,
            FixedTaxAmount = bracket.FixedTaxAmount,
            IsActive = bracket.IsActive
        };
        return Ok(ApiResponse<TaxBracketDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.TaxBracketsWrite)]
    public async Task<IActionResult> Create([FromBody] CreateTaxBracketRequest request, CancellationToken ct)
    {
        var bracket = await _taxBracketService.CreateAsync(
            request.MinAmount,
            request.MaxAmount,
            request.TaxPercentage,
            request.FixedTaxAmount,
            request.IsActive,
            ct);

        var dto = new TaxBracketDto
        {
            TaxBracketId = bracket.TaxBracketId,
            MinAmount = bracket.MinAmount,
            MaxAmount = bracket.MaxAmount,
            TaxPercentage = bracket.TaxPercentage,
            FixedTaxAmount = bracket.FixedTaxAmount,
            IsActive = bracket.IsActive
        };

        return CreatedAtAction(nameof(GetById), new { id = bracket.TaxBracketId }, ApiResponse<TaxBracketDto>.Ok(dto));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.TaxBracketsWrite)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaxBracketRequest request, CancellationToken ct)
    {
        var bracket = await _taxBracketService.UpdateAsync(
            id,
            request.MinAmount,
            request.MaxAmount,
            request.TaxPercentage,
            request.FixedTaxAmount,
            request.IsActive,
            ct);

        if (bracket is null) return NotFound(ApiResponse<object>.Fail("Tax bracket not found."));

        var dto = new TaxBracketDto
        {
            TaxBracketId = bracket.TaxBracketId,
            MinAmount = bracket.MinAmount,
            MaxAmount = bracket.MaxAmount,
            TaxPercentage = bracket.TaxPercentage,
            FixedTaxAmount = bracket.FixedTaxAmount,
            IsActive = bracket.IsActive
        };

        return Ok(ApiResponse<TaxBracketDto>.Ok(dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _taxBracketService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Tax bracket not found."));

        return Ok(ApiResponse<object>.Ok(null!, "Tax bracket deleted."));
    }
}
