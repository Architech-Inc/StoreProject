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
	public class SaleService : ISaleService
	{
		private readonly store_dbContext DbContext;
		public SaleService(store_dbContext store_DbContext)
		{
			DbContext = store_DbContext;
		}
		public ResponseModel AddSale(Sale sale)
		{
			return Crud.Create<Sale>(sale.SaleId, sale, DbContext);
		}

		public ResponseModel AddSales(ObservableCollection<Sale> sales)
		{
			return Crud.CreateRange<Sale>(sales, DbContext);
		}

		public ResponseModel DeleteSale(long saleId)
		{
			return Crud.Delete<Sale>(saleId, DbContext);
		}

		public ResponseModel DeleteSales(ObservableCollection<Sale> sales)
		{
			return Crud.DeleteRange<Sale>(sales, DbContext);
		}

		public ObservableCollection<Sale> GetAllSales()
		{
			return new ObservableCollection<Sale>(DbContext.Sales);
		}

		public Sale GetSale(long saleId)
		{
			return DbContext.Sales.Find(saleId);
		}

		public ResponseModel UpdateSale(Sale sale)
		{
			return Crud.Update(sale.SaleId, sale, DbContext);
		}
	}
}
