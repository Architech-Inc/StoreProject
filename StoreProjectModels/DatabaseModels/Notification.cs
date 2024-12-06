using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Notification
    {
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public string Caption { get; set; }
        public string Details { get; set; }
        public string Actions { get; set; }
        public string Icon { get; set; }
        public bool MarkedAsRead { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateRead { get; set; }

        public virtual User User { get; set; }
    }
}
