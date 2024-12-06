using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class EmployeePrivilegeAction
    {
        public int EmployeePrivilegeActionId { get; set; }
        public int EmployeePrivilegeId { get; set; }
        public string ActionAuthorUserId { get; set; }
        public string Action { get; set; }
        public string Reason { get; set; }
        public DateTime ActionDate { get; set; }

        public virtual User ActionAuthorUser { get; set; }
        public virtual EmployeePrivilege EmployeePrivilege { get; set; }
    }
}
