using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class UserPrivilege
    {
        public UserPrivilege()
        {
            UserPrivilegeActions = new HashSet<UserPrivilegeAction>();
        }

        public int UserPrivilegeId { get; set; }
        public string UserId { get; set; }
        public int PrivilegeId { get; set; }
        public DateTime DateGranted { get; set; }
        public DateTime DateRevoked { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Privilege Privilege { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<UserPrivilegeAction> UserPrivilegeActions { get; set; }
    }
}
