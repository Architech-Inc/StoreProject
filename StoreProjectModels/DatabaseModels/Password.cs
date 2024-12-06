using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Password
    {
        public int PasswordId { get; set; }
        public string UserId { get; set; }
        public string Hashed { get; set; }
        public byte[] Salt { get; set; }
        public DateTime LastChanged { get; set; }
        public string PastPasswords { get; set; }

        public virtual User User { get; set; }
    }
}
