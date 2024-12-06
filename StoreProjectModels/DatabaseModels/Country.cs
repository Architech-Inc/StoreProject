using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Country
    {
        public Country()
        {
            Cities = new HashSet<City>();
            Languages = new HashSet<Language>();
            Phones = new HashSet<Phone>();
            Regions = new HashSet<Region>();
        }

        public int CountryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DialCode { get; set; }
        public string Symbol { get; set; }
        public string Capital { get; set; }
        public string Currency { get; set; }
        public string Continent { get; set; }
        public string ContinentCode { get; set; }
        public string Alpha3 { get; set; }

        public virtual CustomerCountry CustomerCountry { get; set; }
        public virtual EmployeeCountry EmployeeCountry { get; set; }
        public virtual ManufacturerCountry ManufacturerCountry { get; set; }
        public virtual SupplierCountry SupplierCountry { get; set; }
        public virtual UserCountry UserCountry { get; set; }
        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Language> Languages { get; set; }
        public virtual ICollection<Phone> Phones { get; set; }
        public virtual ICollection<Region> Regions { get; set; }
    }
}
