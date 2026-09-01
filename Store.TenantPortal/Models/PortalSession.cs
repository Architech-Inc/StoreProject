namespace Store.TenantPortal.Models;

public class PortalSession
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public string? TenantName { get; set; }
    public string SessionToken { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public bool HasTenant => TenantId.HasValue && !string.IsNullOrWhiteSpace(TenantSlug);
}
