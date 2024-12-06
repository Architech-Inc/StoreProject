using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Dsicount
    {
        public string DiscountId { get; set; }
        public string ItemId { get; set; }
        public string UserId { get; set; }
        public int Percentage { get; set; }
        public string Code { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public int Purchase { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Item Item { get; set; }
        public virtual User User { get; set; }
    }
}
