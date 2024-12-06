using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Otp
    {
        public long OtpId { get; set; }
        public int Code { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string UserId { get; set; }
        public string Guid { get; set; }

        public virtual User User { get; set; }
    }
}
