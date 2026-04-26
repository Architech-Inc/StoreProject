using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Notification : BaseEntity
{
    public int NotificationId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
}
