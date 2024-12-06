using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerRegion
    {
        public int CustomerRegionId { get; set; }
        public string CustomerId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Region Region { get; set; }
    }
}
