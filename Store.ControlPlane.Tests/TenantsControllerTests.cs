using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class TenantsControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly Mock<IDbContextFactory<ControlPlaneDbContext>> _dbContextFactoryMock;
    private readonly Mock<ISecretEncryptionService> _encryptionServiceMock;
    private readonly Mock<ILogger<TenantsController>> _loggerMock;
    private readonly TenantsController _controller;

    public TenantsControllerTests()
    {
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _dbContextFactoryMock = new Mock<IDbContextFactory<ControlPlaneDbContext>>();
        _encryptionServiceMock = new Mock<ISecretEncryptionService>();
        _loggerMock = new Mock<ILogger<TenantsController>>();

        _controller = new TenantsController(
            _orchestratorMock.Object,
            _dbContextFactoryMock.Object,
            _encryptionServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithTenantList()
    {
        var sampleTenants = new List<TenantDto>
        {
            new TenantDto
            {
                TenantId = Guid.NewGuid(),
                Name = "Tenant 1",
                Slug = "tenant-1",
                Status = TenantStatus.Active,
                IsHealthy = true,
                UiUrl = "https://t1.store.example.com",
                ApiUrl = "https://api-t1.store.example.com",
                DateCreated = DateTime.UtcNow
            },
            new TenantDto
            {
                TenantId = Guid.NewGuid(),
                Name = "Tenant 2",
                Slug = "tenant-2",
                Status = TenantStatus.Active,
                IsHealthy = true,
                UiUrl = "https://t2.store.example.com",
                ApiUrl = "https://api-t2.store.example.com",
                DateCreated = DateTime.UtcNow
            }
        };

        _orchestratorMock
            .Setup(o => o.GetAllTenantsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleTenants);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<TenantDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task CheckSlug_TooShort_ReturnsNotAvailable()
    {
        var repoMock = new Mock<ITenantRepository>();

        var result = await _controller.CheckSlug("ab", repoMock.Object, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<SlugCheckDto>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.False(response.Data.IsAvailable);
    }

    [Fact]
    public async Task CheckSlug_AlreadyTaken_ReturnsNotAvailable()
    {
        var repoMock = new Mock<ITenantRepository>();
        repoMock.Setup(r => r.SlugExistsAsync("clexan", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.CheckSlug("clexan", repoMock.Object, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<SlugCheckDto>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.False(response.Data.IsAvailable);
    }

    [Fact]
    public async Task CheckSlug_Available_ReturnsAvailable()
    {
        var repoMock = new Mock<ITenantRepository>();
        repoMock.Setup(r => r.SlugExistsAsync("fresh-brand", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.CheckSlug("fresh-brand", repoMock.Object, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<SlugCheckDto>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.IsAvailable);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        _orchestratorMock
            .Setup(o => o.GetTenantDetailsAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantDetailDto?)null);

        var result = await _controller.GetById(tenantId, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }
}
