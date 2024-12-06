using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Customer
    {
        public Customer()
        {
            CustomerCountries = new HashSet<CustomerCountry>();
            CustomerEmails = new HashSet<CustomerEmail>();
            CustomerLocations = new HashSet<CustomerLocation>();
            CustomerPhones = new HashSet<CustomerPhone>();
            CustomerPrivileges = new HashSet<CustomerPrivilege>();
            Invoices = new HashSet<Invoice>();
        }

        public string CustomerId { get; set; }
        public string NidNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime LastModified { get; set; }
        public string ProfileImagePath { get; set; }

        public virtual CustomerCity CustomerCity { get; set; }
        public virtual CustomerRegion CustomerRegion { get; set; }
        public virtual ICollection<CustomerCountry> CustomerCountries { get; set; }
        public virtual ICollection<CustomerEmail> CustomerEmails { get; set; }
        public virtual ICollection<CustomerLocation> CustomerLocations { get; set; }
        public virtual ICollection<CustomerPhone> CustomerPhones { get; set; }
        public virtual ICollection<CustomerPrivilege> CustomerPrivileges { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}
