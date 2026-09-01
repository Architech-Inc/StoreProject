namespace Store.ControlPlane.Models;

public class PortalAccount
{
    public Guid AccountId { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Format: {iterations}:{base64(salt)}:{base64(hash)}
    public Guid? TenantId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}
