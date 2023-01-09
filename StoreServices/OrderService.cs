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
	public class OrderService : IOrderService
	{
		private readonly store_dbContext DbContext;
		public OrderService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddItemsOrder(ItemsOrder itemsOrder)
		{
			return Crud.Create<ItemsOrder>(itemsOrder.OrderId, itemsOrder, DbContext);
		}

		public ResponseModel DeleteItemsOrder(long itemsOrderId)
		{
			return Crud.Delete<ItemsOrder>(itemsOrderId, DbContext);
		}

		public ObservableCollection<ItemsOrder> GetAllItemsOrders()
		{
			return new(DbContext.ItemsOrders);
		}

		public ItemsOrder GetItemsOrder(long itemsOrderId)
		{
			return DbContext.ItemsOrders.Find(itemsOrderId);
		}

		public ResponseModel UpdateItemsOrder(ItemsOrder itemsOrder)
		{
			return Crud.Update<ItemsOrder>(itemsOrder.OrderId, itemsOrder, DbContext);
		}
	}
}
