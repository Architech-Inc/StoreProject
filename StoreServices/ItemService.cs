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
	public class ItemService: IItemService
	{
		private readonly store_dbContext DbContext;
		public ItemService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddItem(Item item)
		{
			return Crud.Create<Item>(item.ItemId, item, DbContext);
		}

		public ResponseModel AddItems(ObservableCollection<Item> items)
		{
			return Crud.CreateRange<Item>(items, DbContext);
		}

		public ResponseModel DeleteItem(long itemId)
		{
			return Crud.Delete<Item>(itemId, DbContext);
		}

		public ResponseModel DeleteItems(ObservableCollection<Item> items)
		{
			//DbContext.Entry<ObservableCollection<Item>>(items).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
			return Crud.DeleteRange<Item>(items, DbContext);
		}

		public ObservableCollection<Item> GetAllItems()
		{
			return new(DbContext.Items);
		}

		public Item GetItem(long itemId)
		{
			return DbContext.Items.Find(itemId);
		}

		public ResponseModel UpdateItem(Item item)
		{
			return Crud.Update<Item>(item.ItemId, item, DbContext);
		}
	}
}
