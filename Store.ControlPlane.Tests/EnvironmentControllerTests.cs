using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class EnvironmentControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly Mock<ILogger<EnvironmentController>> _loggerMock;
    private readonly EnvironmentController _controller;

    public EnvironmentControllerTests()
    {
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _loggerMock = new Mock<ILogger<EnvironmentController>>();
        _controller = new EnvironmentController(_orchestratorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetStatus_WhenFound_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var statusDto = new EnvironmentStatusDto(
            tenantId,
            "ClexAn Bakery",
            "clexan-bakery",
            "Running",
            true,
            DateTime.UtcNow,
            "All containers healthy",
            new List<ContainerStatusDto>
            {
                new("api", "clexan_api", "API", "store-api:latest", "running", true, DateTime.UtcNow),
                new("ui", "clexan_ui", "UI", "store-ui:latest", "running", true, DateTime.UtcNow)
            });

        _orchestratorMock
            .Setup(o => o.GetEnvironmentStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statusDto);

        var result = await _controller.GetStatus(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<EnvironmentStatusDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.IsHealthy);
        Assert.Equal(2, response.Data.Containers.Count);
    }

    [Fact]
    public async Task GetStatus_WhenNotFound_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        _orchestratorMock
            .Setup(o => o.GetEnvironmentStatusAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EnvironmentStatusDto?)null);

        var result = await _controller.GetStatus(tenantId, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task RestartService_All_DispatchesRestartAll()
    {
        var tenantId = Guid.NewGuid();
        _orchestratorMock
            .Setup(o => o.RestartAllContainersAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RestartService(tenantId, "all", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        _orchestratorMock.Verify(o => o.RestartAllContainersAsync(tenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestartService_SpecificService_DispatchesRestartContainer()
    {
        var tenantId = Guid.NewGuid();
        _orchestratorMock
            .Setup(o => o.RestartContainerAsync(tenantId, "api", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RestartService(tenantId, "api", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        _orchestratorMock.Verify(o => o.RestartContainerAsync(tenantId, "api", It.IsAny<CancellationToken>()), Times.Once);
    }
}
