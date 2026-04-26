using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Region : BaseEntity
{
    public int RegionId { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Country Country { get; set; } = null!;
    public ICollection<City> Cities { get; set; } = new List<City>();
}
