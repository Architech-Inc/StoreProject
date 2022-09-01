using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Category
    {
        public Category()
        {
            Items = new HashSet<Item>();
        }

        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int NoP { get; set; }
        public string ImgBase64 { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
