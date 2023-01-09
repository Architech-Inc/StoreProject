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
	public interface IItemService
	{
		ResponseModel AddItem(Item item);
		ResponseModel AddItems(ObservableCollection<Item> items);
		ResponseModel UpdateItem(Item item);
		ResponseModel DeleteItem(long itemId);
		ResponseModel DeleteItems(ObservableCollection<Item> items);
		ObservableCollection<Item> GetAllItems();
		Item GetItem(long itemId);
	}
}
