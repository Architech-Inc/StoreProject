using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierRegion
    {
        public int SupplierRegionId { get; set; }
        public string SupplierId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Region Region { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
