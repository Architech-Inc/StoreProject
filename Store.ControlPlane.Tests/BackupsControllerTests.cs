using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class BackupsControllerTests
{
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly Mock<ILogger<BackupsController>> _loggerMock;
    private readonly BackupsController _controller;

    public BackupsControllerTests()
    {
        _backupServiceMock = new Mock<IBackupService>();
        _loggerMock = new Mock<ILogger<BackupsController>>();
        _controller = new BackupsController(_backupServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSummary_WhenFound_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var schedule = new BackupScheduleDto("Daily", 7, true, DateTime.UtcNow.AddDays(1), DateTime.UtcNow, "Success");
        var summary = new BackupSummaryDto(tenantId, "test-tenant", schedule, new List<BackupProviderDto>(), new List<BackupJobDto>());

        _backupServiceMock
            .Setup(s => s.GetBackupSummaryAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _controller.GetSummary(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<BackupSummaryDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("test-tenant", response.Data.Slug);
    }

    [Fact]
    public async Task GetSummary_WhenNotFound_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        _backupServiceMock
            .Setup(s => s.GetBackupSummaryAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BackupSummaryDto?)null);

        var result = await _controller.GetSummary(tenantId, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task TriggerBackup_WhenSuccessful_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var backupResponse = new TriggerBackupResponse(
            Guid.NewGuid(),
            "Queued",
            "Snapshot backup initiated.",
            0L,
            new List<string>(),
            DateTime.UtcNow);

        _backupServiceMock
            .Setup(s => s.TriggerBackupNowAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(backupResponse);

        var result = await _controller.TriggerBackup(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TriggerBackupResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Queued", response.Data.Status);
    }

    [Fact]
    public async Task TriggerBackup_WhenInvalidOperation_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();

        _backupServiceMock
            .Setup(s => s.TriggerBackupNowAsync(tenantId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Backup already running"));

        var result = await _controller.TriggerBackup(tenantId, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
    }
}
