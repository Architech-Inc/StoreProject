namespace Store.Models.Interfaces.Services;

public interface IEmailService
{
    Task SendPurchaseOrderEmailAsync(Entities.PurchaseOrder po, string recipientEmail, CancellationToken ct = default);
}
