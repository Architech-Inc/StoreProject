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

    public decimal PriceMultiplier { get; set; } = 1.0m;
    public decimal? TaxRateOverride { get; set; }

    public ICollection<UserBranchRole> UserBranchRoles { get; set; } = new List<UserBranchRole>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<BranchItemStock> ItemStocks { get; set; } = new List<BranchItemStock>();
    public ICollection<DiscountBranch> DiscountBranches { get; set; } = new List<DiscountBranch>();
    public ICollection<LoyaltyCampaignBranch> CampaignBranches { get; set; } = new List<LoyaltyCampaignBranch>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<PersonnelTransferHistory> OutgoingTransfers { get; set; } = new List<PersonnelTransferHistory>();
    public ICollection<PersonnelTransferHistory> IncomingTransfers { get; set; } = new List<PersonnelTransferHistory>();
}
