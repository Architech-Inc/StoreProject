using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierLocation
    {
        public int SupplierLocationId { get; set; }
        public string SupplierId { get; set; }
        public string LocationId { get; set; }

        public virtual Location Location { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
