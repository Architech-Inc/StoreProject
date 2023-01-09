using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices.Interfaces
{
	public interface IInvoiceService
	{
		ResponseModel AddInvoice(Invoice invoice);
		ResponseModel UpdateInvoice(Invoice invoice);
		ResponseModel DeleteInvoice(long invoiceId);
		ObservableCollection<Invoice> GetAllInvoices();
		Invoice GetInvoice(long invoiceId);
	}
}
