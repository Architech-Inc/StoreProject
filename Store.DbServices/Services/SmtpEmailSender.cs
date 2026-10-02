using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Store.Models.Configuration;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

/// <summary>
/// MT-04 — Production SMTP email client supporting custom tenant relays, STARTTLS/SSL, and graceful dev simulation.
/// </summary>
public class SmtpEmailSender : ISmtpEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options?.Value ?? new SmtpOptions();
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        string? fromEmail = null,
        string? fromName = null,
        bool isHtml = false,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email address must not be empty.", nameof(toEmail));

        // When SMTP is unconfigured or disabled, log simulated dispatch (safe fallback for dev and offline tests)
        if (!_options.IsEnabled || string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogInformation(
                "SMTP relay disabled or host unconfigured. Simulated email dispatched to {Recipient} (Subject: {Subject})",
                toEmail,
                subject);
            return true;
        }

        var effectiveFromEmail = !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : _options.FromEmail;
        var effectiveFromName = !string.IsNullOrWhiteSpace(fromName) ? fromName : _options.FromName;

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Timeout = _options.TimeoutMs > 0 ? _options.TimeoutMs : 10000,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        using var mail = new MailMessage
        {
            From = new MailAddress(effectiveFromEmail, effectiveFromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        mail.To.Add(toEmail.Trim());

        _logger.LogInformation(
            "Transmitting email via SMTP {Host}:{Port} (SSL: {EnableSsl}) to {Recipient}",
            _options.Host,
            _options.Port,
            _options.EnableSsl,
            toEmail);

        await client.SendMailAsync(mail, ct);
        return true;
    }
}
