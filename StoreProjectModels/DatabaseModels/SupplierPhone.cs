using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class SupplierPhone
    {
        public long SupplierPhoneId { get; set; }
        public string SupplierId { get; set; }
        public long PhoneId { get; set; }

        public virtual Phone Phone { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
