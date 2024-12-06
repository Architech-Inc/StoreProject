using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierCountry
    {
        public int SupplierCountryId { get; set; }
        public string SupplierId { get; set; }
        public int CountryId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Country Country { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
