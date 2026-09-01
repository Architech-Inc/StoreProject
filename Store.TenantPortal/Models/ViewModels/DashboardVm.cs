using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Models.ViewModels;

public class DashboardVm
{
    public TenantDetailDto Tenant { get; set; } = null!;
    public PortalSession Session { get; set; } = null!;
    public bool AutoRefresh { get; set; } = true;
}
