using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class User
    {
        public User()
        {
            Invoices = new HashSet<Invoice>();
            ItemsOrders = new HashSet<ItemsOrder>();
            Sales = new HashSet<Sale>();
        }

        public string UserId { get; set; }
        public string EmployeeId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? UnitId { get; set; }
        public string AccountType { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
