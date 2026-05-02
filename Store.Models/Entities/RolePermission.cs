using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class RolePermission : BaseEntity
{
    public long RolePermissionId { get; set; }
    public int RoleId { get; set; }
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; } = true;

    public Role Role { get; set; } = null!;
}
