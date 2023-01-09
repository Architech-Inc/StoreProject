using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Item
    {
        public Item()
        {
            ItemsOrders = new HashSet<ItemsOrder>();
            Sales = new HashSet<Sale>();
        }

        public long ItemId { get; set; }
        public int? CategoryId { get; set; }
        public int? UnitId { get; set; }
        public string ItemName { get; set; }
        public float UnitPrice { get; set; }
        public int InStock { get; set; }
        public string ItemType { get; set; }
        public int? ReoderLevel { get; set; }
        public int? DiscountPercentage { get; set; }
        public int? ItemCode { get; set; }
        public DateTime DateCreated { get; set; }
        public byte[] Image { get; set; }

        public virtual Category Category { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
