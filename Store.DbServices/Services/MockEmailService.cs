using Microsoft.Extensions.Logging;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPurchaseOrderEmailAsync(PurchaseOrder po, string recipientEmail, CancellationToken ct = default)
    {
        // Mock email sending
        _logger.LogInformation($"[EMAIL MOCK] Sending Purchase Order {po.ReferenceNumber} to {recipientEmail}. Item Count: {po.Items.Count}");
        return Task.CompletedTask;
    }
}
