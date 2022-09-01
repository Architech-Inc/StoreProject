using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Department
    {
        public Department()
        {
            Employees = new HashSet<Employee>();
        }

        public int DeptId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
