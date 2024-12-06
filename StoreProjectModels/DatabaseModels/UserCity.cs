using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class UserCity
    {
        public int UserCityId { get; set; }
        public string UserId { get; set; }
        public int CityId { get; set; }
        public DateTime DateSet { get; set; }

        public virtual City City { get; set; }
        public virtual User User { get; set; }
    }
}
