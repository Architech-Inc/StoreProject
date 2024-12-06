using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Location
    {
        public Location()
        {
            CustomerLocations = new HashSet<CustomerLocation>();
            EmployeeLocations = new HashSet<EmployeeLocation>();
            ManufacturerLocations = new HashSet<ManufacturerLocation>();
            SupplierLocations = new HashSet<SupplierLocation>();
        }

        public string LocationId { get; set; }
        public string Name { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }

        public virtual ICollection<CustomerLocation> CustomerLocations { get; set; }
        public virtual ICollection<EmployeeLocation> EmployeeLocations { get; set; }
        public virtual ICollection<ManufacturerLocation> ManufacturerLocations { get; set; }
        public virtual ICollection<SupplierLocation> SupplierLocations { get; set; }
    }
}
