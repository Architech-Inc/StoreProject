using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ManufacturerPhone
    {
        public long ManufacturerPhoneId { get; set; }
        public string ManufacturerId { get; set; }
        public long PhoneId { get; set; }

        public virtual Manufacturer Manufacturer { get; set; }
        public virtual Phone Phone { get; set; }
    }
}
