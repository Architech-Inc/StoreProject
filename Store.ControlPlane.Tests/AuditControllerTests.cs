using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class AuditControllerTests
{
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly Mock<ILogger<AuditController>> _loggerMock;
    private readonly AuditController _controller;

    public AuditControllerTests()
    {
        _auditServiceMock = new Mock<IAuditService>();
        _loggerMock = new Mock<ILogger<AuditController>>();
        _controller = new AuditController(_auditServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAuditTrail_ReturnsOkWithAuditLogs()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var sampleLogs = new List<TenantAuditDto>
        {
            new(Guid.NewGuid(), tenantId, DateTime.UtcNow, "system", "ProvisionTenant", "Tenant stack provisioned", "127.0.0.1"),
            new(Guid.NewGuid(), tenantId, DateTime.UtcNow.AddMinutes(-5), "admin", "RestartContainer", "Container 'api' restarted", "192.168.1.1")
        };

        _auditServiceMock
            .Setup(s => s.GetAuditTrailAsync(tenantId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleLogs);

        // Act
        var result = await _controller.GetAuditTrail(tenantId, 50, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<TenantAuditDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
        Assert.Equal("ProvisionTenant", response.Data[0].ActionType);
    }

    [Fact]
    public async Task GetAuditTrail_Empty_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _auditServiceMock
            .Setup(s => s.GetAuditTrailAsync(tenantId, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantAuditDto>());

        // Act
        var result = await _controller.GetAuditTrail(tenantId, 20, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<TenantAuditDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }
}
