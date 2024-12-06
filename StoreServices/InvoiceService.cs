using StoreProjectModels.CRUD;
using StoreProjectModels.Data;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.DbContexts;
using StoreProjectModels.Models;
using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices
{
	public class InvoiceService : IInvoiceService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public InvoiceService(StoreDbContext dbContext, ICrud crud)
        {
            DbContext = dbContext;
            Crud = crud;
        }

        public CrudResponse AddInvoice(Invoice invoice)
		{
			return Crud.CreateEntity<Invoice>(invoice.InvoiceId, invoice);
		}

		public CrudResponse DeleteInvoice(long invoiceId)
		{
			return Crud.DeleteEntity<Invoice>(invoiceId);
		}

		public ObservableCollection<Invoice> GetAllInvoices()
		{
			return new(DbContext.Invoices);
		}

		public Invoice GetInvoice(long invoiceId)
		{
			return DbContext.Invoices.Find(invoiceId);
		}

		public CrudResponse UpdateInvoice(Invoice invoice)
		{
			return Crud.UpdateEntity<Invoice>(invoice.InvoiceId, invoice);
		}
	}
}
