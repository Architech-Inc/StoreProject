using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Sale
    {
        public long SaleId { get; set; }
        public long ItemId { get; set; }
        public string UserId { get; set; }
        public long? InvoiceId { get; set; }
        public string ItemName { get; set; }
        public float UnitPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime SaleDate { get; set; }
        public float Amount { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Item Item { get; set; }
        public virtual User User { get; set; }
    }
}
