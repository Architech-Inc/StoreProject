using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerCountry
    {
        public int ManufacturerCountryId { get; set; }
        public string ManufacturerId { get; set; }
        public int CountryId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Country Country { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
