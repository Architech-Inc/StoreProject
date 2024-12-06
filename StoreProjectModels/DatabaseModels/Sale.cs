using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Sale
    {
        public string SaleId { get; set; }
        public string ItemId { get; set; }
        public string UserId { get; set; }
        public string InvoiceId { get; set; }
        public string Name { get; set; }
        public float UnitPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime DateOfSale { get; set; }
        public float Amount { get; set; }
        public string UnitCode { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Item Item { get; set; }
        public virtual User User { get; set; }
    }
}
