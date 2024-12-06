using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Sentmail
    {
        public int MailId { get; set; }
        public string UserId { get; set; }
        public string EmailBody { get; set; }
        public DateTime DateSent { get; set; }
        public string EmailType { get; set; }
    }
}
