using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerLocation
    {
        public int ManufacturerLocationId { get; set; }
        public string ManufacturerId { get; set; }
        public string LocationId { get; set; }

        public virtual Location Location { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
