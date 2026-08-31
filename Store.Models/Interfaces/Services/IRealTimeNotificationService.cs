using Store.Models.DTOs.Notifications;

namespace Store.Models.Interfaces.Services;

public interface IRealTimeNotificationService
{
    Task BroadcastNotificationAsync(StoreNotificationDto notification, CancellationToken ct = default);
    Task SendToUserAsync(Guid userId, StoreNotificationDto notification, CancellationToken ct = default);
    Task SendToRoleAsync(string roleName, StoreNotificationDto notification, CancellationToken ct = default);
    Task SendToBranchAsync(int branchId, StoreNotificationDto notification, CancellationToken ct = default);
    Task NotifyDiscountOverrideAsync(DiscountOverrideNotificationDto dto, CancellationToken ct = default);
    Task NotifyLowStockAsync(LowStockAlertDto dto, CancellationToken ct = default);
}
