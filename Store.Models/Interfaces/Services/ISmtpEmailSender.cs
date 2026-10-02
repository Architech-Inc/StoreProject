namespace Store.Models.Interfaces.Services;

/// <summary>
/// MT-04 — Abstraction for outbound RFC 5321/5322 email transmission via configured SMTP relays.
/// </summary>
public interface ISmtpEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously over SMTP or simulates delivery if disabled in configuration.
    /// </summary>
    Task<bool> SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        string? fromEmail = null,
        string? fromName = null,
        bool isHtml = false,
        CancellationToken ct = default);
}
