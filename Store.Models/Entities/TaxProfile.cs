using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class TaxProfile : BaseEntity
{
    public int TaxProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal RatePercent { get; set; }
    public TaxApplicationType ApplicationType { get; set; } = TaxApplicationType.Exclusive;
    public bool IsActive { get; set; } = true;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
