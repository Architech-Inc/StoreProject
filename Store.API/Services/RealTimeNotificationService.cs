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
            // Send specifically to the cashier who requested it
            await _hubContext.Clients.Group($"user_{dto.CashierUserId}").ReceiveDiscountOverrideUpdate(dto);

            // Also broadcast general notification to Cashiers and Managers
            var notif = new StoreNotificationDto
            {
                Title = $"Discount Override {dto.Status}",
                Message = $"Override request for {dto.RequestedDiscount:C} is {dto.Status.ToLowerInvariant()}.",
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
}
