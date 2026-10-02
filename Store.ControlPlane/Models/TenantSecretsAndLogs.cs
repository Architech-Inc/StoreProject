namespace Store.ControlPlane.Models;

public class TenantSecrets
{
    public string MySqlRootPassword { get; set; } = string.Empty;
    public string MySqlUserPassword { get; set; } = string.Empty;
    public string MongoDbRootPassword { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
    public string MoMoCallbackKey { get; set; } = string.Empty;
    public string OtpPepper { get; set; } = string.Empty;
    public string BackupEncryptionKey { get; set; } = string.Empty;

    // MT-04 — Per-tenant custom SMTP relay configuration
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SmtpFromEmail { get; set; } = string.Empty;
    public string SmtpFromName { get; set; } = string.Empty;
    public bool SmtpEnableSsl { get; set; } = true;
    public bool SmtpIsEnabled { get; set; } = false;
    public DateTime? SmtpLastTestedAt { get; set; }
    public string? SmtpLastTestStatus { get; set; }
}

public class TenantProvisioningLog
{
    public Guid LogId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
