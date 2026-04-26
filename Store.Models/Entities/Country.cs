using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Country : BaseEntity
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IsoCode { get; set; }
    public string? PhoneCode { get; set; }

    public ICollection<Region> Regions { get; set; } = new List<Region>();
}
