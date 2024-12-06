using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class City
    {
        public City()
        {
            CustomerCities = new HashSet<CustomerCity>();
            EmployeeCities = new HashSet<EmployeeCity>();
            ManufacturerCities = new HashSet<ManufacturerCity>();
            SupplierCities = new HashSet<SupplierCity>();
            UserCities = new HashSet<UserCity>();
        }

        public int CityId { get; set; }
        public int CountryId { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<CustomerCity> CustomerCities { get; set; }
        public virtual ICollection<EmployeeCity> EmployeeCities { get; set; }
        public virtual ICollection<ManufacturerCity> ManufacturerCities { get; set; }
        public virtual ICollection<SupplierCity> SupplierCities { get; set; }
        public virtual ICollection<UserCity> UserCities { get; set; }
    }
}
