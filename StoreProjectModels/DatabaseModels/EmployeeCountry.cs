using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeeCountry
    {
        public int EmployeeCountryId { get; set; }
        public string EmployeeId { get; set; }
        public int CountryId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Country Country { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
