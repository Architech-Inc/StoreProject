using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ItemExpiry
    {
        public int ItemExpiryId { get; set; }
        public string ItemId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsEnforced { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Item Item { get; set; }
    }
}
