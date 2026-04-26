using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class EmployeePrivilege : BaseEntity
{
    public int EmployeePrivilegeId { get; set; }
    public Guid EmployeeId { get; set; }
    public int PrivilegeId { get; set; }
    public PrivilegeType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public Employee Employee { get; set; } = null!;
    public Privilege Privilege { get; set; } = null!;
    public ICollection<EmployeePrivilegeAction> Actions { get; set; } = new List<EmployeePrivilegeAction>();
}
