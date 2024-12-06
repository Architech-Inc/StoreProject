using Microsoft.EntityFrameworkCore;
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
	public class SaleService : ISaleService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public SaleService(StoreDbContext StoreDbContext, ICrud crud)
        {
            DbContext = StoreDbContext;
            Crud = crud;
        }
        public CrudResponse AddSale(Sale sale)
		{
			return Crud.CreateEntity<Sale>(sale.SaleId, sale);
		}

		public CrudResponse AddSales(ObservableCollection<Sale> sales)
		{
			return Crud.CreateEntities<Sale>(nameof(Sale.SaleId), sales);
		}

		public CrudResponse DeleteSale(long saleId)
		{
			return Crud.DeleteEntity<Sale>(saleId);
		}

		public CrudResponse DeleteSales(ObservableCollection<Sale> sales)
		{
			return Crud.DeleteEntities<Sale>(sales);
		}

		public ObservableCollection<Sale> GetAllSales()
		{
			return new ObservableCollection<Sale>(DbContext.Sales);
		}

		public Sale GetSale(long saleId)
		{
			return DbContext.Sales.Find(saleId);
		}

		public CrudResponse UpdateSale(Sale sale)
		{
			return Crud.UpdateEntity(sale.SaleId, sale);
		}
	}
}
