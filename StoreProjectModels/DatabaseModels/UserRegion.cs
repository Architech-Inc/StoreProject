using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class UserRegion
    {
        public int UserRegionId { get; set; }
        public string UserId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Region Region { get; set; }
        public virtual User User { get; set; }
    }
}
