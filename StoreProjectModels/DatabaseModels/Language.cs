using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Language
    {
        public int LanguageId { get; set; }
        public int? CountryId { get; set; }
        public string Name { get; set; }

        public virtual Country Country { get; set; }
    }
}
