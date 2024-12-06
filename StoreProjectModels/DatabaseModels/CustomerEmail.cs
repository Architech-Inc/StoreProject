using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerEmail
    {
        public long CustomerEmailId { get; set; }
        public string CustomerId { get; set; }
        public long EmailId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Email Email { get; set; }
    }
}
