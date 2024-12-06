using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Supplier
    {
        public Supplier()
        {
            ItemsOrders = new HashSet<ItemsOrder>();
            SupplierCountries = new HashSet<SupplierCountry>();
            SupplierEmails = new HashSet<SupplierEmail>();
            SupplierLocations = new HashSet<SupplierLocation>();
            SupplierPhones = new HashSet<SupplierPhone>();
        }

        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string SupplierCode { get; set; }
        public string LogoPath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual SupplierCity SupplierCity { get; set; }
        public virtual SupplierRegion SupplierRegion { get; set; }
        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
        public virtual ICollection<SupplierCountry> SupplierCountries { get; set; }
        public virtual ICollection<SupplierEmail> SupplierEmails { get; set; }
        public virtual ICollection<SupplierLocation> SupplierLocations { get; set; }
        public virtual ICollection<SupplierPhone> SupplierPhones { get; set; }
    }
}
