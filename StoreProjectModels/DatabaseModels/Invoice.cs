using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Invoice
    {
        public Invoice()
        {
            Sales = new HashSet<Sale>();
        }

        public long InvoiceId { get; set; }
        public string UserId { get; set; }
        public int BankAccountNumber { get; set; }
        public float TotalAmount { get; set; }
        public DateTime DateRecorded { get; set; }
        public string PaymentType { get; set; }
        public string BankAccountName { get; set; }
        public string CustomerId { get; set; }
        public string SalesIdList { get; set; }
        public float AmountTendered { get; set; }
        public bool Paid { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
