using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierEmail
    {
        public long SupplierEmailId { get; set; }
        public string SupplierId { get; set; }
        public long EmailId { get; set; }

        public virtual Email Email { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
