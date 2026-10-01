using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 34 — Unit tests for <see cref="CountriesController"/> (GAP-14).
/// Verifies public lookup endpoints, ISO resolution, and default fallback.
/// </summary>
public class CountriesControllerTests
{
    private readonly Mock<ICountryService> _mockCountryService;
    private readonly CountriesController _controller;

    public CountriesControllerTests()
    {
        _mockCountryService = new Mock<ICountryService>();
        _controller = new CountriesController(_mockCountryService.Object, NullLogger<CountriesController>.Instance);
    }

    [Fact]
    public async Task GetAll_ReturnsListOfCountries()
    {
        // Arrange
        var list = new List<CountryDto>
        {
            new CountryDto { CountryId = 1, Name = "Cameroon", IsoCode = "CM", PhoneCode = "+237", FlagEmoji = "🇨🇲" },
            new CountryDto { CountryId = 2, Name = "Nigeria", IsoCode = "NG", PhoneCode = "+234", FlagEmoji = "🇳🇬" }
        };
        _mockCountryService.Setup(s => s.GetCountriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var envelope = Assert.IsType<ApiResponse<IReadOnlyList<CountryDto>>>(ok.Value);
        Assert.True(envelope.Success);
        Assert.Equal(2, envelope.Data!.Count);
        Assert.Equal("Cameroon", envelope.Data[0].Name);
    }

    [Fact]
    public async Task GetByIsoCode_ReturnsCountry_WhenFound()
    {
        // Arrange
        var country = new Country { CountryId = 1, Name = "Cameroon", IsoCode = "CM", PhoneCode = "+237" };
        _mockCountryService.Setup(s => s.GetCountryByIsoCodeAsync("CM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        var result = await _controller.GetByIsoCode("CM", CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var envelope = Assert.IsType<ApiResponse<Country>>(ok.Value);
        Assert.True(envelope.Success);
        Assert.Equal("Cameroon", envelope.Data!.Name);
    }

    [Fact]
    public async Task GetByIsoCode_ReturnsNotFound_WhenCountryDoesNotExist()
    {
        // Arrange
        _mockCountryService.Setup(s => s.GetCountryByIsoCodeAsync("ZZ", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Country?)null);

        // Act
        var result = await _controller.GetByIsoCode("ZZ", CancellationToken.None);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var error = Assert.IsType<ApiErrorResponse>(notFound.Value);
        Assert.Equal("not_found", error.Code);
    }

    [Fact]
    public async Task GetDefault_ReturnsDefaultCountry()
    {
        // Arrange
        var country = new Country { CountryId = 1, Name = "Cameroon", IsoCode = "CM", PhoneCode = "+237" };
        _mockCountryService.Setup(s => s.GetDefaultCountryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(country);

        // Act
        var result = await _controller.GetDefault(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var envelope = Assert.IsType<ApiResponse<Country>>(ok.Value);
        Assert.True(envelope.Success);
        Assert.Equal("CM", envelope.Data!.IsoCode);
    }
}
