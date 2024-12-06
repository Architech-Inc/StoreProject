using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class InvoiceTender
    {
        public string InvoiceTenderId { get; set; }
        public string InvoiceId { get; set; }
        public float Amount { get; set; }
        public DateTime DateTendered { get; set; }
        public string TenderCode { get; set; }

        public virtual Invoice Invoice { get; set; }
    }
}
