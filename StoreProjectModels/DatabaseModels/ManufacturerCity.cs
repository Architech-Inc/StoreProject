using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerCity
    {
        public int ManufacturerCityId { get; set; }
        public string ManufacturerId { get; set; }
        public int CityId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual City City { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
