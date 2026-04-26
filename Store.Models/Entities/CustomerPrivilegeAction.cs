using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class CustomerPrivilegeAction : BaseEntity
{
    public int CustomerPrivilegeActionId { get; set; }
    public int CustomerPrivilegeId { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public CustomerPrivilege CustomerPrivilege { get; set; } = null!;
    public User PerformedByUser { get; set; } = null!;
}
