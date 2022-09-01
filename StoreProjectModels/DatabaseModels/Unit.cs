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
            Users = new HashSet<User>();
        }

        public int UnitId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public virtual ICollection<Item> Items { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
