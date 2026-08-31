namespace Store.ControlPlane.Models;

public class TenantSecrets
{
    public string MySqlRootPassword { get; set; } = string.Empty;
    public string MySqlUserPassword { get; set; } = string.Empty;
    public string MongoDbRootPassword { get; set; } = string.Empty;
    public string JwtSecret { get; set; } = string.Empty;
    public string MoMoCallbackKey { get; set; } = string.Empty;
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
