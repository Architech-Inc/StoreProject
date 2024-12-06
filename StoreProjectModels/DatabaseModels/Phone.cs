using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Phone
    {
        public Phone()
        {
            CustomerPhones = new HashSet<CustomerPhone>();
            EmployeePhones = new HashSet<EmployeePhone>();
            ManufacturerPhones = new HashSet<ManufacturerPhone>();
            SupplierPhones = new HashSet<SupplierPhone>();
            UserPhones = new HashSet<UserPhone>();
        }

        public long PhoneId { get; set; }
        public int CountryId { get; set; }
        public string Number { get; set; }
        public bool IsPrimary { get; set; }
        public bool CanNotify { get; set; }
        public bool IsVerified { get; set; }
        public string Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<CustomerPhone> CustomerPhones { get; set; }
        public virtual ICollection<EmployeePhone> EmployeePhones { get; set; }
        public virtual ICollection<ManufacturerPhone> ManufacturerPhones { get; set; }
        public virtual ICollection<SupplierPhone> SupplierPhones { get; set; }
        public virtual ICollection<UserPhone> UserPhones { get; set; }
    }
}
