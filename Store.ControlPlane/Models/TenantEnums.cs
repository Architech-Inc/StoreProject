namespace Store.ControlPlane.Models;

public enum TenantStatus
{
    Pending,
    Provisioning,
    Active,
    Suspended,
    Failed,
    Terminated
}

public enum TenantTier
{
    Starter,
    Professional,
    Enterprise
}
