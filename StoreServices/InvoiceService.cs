using StoreProjectModels.CRUD;
using StoreProjectModels.DatabaseModels;
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
		private readonly store_dbContext DbContext;
		public InvoiceService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddInvoice(Invoice invoice)
		{
			return Crud.Create<Invoice>(invoice.InvoiceId, invoice, DbContext);
		}

		public ResponseModel DeleteInvoice(long invoiceId)
		{
			return Crud.Delete<Invoice>(invoiceId, DbContext);
		}

		public ObservableCollection<Invoice> GetAllInvoices()
		{
			return new(DbContext.Invoices);
		}

		public Invoice GetInvoice(long invoiceId)
		{
			return DbContext.Invoices.Find(invoiceId);
		}

		public ResponseModel UpdateInvoice(Invoice invoice)
		{
			return Crud.Update<Invoice>(invoice.InvoiceId, invoice, DbContext);
		}
	}
}
