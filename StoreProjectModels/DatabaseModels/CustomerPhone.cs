using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerPhone
    {
        public long CustomerPhoneId { get; set; }
        public string CustomerId { get; set; }
        public long PhoneId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Phone Phone { get; set; }
    }
}
