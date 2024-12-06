using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class ChangeLog
    {
        public string ChangeLogId { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
        public string Link { get; set; }
        public int DateLogged { get; set; }

        public virtual User User { get; set; }
    }
}
