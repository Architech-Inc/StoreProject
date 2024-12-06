using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierCity
    {
        public int SupplierCityId { get; set; }
        public string SupplierId { get; set; }
        public int CityId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual City City { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
