using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class DomainsControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly Mock<ILogger<DomainsController>> _loggerMock;
    private readonly DomainsController _controller;

    public DomainsControllerTests()
    {
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _loggerMock = new Mock<ILogger<DomainsController>>();
        _controller = new DomainsController(_orchestratorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetDomains_WhenFound_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var domainDto = new TenantDomainDto(
            tenantId,
            "clexan-bakery",
            "https://clexan-bakery.store.example.com",
            "https://api-clexan-bakery.store.example.com",
            "shop.clexan.com",
            "Verified",
            "_clexan-verify",
            "token123",
            DateTime.UtcNow,
            null);

        _orchestratorMock
            .Setup(o => o.GetDomainConfigAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainDto);

        var result = await _controller.GetDomains(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TenantDomainDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("shop.clexan.com", response.Data.CustomDomain);
    }

    [Fact]
    public async Task GetDomains_WhenNotFound_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        _orchestratorMock
            .Setup(o => o.GetDomainConfigAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDomainDto?)null);

        var result = await _controller.GetDomains(tenantId, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task SetCustomDomain_Valid_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var request = new SetCustomDomainRequest("shop.clexan.com");
        var domainDto = new TenantDomainDto(
            tenantId,
            "clexan-bakery",
            "https://clexan-bakery.store.example.com",
            "https://api-clexan-bakery.store.example.com",
            "shop.clexan.com",
            "PendingVerification",
            "_clexan-verify",
            "token123",
            null,
            null);

        _orchestratorMock
            .Setup(o => o.SetCustomDomainAsync(tenantId, "shop.clexan.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(domainDto);

        var result = await _controller.SetCustomDomain(tenantId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TenantDomainDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("PendingVerification", response.Data.CustomDomainStatus);
    }

    [Fact]
    public async Task VerifyCustomDomain_ReturnsOkWithVerificationResult()
    {
        var tenantId = Guid.NewGuid();
        var verifyResponse = new VerifyDomainResponse(
            "shop.clexan.com",
            true,
            "Verified",
            "_clexan-verify.shop.clexan.com",
            "token123",
            new List<string> { "token123" },
            "Verification successful.");

        _orchestratorMock
            .Setup(o => o.VerifyCustomDomainAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verifyResponse);

        var result = await _controller.VerifyCustomDomain(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<VerifyDomainResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.IsVerified);
    }
}
