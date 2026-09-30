using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.API.Application.Abstractions;
using Store.API.Controllers;
using Store.Models.DTOs.Notifications;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class ContactChangeNotificationTests
{
    private readonly Mock<IRequestDispatcher> _mockDispatcher;
    private readonly Mock<ISystemSettingService> _mockSettings;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly Mock<IRealTimeNotificationService> _mockNotifications;
    private readonly UsersController _controller;

    public ContactChangeNotificationTests()
    {
        _mockDispatcher = new Mock<IRequestDispatcher>();
        _mockSettings = new Mock<ISystemSettingService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _mockNotifications = new Mock<IRealTimeNotificationService>();

        _controller = new UsersController(
            _mockDispatcher.Object,
            _mockSettings.Object,
            _mockUserService.Object,
            _mockLogger.Object,
            _mockNotifications.Object);

        var adminId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("uid", adminId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task VerifyContactChange_DispatchesNotificationToManagerAndAdmin_WhenTokenValid()
    {
        _mockUserService.Setup(s => s.VerifyContactChangeAsync("valid-token-12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.VerifyContactChange("valid-token-12345", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Manager", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Admin", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveContactChange_DispatchesSuccessNotificationToRoles()
    {
        var requestId = Guid.NewGuid();
        _mockUserService.Setup(s => s.ApproveContactChangeAsync(requestId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ApproveContactChange(requestId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Manager", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.Severity == "Success" &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Admin", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.Severity == "Success" &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectContactChange_DispatchesWarningNotificationToRoles()
    {
        var requestId = Guid.NewGuid();
        _mockUserService.Setup(s => s.RejectContactChangeAsync(requestId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.RejectContactChange(requestId, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Manager", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.Severity == "Warning" &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
        _mockNotifications.Verify(n => n.SendToRoleAsync("Admin", It.Is<StoreNotificationDto>(dto =>
            dto.Category == NotificationCategory.ContactRequest &&
            dto.Severity == "Warning" &&
            dto.TargetUrl == "/ContactRequests"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
