using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Invoice
    {
        public Invoice()
        {
            InvoiceTenders = new HashSet<InvoiceTender>();
            Sales = new HashSet<Sale>();
        }

        public string InvoiceId { get; set; }
        public string UserId { get; set; }
        public string CustomerId { get; set; }
        public string BankAccountNumber { get; set; }
        public float TotalAmount { get; set; }
        public DateTime DateRecorded { get; set; }
        public string PaymentType { get; set; }
        public string BankAccountName { get; set; }
        public string SalesIdList { get; set; }
        public float AmountTendered { get; set; }
        public bool Paid { get; set; }
        public DateTime LastModified { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<InvoiceTender> InvoiceTenders { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
