using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/lookup/[controller]")]
[AllowAnonymous]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<CountryDto>>.Ok(await _countryService.GetCountriesAsync(ct)));

    [HttpGet("{isoCode}")]
    public async Task<IActionResult> GetByIsoCode(string isoCode, CancellationToken ct)
    {
        var country = await _countryService.GetCountryByIsoCodeAsync(isoCode, ct);
        if (country is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, $"Country with ISO code '{isoCode}' not found."));

        return Ok(ApiResponse<Country>.Ok(country));
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefault(CancellationToken ct) =>
        Ok(ApiResponse<Country>.Ok(await _countryService.GetDefaultCountryAsync(ct)));
}
