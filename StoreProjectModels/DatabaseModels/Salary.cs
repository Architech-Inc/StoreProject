using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Salary
    {
        public Salary()
        {
            Employees = new HashSet<Employee>();
        }

        public int SalaryId { get; set; }
        public float Amount { get; set; }
        public string Code { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}
