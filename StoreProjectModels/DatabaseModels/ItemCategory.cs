using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ItemCategory
    {
        public int ItemCategoryId { get; set; }
        public string ItemId { get; set; }
        public int CategoryId { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual Category Category { get; set; }
        public virtual Item Item { get; set; }
    }
}
