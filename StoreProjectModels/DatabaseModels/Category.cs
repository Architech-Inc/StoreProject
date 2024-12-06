using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Category
    {
        public Category()
        {
            ItemCategories = new HashSet<ItemCategory>();
            Items = new HashSet<Item>();
        }

        public int CategoryId { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public int NumberOfProducts { get; set; }
        public string IconPath { get; set; }

        public virtual ICollection<ItemCategory> ItemCategories { get; set; }
        public virtual ICollection<Item> Items { get; set; }
    }
}
