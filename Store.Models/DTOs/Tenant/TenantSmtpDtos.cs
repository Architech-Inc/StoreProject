using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Tenant;

/// <summary>
/// MT-04 — Tenant SMTP configuration surfaced to administrative callers and portal UI.
/// Passwords are never sent plaintext over the wire; HasPassword and PasswordMasked
/// indicate presence of saved credentials.
/// </summary>
public record TenantSmtpConfigDto(
    Guid TenantId,
    string Slug,
    string Host,
    int Port,
    string? Username,
    string? PasswordMasked,
    bool HasPassword,
    string FromEmail,
    string? FromName,
    bool EnableSsl,
    bool IsEnabled,
    DateTime? LastTestedAt,
    string? LastTestStatus
);

/// <summary>
/// MT-04 — Request payload for configuring or rotating per-tenant custom SMTP credentials.
/// </summary>
public class UpdateTenantSmtpRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    [StringLength(120)]
    public string? Username { get; set; }

    /// <summary>
    /// Optional when updating an existing SMTP config: leaving this null or empty
    /// preserves the existing encrypted password.
    /// </summary>
    [StringLength(256)]
    public string? Password { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string FromEmail { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FromName { get; set; }

    public bool EnableSsl { get; set; } = true;

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// MT-04 — Request payload for validating tenant SMTP connectivity with a live test email.
/// </summary>
public class TestSmtpRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string RecipientEmail { get; set; } = string.Empty;
}

/// <summary>
/// MT-04 — Verification response detailing SMTP handshake outcome and round-trip latency.
/// </summary>
public record TestSmtpResponse(
    bool Success,
    string Message,
    long LatencyMs
);
