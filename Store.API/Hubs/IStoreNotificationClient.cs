using Store.Models.DTOs.Notifications;

namespace Store.API.Hubs;

public interface IStoreNotificationClient
{
    Task ReceiveNotification(StoreNotificationDto notification);
    Task ReceiveDiscountOverrideUpdate(DiscountOverrideNotificationDto dto);
    Task ReceiveLowStockAlert(LowStockAlertDto dto);
}
