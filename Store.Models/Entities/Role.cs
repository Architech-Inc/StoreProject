using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Role : BaseEntity
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}
