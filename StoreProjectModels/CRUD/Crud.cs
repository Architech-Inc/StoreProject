using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.CRUD
{
	//public class Person<T> : ICrud<T>
	//{
	//	public ResponseModel Add<T1>(dynamic id, dynamic value, store_dbContext DbContext)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public ObservableCollection<T> Collection(store_dbContext DbContext)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public ResponseModel Delete<T1>(dynamic id, store_dbContext DbContext)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public T1 GetSingle<T1>(dynamic id, store_dbContext DbContext)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public ResponseModel Update<T1>(dynamic id, dynamic value, store_dbContext DbContext)
	//	{
	//		throw new NotImplementedException();
	//	}
	//}
	public static class Crud//<T>: ICrud<T>, where T: class
	{
		public static T Read<T>(dynamic id, store_dbContext DbContext) where T: class
		{
			return DbContext.Find<T>(id);
		}
		public static ResponseModel UpdateRange<T>(dynamic value)
		{
			return new(true, "");
		}
		public static ResponseModel Create<T>(dynamic id, T value, store_dbContext DbContext) where T: class
		{
			string name = typeof(T).Name;
			if (value == null) return new(false, $"{name}Null");
			try
			{
				if (DbContext.Find<T>(id) != null) return new(false, $"{name}Exists");
				DbContext.Add<T>(value);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public static ResponseModel CreateRange<T>(dynamic list, store_dbContext DbContext) where T: class
		{
			try
			{
				DbContext.AddRange(list);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public static ResponseModel Delete<T>(dynamic id, store_dbContext DbContext) where T: class
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
		public static ResponseModel DeleteRange<T>(dynamic list, store_dbContext DbContext) where T: class
		{
			try
			{
				foreach (var item in list)
				{
					DbContext.Entry<T>(item).State = EntityState.Deleted;
				}
				/*for (int i = 0; i < list.Count; i++)
				{
					DbContext.Entry<T>(list[i]).State = EntityState.Detached;
				}*/
				//DbContext.RemoveRange(list);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
		public static ResponseModel Update<T>(dynamic id, T value, store_dbContext DbContext) where T: class
		{
			T obj = DbContext.Find<T>(id);
			string name = typeof(T).Name;
			if (obj == null) return new(false, $"{name}NotFound");
			try
			{
				DbContext.Entry<T>(obj).State = EntityState.Detached;
				obj = value;
				DbContext.Entry<T>(obj).State = EntityState.Modified;
				//DbContext.Update<T>(obj);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
	}
}
