using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface INotificationService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, Guid? userId = null, CancellationToken ct = default);
    Task<bool> SendSmsAsync(string toNumber, string message, Guid? userId = null, CancellationToken ct = default);
    Task<bool> SendWhatsAppAsync(string toNumber, string message, Guid? userId = null, CancellationToken ct = default);
}
