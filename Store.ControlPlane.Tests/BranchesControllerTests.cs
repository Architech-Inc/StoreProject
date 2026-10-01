using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class BranchesControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly Mock<ILogger<BranchesController>> _loggerMock;
    private readonly BranchesController _controller;

    public BranchesControllerTests()
    {
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _loggerMock = new Mock<ILogger<BranchesController>>();
        _controller = new BranchesController(_orchestratorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetBranches_ReturnsOkWithBranchList()
    {
        var tenantId = Guid.NewGuid();
        var sampleBranches = new List<BranchDto>
        {
            new BranchDto(Guid.NewGuid(), "Main Branch", "main-branch", "Platform", null, "https://main.store.example.com", "Verified", "", "", DateTime.UtcNow),
            new BranchDto(Guid.NewGuid(), "Downtown", "downtown", "Platform", null, "https://downtown.store.example.com", "Verified", "", "", DateTime.UtcNow)
        };

        _orchestratorMock
            .Setup(o => o.GetBranchesAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleBranches);

        var result = await _controller.GetBranches(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<BranchDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task AddBranch_ValidRequest_ReturnsCreated()
    {
        var tenantId = Guid.NewGuid();
        var request = new CreateBranchRequest("North Branch", "north-branch", "Platform", null);
        var createdDto = new BranchDto(Guid.NewGuid(), "North Branch", "north-branch", "Platform", null, "https://north.store.example.com", "Verified", "", "", DateTime.UtcNow);

        _orchestratorMock
            .Setup(o => o.AddBranchAsync(tenantId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        var result = await _controller.AddBranch(tenantId, request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        var response = Assert.IsType<ApiResponse<BranchDto>>(objectResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("North Branch", response.Data.BranchName);
    }

    [Fact]
    public async Task AddBranch_InvalidModel_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        var request = new CreateBranchRequest("", "", "Platform", null);
        _controller.ModelState.AddModelError("BranchName", "BranchName is required");

        var result = await _controller.AddBranch(tenantId, request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
    }
}
