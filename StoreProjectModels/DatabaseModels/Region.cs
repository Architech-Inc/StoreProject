using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Region
    {
        public Region()
        {
            CustomerRegions = new HashSet<CustomerRegion>();
            EmployeeRegions = new HashSet<EmployeeRegion>();
            ManufacturerRegions = new HashSet<ManufacturerRegion>();
            SupplierRegions = new HashSet<SupplierRegion>();
            UserRegions = new HashSet<UserRegion>();
        }

        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public int CapitalCity { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<CustomerRegion> CustomerRegions { get; set; }
        public virtual ICollection<EmployeeRegion> EmployeeRegions { get; set; }
        public virtual ICollection<ManufacturerRegion> ManufacturerRegions { get; set; }
        public virtual ICollection<SupplierRegion> SupplierRegions { get; set; }
        public virtual ICollection<UserRegion> UserRegions { get; set; }
    }
}
