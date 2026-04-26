using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class City : BaseEntity
{
    public int CityId { get; set; }
    public int RegionId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Region Region { get; set; } = null!;
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
