using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class ManufacturerLocation : BaseEntity
{
    public int ManufacturerLocationId { get; set; }
    public Guid ManufacturerId { get; set; }
    public int LocationId { get; set; }
    public bool IsPrimary { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;
    public Location Location { get; set; } = null!;
}
