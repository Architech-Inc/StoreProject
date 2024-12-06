using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerLocation
    {
        public int CustomerLocationId { get; set; }
        public string CustomerId { get; set; }
        public string LocationId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Location Location { get; set; }
    }
}
