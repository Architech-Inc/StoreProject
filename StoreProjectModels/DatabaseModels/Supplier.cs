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
        }

        public string SupplierId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string Code { get; set; }
        public string LogoBase64 { get; set; }

        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
    }
}
