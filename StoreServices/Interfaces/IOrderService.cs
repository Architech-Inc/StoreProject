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
	public interface IOrderService
	{
		CrudResponse AddItemsOrder(ItemsOrder itemsOrder);
		CrudResponse UpdateItemsOrder(ItemsOrder itemsOrder);
		CrudResponse DeleteItemsOrder(long itemsOrderId);
		ObservableCollection<ItemsOrder> GetAllItemsOrders();
		ItemsOrder GetItemsOrder(long itemsOrderId);
	}
}
