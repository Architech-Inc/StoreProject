using Microsoft.AspNetCore.SignalR;
using Store.API.Hubs;
using Store.Models.DTOs.Notifications;
using Store.Models.Interfaces.Services;

namespace Store.API.Services;

public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<StoreNotificationHub, IStoreNotificationClient> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;

    public RealTimeNotificationService(
        IHubContext<StoreNotificationHub, IStoreNotificationClient> hubContext,
        ILogger<RealTimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastNotificationAsync(StoreNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.ReceiveNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast real-time notification.");
        }
    }

    public async Task SendToUserAsync(Guid userId, StoreNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}").ReceiveNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to user {UserId}.", userId);
        }
    }

    public async Task SendToRoleAsync(string roleName, StoreNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"role_{roleName.ToLowerInvariant()}").ReceiveNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to role {Role}.", roleName);
        }
    }

    public async Task SendToBranchAsync(int branchId, StoreNotificationDto notification, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"branch_{branchId}").ReceiveNotification(notification);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notification to branch {BranchId}.", branchId);
        }
    }

    public async Task NotifyDiscountOverrideAsync(DiscountOverrideNotificationDto dto, CancellationToken ct = default)
    {
        try
        {
            // Send specifically to the cashier user who requested it
            await _hubContext.Clients.Group($"user_{dto.CashierUserId}").ReceiveDiscountOverrideUpdate(dto);

            // If an active POS terminal session was associated, also target that POS terminal directly
            if (!string.IsNullOrWhiteSpace(dto.PosSessionId))
            {
                await _hubContext.Clients.Group($"pos_session_{dto.PosSessionId.Trim()}").ReceiveDiscountOverrideUpdate(dto);
            }

            // Also broadcast general notification to Managers and Admins
            var notif = new StoreNotificationDto
            {
                Title = $"Discount Override {dto.Status}",
                Message = $"Override #{dto.DiscountOverrideRequestId} ({dto.RequestedDiscount} discount) is {dto.Status.ToLowerInvariant()}.",
                Category = NotificationCategory.DiscountApproval,
                Severity = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "Success" : "Warning",
                TargetUrl = "/DiscountOverrides"
            };

            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send discount override notification.");
        }
    }

    public async Task NotifyLowStockAsync(LowStockAlertDto dto, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.ReceiveLowStockAlert(dto);

            var notif = new StoreNotificationDto
            {
                Title = "Low Stock Alert",
                Message = $"{dto.ItemName} has reached critical stock level ({dto.CurrentStock} left, reorder at {dto.ReorderLevel}).",
                Category = NotificationCategory.LowStock,
                Severity = "Warning",
                TargetUrl = $"/Catalog?search={Uri.EscapeDataString(dto.ItemName)}",
                ActionLabel = "Restock Now"
            };

            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
            if (dto.BranchId.HasValue)
            {
                await _hubContext.Clients.Group($"branch_{dto.BranchId.Value}").ReceiveNotification(notif);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send low stock notification.");
        }
    }

    public async Task NotifyRestockRecommendationAsync(RestockRecommendationNotificationDto dto, CancellationToken ct = default)
    {
        try
        {
            // 1. Push the structured payload to anyone subscribed to the restock UI
            //    (manager / admin clients that joined the relevant branch group).
            await _hubContext.Clients.All.ReceiveRestockRecommendation(dto);
            await _hubContext.Clients.Group($"branch_{dto.BranchId}").ReceiveRestockRecommendation(dto);

            // 2. Surface a generic toast in the notification bell so users who
            //    aren't on the restock page still see something happened.
            var severity = dto.Severity == "Critical" ? "Danger"
                         : dto.Severity == "High" ? "Warning"
                         : "Info";

            var notif = new StoreNotificationDto
            {
                Title = $"Restock alert · {dto.ItemName}",
                Message = $"Branch {dto.BranchName} needs {dto.RecommendedQuantity}× more. " +
                          (dto.DaysOfStock.HasValue ? $"~{dto.DaysOfStock.Value:0.#} days left. " : "") +
                          $"(Severity: {dto.Severity})",
                Category = NotificationCategory.RestockRecommendation,
                Severity = severity,
                TargetUrl = "/Restock",
                ActionLabel = "Open restock"
            };
            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send restock recommendation notification.");
        }
    }

    public async Task NotifyCashVarianceAsync(CashVarianceAlertDto dto, CancellationToken ct = default)
    {
        try
        {
            // 1. Dispatch typed event to managers, admins, and branch group if present
            await _hubContext.Clients.Group("role_manager").ReceiveCashVarianceAlert(dto);
            await _hubContext.Clients.Group("role_admin").ReceiveCashVarianceAlert(dto);
            if (dto.BranchId.HasValue)
            {
                await _hubContext.Clients.Group($"branch_{dto.BranchId.Value}").ReceiveCashVarianceAlert(dto);
            }

            // 2. Dispatch StoreNotificationDto for the activity center and live toast
            var shiftIdPrefix = dto.ShiftId.ToString("N")[..8];
            var varianceFormatted = $"{(dto.VarianceAmount > 0 ? "+" : "")}{dto.VarianceAmount:N0} XAF";
            var branchText = !string.IsNullOrWhiteSpace(dto.BranchName) ? $" · {dto.BranchName}" : "";
            var notif = new StoreNotificationDto
            {
                Title = $"Cash Variance Alert{branchText}",
                Message = $"Shift #{shiftIdPrefix} closed by {dto.CashierName} with {varianceFormatted} variance (Expected: {dto.ExpectedAmount:N0} XAF, Actual: {dto.ActualAmount:N0} XAF).",
                Category = NotificationCategory.CashVariance,
                Severity = dto.Severity,
                TargetUrl = "/CashVariance",
                ActionLabel = "Review Variance"
            };

            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
            if (dto.BranchId.HasValue)
            {
                await _hubContext.Clients.Group($"branch_{dto.BranchId.Value}").ReceiveNotification(notif);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send cash variance alert notification.");
        }
    }

    public async Task NotifyPurchaseOrderUpdateAsync(PurchaseOrderNotificationDto dto, CancellationToken ct = default)
    {
        try
        {
            // 1. Dispatch typed event to managers, admins, requesting user, and branch
            await _hubContext.Clients.Group("role_manager").ReceivePurchaseOrderUpdate(dto);
            await _hubContext.Clients.Group("role_admin").ReceivePurchaseOrderUpdate(dto);
            if (dto.BranchId.HasValue)
            {
                await _hubContext.Clients.Group($"branch_{dto.BranchId.Value}").ReceivePurchaseOrderUpdate(dto);
            }
            if (dto.RequestedByUserId.HasValue && dto.RequestedByUserId != Guid.Empty)
            {
                await _hubContext.Clients.Group($"user_{dto.RequestedByUserId.Value}").ReceivePurchaseOrderUpdate(dto);
            }

            // 2. Dispatch StoreNotificationDto for the activity center and live toast
            var severity = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "Success"
                         : dto.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ? "Warning"
                         : "Info";

            var orderDisplay = !string.IsNullOrWhiteSpace(dto.OrderNumber) ? dto.OrderNumber : dto.PurchaseOrderId.ToString();
            var supplierText = !string.IsNullOrWhiteSpace(dto.SupplierName) ? $" ({dto.SupplierName})" : "";
            var defaultMsg = $"PO #{orderDisplay}{supplierText} transitioned to {dto.Status}. Total: {dto.TotalAmount:N0} XAF.";

            var notif = new StoreNotificationDto
            {
                Title = $"Purchase Order #{orderDisplay} · {dto.Status}",
                Message = !string.IsNullOrWhiteSpace(dto.Message) ? dto.Message : defaultMsg,
                Category = NotificationCategory.PurchaseOrder,
                Severity = severity,
                TargetUrl = "/PurchaseOrders",
                ActionLabel = "View Order"
            };

            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
            if (dto.BranchId.HasValue)
            {
                await _hubContext.Clients.Group($"branch_{dto.BranchId.Value}").ReceiveNotification(notif);
            }
            if (dto.RequestedByUserId.HasValue && dto.RequestedByUserId != Guid.Empty)
            {
                await _hubContext.Clients.Group($"user_{dto.RequestedByUserId.Value}").ReceiveNotification(notif);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send purchase order notification.");
        }
    }

    public async Task NotifyContactRequestAsync(ContactRequestNotificationDto dto, CancellationToken ct = default)
    {
        try
        {
            // 1. Dispatch typed event to managers, admins, and requesting user
            await _hubContext.Clients.Group("role_manager").ReceiveContactRequestUpdate(dto);
            await _hubContext.Clients.Group("role_admin").ReceiveContactRequestUpdate(dto);
            if (dto.UserId != Guid.Empty)
            {
                await _hubContext.Clients.Group($"user_{dto.UserId}").ReceiveContactRequestUpdate(dto);
            }

            // 2. Dispatch StoreNotificationDto for the activity center and live toast
            var severity = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "Success"
                         : dto.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ? "Warning"
                         : "Info";

            var notif = new StoreNotificationDto
            {
                Title = $"Contact Request {dto.Status}",
                Message = $"Contact change request for {dto.Username} was {dto.Status.ToLowerInvariant()}.",
                Category = NotificationCategory.ContactRequest,
                Severity = severity,
                TargetUrl = "/ContactRequests",
                ActionLabel = "View Requests"
            };

            await _hubContext.Clients.Group("role_manager").ReceiveNotification(notif);
            await _hubContext.Clients.Group("role_admin").ReceiveNotification(notif);
            if (dto.UserId != Guid.Empty)
            {
                await _hubContext.Clients.Group($"user_{dto.UserId}").ReceiveNotification(notif);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send contact request notification.");
        }
    }
}
