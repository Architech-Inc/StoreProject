using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeeLocation
    {
        public int EmployeeLocationId { get; set; }
        public string EmployeeId { get; set; }
        public string LocationId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Location Location { get; set; }
    }
}
