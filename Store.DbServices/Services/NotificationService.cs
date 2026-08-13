using Microsoft.Extensions.Logging;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class NotificationService : INotificationService
{
    private readonly ICommunicationLogService _logService;
    private readonly ILogger<NotificationService> _logger;
    private const int MaxRetries = 3;

    public NotificationService(ICommunicationLogService logService, ILogger<NotificationService> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, Guid? userId = null, CancellationToken ct = default)
    {
        var payload = $"Subject: {subject}\nBody: {body}";
        return await SendWithRetryAsync(toEmail, CommunicationChannel.Email, payload, userId, ct);
    }

    public async Task<bool> SendSmsAsync(string toNumber, string message, Guid? userId = null, CancellationToken ct = default)
    {
        return await SendWithRetryAsync(toNumber, CommunicationChannel.Sms, message, userId, ct);
    }

    public async Task<bool> SendWhatsAppAsync(string toNumber, string message, Guid? userId = null, CancellationToken ct = default)
    {
        return await SendWithRetryAsync(toNumber, CommunicationChannel.WhatsApp, message, userId, ct);
    }

    private async Task<bool> SendWithRetryAsync(string recipient, CommunicationChannel channel, string payload, Guid? userId, CancellationToken ct)
    {
        int attempt = 0;
        bool success = false;
        string? lastError = null;

        var log = new CommunicationLog
        {
            Recipient = recipient,
            Channel = channel,
            Payload = payload,
            UserId = userId,
            Status = CommunicationStatus.Pending
        };

        while (attempt < MaxRetries && !success)
        {
            attempt++;
            try
            {
                // Dummy sending logic for now since providers are not decided
                _logger.LogInformation("Attempt {Attempt} - Sending {Channel} to {Recipient}", attempt, channel, recipient);
                
                // Simulate network delay or transient failure
                await Task.Delay(500, ct); 
                
                // Random failure simulation for testing retries (remove in production)
                // if (Random.Shared.Next(10) < 3) throw new Exception("Transient provider error");

                success = true;
                log.Status = CommunicationStatus.Sent; // or Delivered based on provider webhook in future
                log.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                success = false;
                lastError = ex.Message;
                _logger.LogWarning(ex, "Attempt {Attempt} failed to send {Channel} to {Recipient}", attempt, channel, recipient);
                log.Status = CommunicationStatus.Retrying;
                log.ErrorMessage = $"Attempt {attempt}: {ex.Message}";
                
                if (attempt < MaxRetries)
                {
                    await Task.Delay(1000 * attempt, ct); // Exponential backoff
                }
            }
        }

        if (!success)
        {
            log.Status = CommunicationStatus.Failed;
            log.ErrorMessage = $"Failed after {MaxRetries} attempts. Last error: {lastError}";
        }

        log.RetryCount = attempt;
        await _logService.LogCommunicationAsync(log, ct);
        
        return success;
    }
}
