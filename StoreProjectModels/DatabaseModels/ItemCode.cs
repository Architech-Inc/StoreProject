using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ItemCode
    {
        public int ItemCodeId { get; set; }
        public string ItemId { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }

        public virtual Item Item { get; set; }
    }
}
