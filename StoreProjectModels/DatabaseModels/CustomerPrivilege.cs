using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerPrivilege
    {
        public CustomerPrivilege()
        {
            CustomerPrivilegeActions = new HashSet<CustomerPrivilegeAction>();
        }

        public int CustomerPrivilegeId { get; set; }
        public string CustomerId { get; set; }
        public int PrivilegeId { get; set; }
        public DateTime DateGranted { get; set; }
        public DateTime DateRevoked { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Privilege Privilege { get; set; }
        public virtual ICollection<CustomerPrivilegeAction> CustomerPrivilegeActions { get; set; }
    }
}
