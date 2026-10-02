namespace Store.Models.Configuration;

/// <summary>
/// MT-04 — Strongly typed configuration options for tenant-scoped or platform SMTP outbound email relays.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>SMTP relay host, e.g. smtp.sendgrid.net, smtp.mailgun.org, or mail.tenantdomain.com.</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>SMTP server port (typically 587 for STARTTLS, 465 for SSL, or 25).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Authentication username / API key user.</summary>
    public string? Username { get; set; }

    /// <summary>Authentication password or API secret key.</summary>
    public string? Password { get; set; }

    /// <summary>Sender envelope and RFC 5322 From email address.</summary>
    public string FromEmail { get; set; } = "noreply@clexanfoods.cm";

    /// <summary>Friendly sender display name (e.g. "ClexAn Foods - Bastos").</summary>
    public string FromName { get; set; } = "ClexAn Foods";

    /// <summary>Whether to use TLS/SSL encryption for the SMTP session.</summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// When false or host is empty, outbound emails log simulated dispatches without connecting over the network.
    /// In production environments, set to true to deliver real emails.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>Socket connection and send timeout in milliseconds (default 10s).</summary>
    public int TimeoutMs { get; set; } = 10000;
}
