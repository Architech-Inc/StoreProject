using Store.Models.Entities.Base;

namespace Store.Models.Entities;
// Invoice is in the same namespace — no extra using needed

public class Branch : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserBranchRole> UserBranchRoles { get; set; } = new List<UserBranchRole>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
