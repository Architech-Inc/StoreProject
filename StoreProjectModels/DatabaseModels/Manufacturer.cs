using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Manufacturer
    {
        public Manufacturer()
        {
            Items = new HashSet<Item>();
            ManufacturerCountries = new HashSet<ManufacturerCountry>();
            ManufacturerEmails = new HashSet<ManufacturerEmail>();
            ManufacturerLocations = new HashSet<ManufacturerLocation>();
            ManufacturerPhones = new HashSet<ManufacturerPhone>();
        }

        public string ManufacturerId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string ManufacturerCode { get; set; }
        public string LogoPath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual ManufacturerCity ManufacturerCity { get; set; }
        public virtual ManufacturerRegion ManufacturerRegion { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<ManufacturerCountry> ManufacturerCountries { get; set; }
        public virtual ICollection<ManufacturerEmail> ManufacturerEmails { get; set; }
        public virtual ICollection<ManufacturerLocation> ManufacturerLocations { get; set; }
        public virtual ICollection<ManufacturerPhone> ManufacturerPhones { get; set; }
    }
}
