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
	public interface ISaleService
	{
		ResponseModel AddSale(Sale sale);
		ResponseModel AddSales(ObservableCollection<Sale> sales);
		ResponseModel UpdateSale(Sale sale);
		ResponseModel DeleteSale(long saleId);
		ResponseModel DeleteSales(ObservableCollection<Sale> sales);
		ObservableCollection<Sale> GetAllSales();
		Sale GetSale(long saleId);
	}
}
