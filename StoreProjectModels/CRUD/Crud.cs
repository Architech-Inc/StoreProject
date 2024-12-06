using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using StoreProjectModels.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.DbContexts;

namespace StoreProjectModels.CRUD
{
	public class Crud: ICrud
	{
        private readonly StoreDbContext _dbContext;
		public Crud(StoreDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		CrudResponse ICrud.CreateEntity<T>(dynamic entityId, T entity)
		{
			string name = typeof(T).Name;
			if (entity == null) return new(false, $"{name}Null");
			try
			{
				if (_dbContext.Find<T>(entityId) != null) return new(false, $"{name}Exists");
				//_dbContext.Entry<T>(entity).State = EntityState.Added;
				_dbContext.Add<T>(entity);
				_dbContext.SaveChanges();
				return new(true, "Create success");
			}
			catch (Exception ex)
			{
				return new(false, $"Create failed", $"Details: {ex.Message}");
			}
		}

		public CrudResponse CreateEntities<T>(string primaryKeyName, ObservableCollection<T> entities) where T : class
		{
			ObservableCollection<T> found = new();
			int countFound = 0, countNotFound = 0;

			if (entities.Count < 1)
			{
				return new(false, "Entities empty");
			}

			try
			{
				foreach (var entity in entities)
				{
					var entityId = _dbContext.Entry<T>(entity).Property(primaryKeyName).CurrentValue;
					T obj = _dbContext.Find<T>(entityId)!;
					if (obj != null)
					{
						found.Add(obj);
						countFound++;
					}
					else
					{
						_dbContext.Entry<T>(entity).State = EntityState.Detached;
						_dbContext.Entry<T>(entity).State = EntityState.Added;
						_dbContext.SaveChanges();
						countNotFound++;
					}
				}
				return new(true, "Creates success", new OperationInfo(countFound, countNotFound, JsonConvert.SerializeObject(found)).ToString());
			}
			catch (Exception ex)
			{
				return new(false, $"Creates failed", $"Details: {ex.Message}");
			}
		}

		CrudResponse ICrud.DeleteEntity<T>(dynamic entityId)
		{
			T entity = _dbContext.Find<T>(entityId);
			string name = typeof(T).Name;
			if (entity == null) return new(false, $"{name}NotFound");
			try
			{
				_dbContext.Entry<T>(entity).State = EntityState.Deleted;
				_dbContext.SaveChanges();
				return new(true, "Delete success");
			}
			catch (Exception ex)
			{
				return new(false, $"Delete failed", $"Details: {ex.Message}");
			}
		}

		public CrudResponse DeleteEntities<T>(dynamic entities) where T : class
		{
			try
			{
				foreach (var entity in entities)
				{
					_dbContext.Entry<T>(entity).State = EntityState.Deleted;
				}
				_dbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Deletes failed", $"Details: {ex.Message}");
			}
		}

		ObservableCollection<T> ICrud.GetEntities<T>()
		{
			throw new NotImplementedException();
		}

		T ICrud.GetEntity<T>(dynamic entityId)
		{
			return _dbContext.Find<T>(entityId);
		}

		CrudResponse ICrud.UpdateEntities<T>(string primaryKeyName, ObservableCollection<T> entities)
		{
			ObservableCollection<T> notFound = new();
			int countFound = 0, countNotFound = 0;

			if(entities.Count < 1)
			{
				return new(false, "Entities empty");
			}

			try
			{
				foreach (var entity in entities)
				{
					var entityId = _dbContext.Entry<T>(entity).Property(primaryKeyName).CurrentValue;
					T obj = _dbContext.Find<T>(entityId)!;
					if (obj != null)
					{
						_dbContext.Entry<T>(obj).State = EntityState.Detached;
						obj = entity;
						_dbContext.Entry<T>(obj).State = EntityState.Modified;
						_dbContext.SaveChanges();
						countFound++;
					}
					else
					{
						notFound.Add(entity);
						countNotFound++;
					}
				}
				return new(true, "Updates success", new OperationInfo(countFound, countNotFound, JsonConvert.SerializeObject(notFound)).ToString());
			}
			catch (Exception ex)
			{
				return new(false, $"Updates failed", $"Details: {ex.Message}");
			}
		}

		CrudResponse ICrud.UpdateEntity<T>(dynamic entityId, T entity)
		{
			T obj = _dbContext.Find<T>(entityId);
			string name = typeof(T).Name;
			if (obj == null) return new(false, $"{name}NotFound");
			try
			{
				_dbContext.Entry<T>(obj).State = EntityState.Detached;
				obj = entity;
				_dbContext.Entry<T>(obj).State = EntityState.Modified;
				_dbContext.SaveChanges();
				return new(true, "Update success");
			}
			catch (Exception ex)
			{
				return new(false, $"Update failed", $"Details: {ex.Message}");
			}
		}

		class OperationInfo
		{
            public OperationInfo(int found, int notFound, string data)
            {
				Found = found;
				NotFound = notFound;
				Data = data; 
			}
			public int Found { get; set; }
			public int NotFound { get; set; }
			public string Data { get; set; } = string.Empty;

			public override string ToString()
			{
				return JsonConvert.SerializeObject(this);
			}
		}
	}
}
