using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerPrivilegeAction
    {
        public int CustomerPrivilegeActionId { get; set; }
        public int CustomerPrivilegeId { get; set; }
        public string ActionAuthorUserId { get; set; }
        public string Action { get; set; }
        public string Reason { get; set; }
        public DateTime ActionDate { get; set; }

        public virtual User ActionAuthorUser { get; set; }
        public virtual CustomerPrivilege CustomerPrivilege { get; set; }
    }
}
