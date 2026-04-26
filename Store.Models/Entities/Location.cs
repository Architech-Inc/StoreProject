using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Location : BaseEntity
{
    public int LocationId { get; set; }
    public int? CityId { get; set; }
    public string? StreetAddress { get; set; }
    public string? PostalCode { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }

    public City? City { get; set; }
    public ICollection<EmployeeLocation> EmployeeLocations { get; set; } = new List<EmployeeLocation>();
    public ICollection<CustomerLocation> CustomerLocations { get; set; } = new List<CustomerLocation>();
    public ICollection<SupplierLocation> SupplierLocations { get; set; } = new List<SupplierLocation>();
    public ICollection<ManufacturerLocation> ManufacturerLocations { get; set; } = new List<ManufacturerLocation>();
}
