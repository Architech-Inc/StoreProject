using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeeCity
    {
        public int EmployeeCityId { get; set; }
        public string EmployeeId { get; set; }
        public int CityId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual City City { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
