using Store.Models.Entities.Base;
using Store.Models.Entities.Contacts;

namespace Store.Models.Entities;

public class Manufacturer : BaseEntity
{
    public Guid ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }

    // Navigation
    public ICollection<ManufacturerEmail> Emails { get; set; } = new List<ManufacturerEmail>();
    public ICollection<ManufacturerPhone> Phones { get; set; } = new List<ManufacturerPhone>();
    public ICollection<ManufacturerLocation> Locations { get; set; } = new List<ManufacturerLocation>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
}
