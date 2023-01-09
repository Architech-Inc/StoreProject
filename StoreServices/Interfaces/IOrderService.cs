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
		ResponseModel AddItemsOrder(ItemsOrder itemsOrder);
		ResponseModel UpdateItemsOrder(ItemsOrder itemsOrder);
		ResponseModel DeleteItemsOrder(long itemsOrderId);
		ObservableCollection<ItemsOrder> GetAllItemsOrders();
		ItemsOrder GetItemsOrder(long itemsOrderId);
	}
}
