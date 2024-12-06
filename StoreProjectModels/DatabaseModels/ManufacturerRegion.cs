using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerRegion
    {
        public int ManufacturerRegionId { get; set; }
        public string ManufacturerId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Region Region { get; set; }
    }
}
