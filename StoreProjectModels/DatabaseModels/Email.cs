using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Email
    {
        public Email()
        {
            CustomerEmails = new HashSet<CustomerEmail>();
            EmployeeEmails = new HashSet<EmployeeEmail>();
            ManufacturerEmails = new HashSet<ManufacturerEmail>();
            SupplierEmails = new HashSet<SupplierEmail>();
            UserEmails = new HashSet<UserEmail>();
        }

        public long EmailId { get; set; }
        public string Address { get; set; }
        public bool IsPrimary { get; set; }
        public bool CanMailTo { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsVerified { get; set; }

        public virtual ICollection<CustomerEmail> CustomerEmails { get; set; }
        public virtual ICollection<EmployeeEmail> EmployeeEmails { get; set; }
        public virtual ICollection<ManufacturerEmail> ManufacturerEmails { get; set; }
        public virtual ICollection<SupplierEmail> SupplierEmails { get; set; }
        public virtual ICollection<UserEmail> UserEmails { get; set; }
    }
}
