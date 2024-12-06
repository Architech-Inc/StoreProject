using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerCountry
    {
        public int CustomerCountryId { get; set; }
        public string CustomerId { get; set; }
        public int CountryId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Country Country { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
