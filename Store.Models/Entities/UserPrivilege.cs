using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class UserPrivilege : BaseEntity
{
    public int UserPrivilegeId { get; set; }
    public Guid UserId { get; set; }
    public int PrivilegeId { get; set; }
    public PrivilegeType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public Privilege Privilege { get; set; } = null!;
    public ICollection<UserPrivilegeAction> Actions { get; set; } = new List<UserPrivilegeAction>();
}
