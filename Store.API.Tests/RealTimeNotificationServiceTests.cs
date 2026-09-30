using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Store.API.Hubs;
using Store.API.Services;
using Store.Models.DTOs.Notifications;
using Xunit;

namespace Store.API.Tests;

public class RealTimeNotificationServiceTests
{
    private readonly Mock<IHubContext<StoreNotificationHub, IStoreNotificationClient>> _mockHubContext;
    private readonly Mock<IHubClients<IStoreNotificationClient>> _mockClients;
    private readonly Mock<IStoreNotificationClient> _mockClientProxy;
    private readonly Mock<ILogger<RealTimeNotificationService>> _mockLogger;
    private readonly RealTimeNotificationService _service;

    public RealTimeNotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<StoreNotificationHub, IStoreNotificationClient>>();
        _mockClients = new Mock<IHubClients<IStoreNotificationClient>>();
        _mockClientProxy = new Mock<IStoreNotificationClient>();
        _mockLogger = new Mock<ILogger<RealTimeNotificationService>>();

        _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);

        _service = new RealTimeNotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task NotifyDiscountOverrideAsync_SendsToCashierAndPosSession_WhenPosSessionIdProvided()
    {
        var cashierId = Guid.NewGuid();
        var posSessionId = "pos_session_abc123";

        var dto = new DiscountOverrideNotificationDto
        {
            DiscountOverrideRequestId = 42,
            ItemId = Guid.NewGuid(),
            PosSessionId = posSessionId,
            CashierUserId = cashierId,
            Status = "Approved",
            RequestedDiscount = 1500m,
            Reason = "Manager approved discount"
        };

        await _service.NotifyDiscountOverrideAsync(dto);

        // Verify sent to cashier's personal user group
        _mockClients.Verify(c => c.Group($"user_{cashierId}"), Times.Once);

        // Verify sent directly to POS terminal session group (GAP-28)
        _mockClients.Verify(c => c.Group($"pos_session_{posSessionId}"), Times.Once);

        // Verify payload dispatched
        _mockClientProxy.Verify(p => p.ReceiveDiscountOverrideUpdate(dto), Times.Exactly(2));

        // Verify broadcast to managers and admins
        _mockClients.Verify(c => c.Group("role_manager"), Times.Once);
        _mockClients.Verify(c => c.Group("role_admin"), Times.Once);
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.DiscountApproval &&
            n.Severity == "Success")), Times.Exactly(2));
    }

    [Fact]
    public async Task NotifyDiscountOverrideAsync_OmitsPosSessionGroup_WhenPosSessionIdMissing()
    {
        var cashierId = Guid.NewGuid();

        var dto = new DiscountOverrideNotificationDto
        {
            DiscountOverrideRequestId = 43,
            ItemId = Guid.NewGuid(),
            PosSessionId = null,
            CashierUserId = cashierId,
            Status = "Rejected",
            RequestedDiscount = 500m,
            Reason = "Price too low"
        };

        await _service.NotifyDiscountOverrideAsync(dto);

        // Verify sent to cashier
        _mockClients.Verify(c => c.Group($"user_{cashierId}"), Times.Once);

        // Verify NOT sent to any pos_session group
        _mockClients.Verify(c => c.Group(It.Is<string>(g => g.StartsWith("pos_session_"))), Times.Never);

        // Verify notification severity is Warning on Rejected
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.DiscountApproval &&
            n.Severity == "Warning")), Times.Exactly(2));
    }

    [Fact]
    public async Task NotifyDiscountOverrideAsync_HandlesHubExceptionGracefully()
    {
        _mockClients.Setup(c => c.Group(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Hub connection broken"));

        var dto = new DiscountOverrideNotificationDto
        {
            DiscountOverrideRequestId = 44,
            CashierUserId = Guid.NewGuid(),
            Status = "Approved"
        };

        // Must not throw
        await _service.NotifyDiscountOverrideAsync(dto);
    }

    [Fact]
    public async Task NotifyLowStockAsync_SendsToAllAndRoleGroups()
    {
        var alert = new LowStockAlertDto
        {
            ItemId = Guid.NewGuid(),
            ItemName = "Organic Jasmine Rice 5kg",
            Barcode = "200000000001",
            CurrentStock = 3,
            ReorderLevel = 10,
            BranchId = 2
        };

        await _service.NotifyLowStockAsync(alert);

        _mockClientProxy.Verify(p => p.ReceiveLowStockAlert(alert), Times.Once);
        _mockClients.Verify(c => c.Group("role_manager"), Times.Once);
        _mockClients.Verify(c => c.Group("role_admin"), Times.Once);
        _mockClients.Verify(c => c.Group("branch_2"), Times.Once);
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.LowStock &&
            n.Severity == "Warning" &&
            n.Title == "Low Stock Alert")), Times.Exactly(3));
    }

    [Fact]
    public async Task SendToRoleAsync_SendsNotificationToTargetGroup()
    {
        var notif = new StoreNotificationDto
        {
            Title = "Contact Request Approved",
            Message = "Request approved by admin.",
            Category = NotificationCategory.ContactRequest,
            Severity = "Success"
        };

        await _service.SendToRoleAsync("Manager", notif);

        _mockClients.Verify(c => c.Group("role_manager"), Times.Once);
        _mockClientProxy.Verify(p => p.ReceiveNotification(notif), Times.Once);
    }
}
