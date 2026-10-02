using Microsoft.Extensions.Logging;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class MockEmailService : IEmailService
{
    private readonly ISmtpEmailSender _smtpEmailSender;
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ISmtpEmailSender smtpEmailSender, ILogger<MockEmailService> logger)
    {
        _smtpEmailSender = smtpEmailSender;
        _logger = logger;
    }

    public async Task SendPurchaseOrderEmailAsync(PurchaseOrder po, string recipientEmail, CancellationToken ct = default)
    {
        var subject = $"Purchase Order {po.ReferenceNumber} — ClexAn Foods";
        var body = $"Purchase Order {po.ReferenceNumber} has been dispatched.\nItems Count: {po.Items.Count}\nStatus: {po.Status}";
        _logger.LogInformation("Dispatching Purchase Order {ReferenceNumber} email to {Recipient}", po.ReferenceNumber, recipientEmail);
        await _smtpEmailSender.SendEmailAsync(recipientEmail, subject, body, ct: ct);
    }
}
