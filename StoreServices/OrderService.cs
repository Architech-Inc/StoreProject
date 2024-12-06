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
	public class OrderService : IOrderService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public OrderService(StoreDbContext dbContext, ICrud crud)
        {
            DbContext = dbContext;
            Crud = crud;
        }

        public CrudResponse AddItemsOrder(ItemsOrder itemsOrder)
		{
			return Crud.CreateEntity<ItemsOrder>(itemsOrder.OrderId, itemsOrder);
		}

		public CrudResponse DeleteItemsOrder(long itemsOrderId)
		{
			return Crud.DeleteEntity<ItemsOrder>(itemsOrderId);
		}

		public ObservableCollection<ItemsOrder> GetAllItemsOrders()
		{
			return new(DbContext.ItemsOrders);
		}

		public ItemsOrder GetItemsOrder(long itemsOrderId)
		{
			return DbContext.ItemsOrders.Find(itemsOrderId);
		}

		public CrudResponse UpdateItemsOrder(ItemsOrder itemsOrder)
		{
			return Crud.UpdateEntity<ItemsOrder>(itemsOrder.OrderId, itemsOrder);
		}
	}
}
