using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeeEmail
    {
        public long EmployeeEmailId { get; set; }
        public string EmployeeId { get; set; }
        public long EmailId { get; set; }

        public virtual Email Email { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
