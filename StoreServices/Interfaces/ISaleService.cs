using StoreProjectModels.Data;
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
		CrudResponse AddSale(Sale sale);
		CrudResponse AddSales(ObservableCollection<Sale> sales);
		CrudResponse UpdateSale(Sale sale);
		CrudResponse DeleteSale(long saleId);
		CrudResponse DeleteSales(ObservableCollection<Sale> sales);
		ObservableCollection<Sale> GetAllSales();
		Sale GetSale(long saleId);
	}
}
