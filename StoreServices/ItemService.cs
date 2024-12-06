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
	public class ItemService: IItemService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public ItemService(StoreDbContext dbContext, ICrud crud)
        {
            DbContext = dbContext;
            Crud = crud;
        }

        public CrudResponse AddItem(Item item)
		{
			return Crud.CreateEntity<Item>(item.ItemId, item);
		}

		public CrudResponse AddItems(ObservableCollection<Item> items)
		{
			return Crud.CreateEntities<Item>(nameof(Item.ItemId), items);
		}

		public CrudResponse DeleteItem(long itemId)
		{
			return Crud.DeleteEntity<Item>(itemId);
		}

		public CrudResponse DeleteItems(ObservableCollection<Item> items)
		{
			//DbContext.Entry<ObservableCollection<Item>>(items).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
			return Crud.DeleteEntities<Item>(items);
		}

		public ObservableCollection<Item> GetAllItems()
		{
			return new(DbContext.Items);
		}

		public Item GetItem(long itemId)
		{
			return DbContext.Items.Find(itemId);
		}

		public CrudResponse UpdateItem(Item item)
		{
			return Crud.UpdateEntity<Item>(item.ItemId, item);
		}
	}
}
