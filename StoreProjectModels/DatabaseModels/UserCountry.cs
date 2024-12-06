using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class UserCountry
    {
        public int UserCountryId { get; set; }
        public string UserId { get; set; }
        public int CountryId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual Country Country { get; set; }
        public virtual User User { get; set; }
    }
}
