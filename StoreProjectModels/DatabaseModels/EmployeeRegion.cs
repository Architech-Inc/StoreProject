using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeeRegion
    {
        public int EmployeeRegionId { get; set; }
        public string EmployeeId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Region Region { get; set; }
    }
}
