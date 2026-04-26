using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class UserPrivilegeAction : BaseEntity
{
    public int UserPrivilegeActionId { get; set; }
    public int UserPrivilegeId { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public UserPrivilege UserPrivilege { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}
