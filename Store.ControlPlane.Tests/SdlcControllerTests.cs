using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Xunit;

namespace Store.ControlPlane.Tests;

public class SdlcControllerTests
{
    private readonly Mock<ITenantOrchestrator> _orchestratorMock;
    private readonly ControlPlaneDbContext _dbContext;
    private readonly SdlcController _controller;

    public SdlcControllerTests()
    {
        var options = new DbContextOptionsBuilder<ControlPlaneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ControlPlaneDbContext(options);
        _orchestratorMock = new Mock<ITenantOrchestrator>();
        _controller = new SdlcController(_orchestratorMock.Object, _dbContext);
    }

    [Fact]
    public async Task GetReleases_ReturnsAllReleasesOrderedByDate()
    {
        var release1 = new SystemRelease
        {
            ReleaseId = Guid.NewGuid(),
            VersionName = "v1.0",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            IsPublic = true,
            ReleaseNotes = "Initial release"
        };
        var release2 = new SystemRelease
        {
            ReleaseId = Guid.NewGuid(),
            VersionName = "v2.0",
            ReleaseDate = DateTime.UtcNow,
            IsPublic = true,
            ReleaseNotes = "Major upgrade"
        };

        _dbContext.SystemReleases.AddRange(release1, release2);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetReleases(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var releases = Assert.IsAssignableFrom<List<SystemReleaseDto>>(okResult.Value);
        Assert.Equal(2, releases.Count);
        Assert.Equal("v2.0", releases[0].VersionName);
    }

    [Fact]
    public async Task UpgradeTenant_WhenTenantNotFound_ReturnsNotFound()
    {
        var result = await _controller.UpgradeTenant("missing-tenant", Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpgradeTenant_WhenReleaseNotFound_ReturnsNotFound()
    {
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Acme Store",
            Slug = "acme-store"
        };
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.UpgradeTenant("acme-store", Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Release not found", notFound.Value);
    }

    [Fact]
    public async Task UpgradeTenant_Valid_TriggersSnapshotAndRestarts()
    {
        var release = new SystemRelease
        {
            ReleaseId = Guid.NewGuid(),
            VersionName = "v3.0",
            ReleaseDate = DateTime.UtcNow,
            IsPublic = true
        };
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Acme Store",
            Slug = "acme-store"
        };
        _dbContext.SystemReleases.Add(release);
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.UpgradeTenant("acme-store", release.ReleaseId, CancellationToken.None);

        var okResult = Assert.IsType<OkResult>(result);
        Assert.Equal(release.ReleaseId, tenant.CurrentReleaseId);

        _orchestratorMock.Verify(o => o.CreateSnapshotAsync(tenant.TenantId, SnapshotType.PreUpgrade, It.IsAny<CancellationToken>()), Times.Once);
        _orchestratorMock.Verify(o => o.RestartAllContainersAsync(tenant.TenantId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
