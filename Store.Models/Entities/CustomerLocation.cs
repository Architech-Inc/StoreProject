using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class CustomerLocation : BaseEntity
{
    public int CustomerLocationId { get; set; }
    public Guid CustomerId { get; set; }
    public int LocationId { get; set; }
    public bool IsPrimary { get; set; }

    public Customer Customer { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
