using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class EmployeePrivilegeAction : BaseEntity
{
    public int EmployeePrivilegeActionId { get; set; }
    public int EmployeePrivilegeId { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public EmployeePrivilege EmployeePrivilege { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}
