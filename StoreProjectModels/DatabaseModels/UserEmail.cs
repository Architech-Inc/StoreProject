using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class UserEmail
    {
        public long UserEmailId { get; set; }
        public string UserId { get; set; }
        public long EmailId { get; set; }

        public virtual Email Email { get; set; }
        public virtual User User { get; set; }
    }
}
