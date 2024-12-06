using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Unit
    {
        public Unit()
        {
            Items = new HashSet<Item>();
        }

        public int UnitId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string UnitCode { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
