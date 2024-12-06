using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeePrivilege
    {
        public EmployeePrivilege()
        {
            EmployeePrivilegeActions = new HashSet<EmployeePrivilegeAction>();
        }

        public int EmployeePrivilegeId { get; set; }
        public string EmployeeId { get; set; }
        public int PrivilegeId { get; set; }
        public DateTime DateSet { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Privilege Privilege { get; set; }
        public virtual ICollection<EmployeePrivilegeAction> EmployeePrivilegeActions { get; set; }
    }
}
