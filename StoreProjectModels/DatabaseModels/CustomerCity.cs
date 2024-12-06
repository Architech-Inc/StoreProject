using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class CustomerCity
    {
        public int CustomerCityId { get; set; }
        public string CustomerId { get; set; }
        public int CityId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual City City { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
