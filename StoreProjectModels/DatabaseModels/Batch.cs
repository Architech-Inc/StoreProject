using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Batch
    {
        public string BatchId { get; set; }
        public string ItemId { get; set; }
        public string Code { get; set; }
        public string BatchCode { get; set; }
        public string BatchCodeType { get; set; }
        public DateTime ManufacturedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ImagePath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Item Item { get; set; }
    }
}
