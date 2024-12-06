using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class User
    {
        public User()
        {
            ChangeLogs = new HashSet<ChangeLog>();
            CustomerPrivilegeActions = new HashSet<CustomerPrivilegeAction>();
            Dsicounts = new HashSet<Dsicount>();
            EmployeePrivilegeActions = new HashSet<EmployeePrivilegeAction>();
            Invoices = new HashSet<Invoice>();
            ItemsOrders = new HashSet<ItemsOrder>();
            Notifications = new HashSet<Notification>();
            Otps = new HashSet<Otp>();
            Passwords = new HashSet<Password>();
            Sales = new HashSet<Sale>();
            UserCountries = new HashSet<UserCountry>();
            UserEmails = new HashSet<UserEmail>();
            UserPhones = new HashSet<UserPhone>();
            UserPrivilegeActions = new HashSet<UserPrivilegeAction>();
            UserPrivileges = new HashSet<UserPrivilege>();
        }

        public string UserId { get; set; }
        public string EmployeeId { get; set; }
        public int? RoleId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public string ImagePath { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Role Role { get; set; }
        public virtual UserCity UserCity { get; set; }
        public virtual UserRegion UserRegion { get; set; }
        public virtual UserToken UserToken { get; set; }
        public virtual ICollection<ChangeLog> ChangeLogs { get; set; }
        public virtual ICollection<CustomerPrivilegeAction> CustomerPrivilegeActions { get; set; }
        public virtual ICollection<Dsicount> Dsicounts { get; set; }
        public virtual ICollection<EmployeePrivilegeAction> EmployeePrivilegeActions { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<ItemsOrder> ItemsOrders { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Otp> Otps { get; set; }
        public virtual ICollection<Password> Passwords { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<UserCountry> UserCountries { get; set; }
        public virtual ICollection<UserEmail> UserEmails { get; set; }
        public virtual ICollection<UserPhone> UserPhones { get; set; }
        public virtual ICollection<UserPrivilegeAction> UserPrivilegeActions { get; set; }
        public virtual ICollection<UserPrivilege> UserPrivileges { get; set; }
    }
}
