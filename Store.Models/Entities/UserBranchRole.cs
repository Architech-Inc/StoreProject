using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class UserBranchRole : BaseEntity
{
    public long UserBranchRoleId { get; set; }
    public Guid UserId { get; set; }
    public int BranchId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
