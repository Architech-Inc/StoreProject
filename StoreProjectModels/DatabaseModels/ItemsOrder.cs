using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ItemsOrder
    {
        public long OrderId { get; set; }
        public long ItemId { get; set; }
        public string UserId { get; set; }
        public string SupplierId { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ReceivedDate { get; set; }
        public float Cost { get; set; }
        public string Status { get; set; }

        public virtual Item Item { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual User User { get; set; }
    }
}
