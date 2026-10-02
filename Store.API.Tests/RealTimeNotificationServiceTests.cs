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

    [Fact]
    public async Task NotifyCashVarianceAsync_SendsToRolesAndBranch_WhenDiscrepancyOccurs()
    {
        var dto = new CashVarianceAlertDto
        {
            ShiftId = Guid.NewGuid(),
            CashierUserId = Guid.NewGuid(),
            CashierName = "Cashier Sarah",
            BranchId = 3,
            BranchName = "Downtown",
            ExpectedAmount = 150000m,
            ActualAmount = 142000m,
            VarianceAmount = -8000m,
            Severity = "Danger",
            Notes = "Shortage at drawer close"
        };

        await _service.NotifyCashVarianceAsync(dto);

        // Verify typed event and activity notification sent to manager, admin, and branch group
        _mockClients.Verify(c => c.Group("role_manager"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group("role_admin"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group("branch_3"), Times.Exactly(2));
        _mockClientProxy.Verify(p => p.ReceiveCashVarianceAlert(dto), Times.Exactly(3));

        // Verify activity center notification sent
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.CashVariance &&
            n.Severity == "Danger" &&
            n.TargetUrl == "/CashVariance")), Times.Exactly(3));
    }

    [Fact]
    public async Task NotifyPurchaseOrderUpdateAsync_SendsToRolesBranchAndRequester()
    {
        var requesterId = Guid.NewGuid();
        var dto = new PurchaseOrderNotificationDto
        {
            PurchaseOrderId = 101,
            OrderNumber = "PO-2026-00101",
            SupplierName = "Acme Supplies",
            TotalAmount = 250000m,
            Status = "Approved",
            RequestedByUserId = requesterId,
            RequestedByName = "Buyer John",
            ApprovedByUserId = Guid.NewGuid(),
            ApprovedByName = "Manager Jane",
            BranchId = 5,
            BranchName = "North Branch"
        };

        await _service.NotifyPurchaseOrderUpdateAsync(dto);

        // Verify typed event and activity notification sent to manager, admin, branch, and requester
        _mockClients.Verify(c => c.Group("role_manager"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group("role_admin"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group("branch_5"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group($"user_{requesterId}"), Times.Exactly(2));
        _mockClientProxy.Verify(p => p.ReceivePurchaseOrderUpdate(dto), Times.Exactly(4));

        // Verify activity center notification sent with Success severity on Approved
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.PurchaseOrder &&
            n.Severity == "Success" &&
            n.TargetUrl == "/PurchaseOrders")), Times.Exactly(4));
    }

    [Fact]
    public async Task NotifyContactRequestAsync_SendsToRolesAndUser()
    {
        var userId = Guid.NewGuid();
        var dto = new ContactRequestNotificationDto
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
            Username = "cashier.doe",
            RequestType = "Phone",
            Status = "Approved",
            ReviewedByUserId = Guid.NewGuid()
        };

        await _service.NotifyContactRequestAsync(dto);

        // Verify typed event and activity notification sent to manager, admin, and user
        _mockClients.Verify(c => c.Group("role_manager"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group("role_admin"), Times.Exactly(2));
        _mockClients.Verify(c => c.Group($"user_{userId}"), Times.Exactly(2));
        _mockClientProxy.Verify(p => p.ReceiveContactRequestUpdate(dto), Times.Exactly(3));

        // Verify activity center notification sent
        _mockClientProxy.Verify(p => p.ReceiveNotification(It.Is<StoreNotificationDto>(n =>
            n.Category == NotificationCategory.ContactRequest &&
            n.Severity == "Success" &&
            n.TargetUrl == "/ContactRequests")), Times.Exactly(3));
    }
}
