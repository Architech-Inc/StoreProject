using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Item
    {
        public Item()
        {
            Batches = new HashSet<Batch>();
            ItemCategories = new HashSet<ItemCategory>();
            ItemCodes = new HashSet<ItemCode>();
            ItemsOrders = new HashSet<ItemsOrder>();
            Sales = new HashSet<Sale>();
        }

        public string ItemId { get; set; }
        public int? CategoryId { get; set; }
        public int? UnitId { get; set; }
        public string ManufacturerId { get; set; }
        public string ItemName { get; set; }
        public float UnitPrice { get; set; }
        public int InStock { get; set; }
        public string Type { get; set; }
        public int? ReorderLevel { get; set; }
        public int? DiscountPercentage { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string ImagePath { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Category Category { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual Dsicount Dsicount { get; set; }
        public virtual ItemExpiry ItemExpiry { get; set; }
        public virtual ICollection<Batch> Batches { get; set; }
        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<ItemCode> ItemCodes { get; set; }
        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
