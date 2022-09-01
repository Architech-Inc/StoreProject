using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Employee
    {
        public Employee()
        {
            Users = new HashSet<User>();
        }

        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int NidNumber { get; set; }
        public DateTime DateEmployed { get; set; }
        public int? DeptId { get; set; }
        public int? SalaryId { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Pob { get; set; }
        public DateTime Dob { get; set; }

        public virtual Department Dept { get; set; }
        public virtual Salary Salary { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
