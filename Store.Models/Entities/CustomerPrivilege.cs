using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class CustomerPrivilege : BaseEntity
{
    public int CustomerPrivilegeId { get; set; }
    public Guid CustomerId { get; set; }
    public int PrivilegeId { get; set; }
    public PrivilegeType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public Customer Customer { get; set; } = null!;
    public Privilege Privilege { get; set; } = null!;
    public ICollection<CustomerPrivilegeAction> Actions { get; set; } = new List<CustomerPrivilegeAction>();
}
