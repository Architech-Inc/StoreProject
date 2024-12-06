using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerEmail
    {
        public long ManufacturerEmailId { get; set; }
        public string ManufacturerId { get; set; }
        public long EmailId { get; set; }

        public virtual Email Email { get; set; }
        public virtual Manufacturer Manufacturer { get; set; }
    }
}
