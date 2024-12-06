using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Employee
    {
        public Employee()
        {
            EmployeeCountries = new HashSet<EmployeeCountry>();
            EmployeeEmails = new HashSet<EmployeeEmail>();
            EmployeeLocations = new HashSet<EmployeeLocation>();
            EmployeePhones = new HashSet<EmployeePhone>();
            EmployeePrivileges = new HashSet<EmployeePrivilege>();
            Users = new HashSet<User>();
        }

        public string EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SalaryId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int NidNumber { get; set; }
        public DateTime DateEmployed { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Department Department { get; set; }
        public virtual Salary Salary { get; set; }
        public virtual EmployeeCity EmployeeCity { get; set; }
        public virtual EmployeeRegion EmployeeRegion { get; set; }
        public virtual ICollection<EmployeeCountry> EmployeeCountries { get; set; }
        public virtual ICollection<EmployeeEmail> EmployeeEmails { get; set; }
        public virtual ICollection<EmployeeLocation> EmployeeLocations { get; set; }
        public virtual ICollection<EmployeePhone> EmployeePhones { get; set; }
        public virtual ICollection<EmployeePrivilege> EmployeePrivileges { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
