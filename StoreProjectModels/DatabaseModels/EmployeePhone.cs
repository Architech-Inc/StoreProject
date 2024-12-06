using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeePhone
    {
        public long EmployeePhoneId { get; set; }
        public string EmployeeId { get; set; }
        public long PhoneId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Phone Phone { get; set; }
    }
}
