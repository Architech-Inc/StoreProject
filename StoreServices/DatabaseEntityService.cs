using Microsoft.EntityFrameworkCore;
using StoreProjectModels.CRUD;
using StoreProjectModels.Data;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.DbContexts;
using StoreProjectModels.Models;
using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices
{
	public class DatabaseEntityService: IDatabaseEntityService
	{
		private readonly StoreDbContext DbContext;
        public DatabaseEntityService(StoreDbContext storeContext)
		{
			DbContext = storeContext;
		}

		#region Entity
		public CrudResponse AddEntity<T>(T entity) where T : class
		{
			string name = typeof(T).Name;
			if (entity == null) return new(false, $"{name}Null");
			try
			{
				DbContext.Add<T>(entity);
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public CrudResponse UpdateEntity<T>(T entity) where T : class
		{
			string name = typeof(T).Name;
			if (entity == null) return new(false, $"{name}NotFound");
			try
			{
				DbContext.Entry<T>(entity).State = EntityState.Detached;
				//StoreContext.Entry<T>(entity).State = EntityState.Modified;
				DbContext.Update<T>(entity);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public CrudResponse DeleteEntity<T>(int id) where T : class
		{
			T obj = DbContext.Find<T>(id);
			string name = typeof(T).Name;
			if (obj == null) return new(false, $"{name}NotFound");
			try
			{
				DbContext.Entry<T>(obj).State = EntityState.Deleted;
				//DbContext.Remove<T>(obj);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public CrudResponse DeleteEntity<T>(long id) where T : class
		{
			T obj = DbContext.Find<T>(id);
			string name = typeof(T).Name;
			if (obj == null) return new(false, $"{name}NotFound");
			try
			{
				DbContext.Entry<T>(obj).State = EntityState.Deleted;
				//DbContext.Remove<T>(obj);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public CrudResponse DeleteEntity<T>(string id) where T : class
		{
			T obj = DbContext.Find<T>(id);
			string name = typeof(T).Name;
			if (obj == null) return new(false, $"{name}NotFound");
			try
			{
				DbContext.Entry<T>(obj).State = EntityState.Deleted;
				//DbContext.Remove<T>(obj);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		#endregion
	}
}
