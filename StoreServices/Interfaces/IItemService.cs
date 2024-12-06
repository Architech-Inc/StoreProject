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
	public interface IItemService
	{
		CrudResponse AddItem(Item item);
		CrudResponse AddItems(ObservableCollection<Item> items);
		CrudResponse UpdateItem(Item item);
		CrudResponse DeleteItem(long itemId);
		CrudResponse DeleteItems(ObservableCollection<Item> items);
		ObservableCollection<Item> GetAllItems();
		Item GetItem(long itemId);
	}
}
