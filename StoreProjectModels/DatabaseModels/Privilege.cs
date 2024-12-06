using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Privilege
    {
        public Privilege()
        {
            CustomerPrivileges = new HashSet<CustomerPrivilege>();
            EmployeePrivileges = new HashSet<EmployeePrivilege>();
            UserPrivileges = new HashSet<UserPrivilege>();
        }

        public int PrivilegeId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public virtual ICollection<CustomerPrivilege> CustomerPrivileges { get; set; }
        public virtual ICollection<EmployeePrivilege> EmployeePrivileges { get; set; }
        public virtual ICollection<UserPrivilege> UserPrivileges { get; set; }
    }
}
