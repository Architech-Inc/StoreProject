using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class PublicStatusControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly Mock<ILogger<PublicStatusController>> _loggerMock;
    private readonly PublicStatusController _controller;

    public PublicStatusControllerTests()
    {
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _loggerMock = new Mock<ILogger<PublicStatusController>>();
        _controller = new PublicStatusController(_orchestratorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetStatus_EmptySlug_ReturnsBadRequest()
    {
        var result = await _controller.GetStatus("", CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetStatus_NotFound_ReturnsNotFound()
    {
        _orchestratorMock
            .Setup(o => o.GetPublicStatusAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantStatusDto?)null);

        var result = await _controller.GetStatus("non-existent", CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task GetStatus_Found_ReturnsOkWithStatus()
    {
        var statusDto = new TenantStatusDto
        {
            TenantId = Guid.NewGuid(),
            Name = "Acme Store",
            Slug = "acme-store",
            Status = "Active",
            IsHealthy = true,
            IsAcceptingTraffic = true
        };

        _orchestratorMock
            .Setup(o => o.GetPublicStatusAsync("acme-store", It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusDto);

        var result = await _controller.GetStatus("acme-store", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TenantStatusDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.IsHealthy);
        Assert.Equal("acme-store", response.Data.Slug);
    }
}
