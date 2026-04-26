using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Privilege : BaseEntity
{
    public int PrivilegeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;

    public ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
    public ICollection<EmployeePrivilege> EmployeePrivileges { get; set; } = new List<EmployeePrivilege>();
    public ICollection<CustomerPrivilege> CustomerPrivileges { get; set; } = new List<CustomerPrivilege>();
}
