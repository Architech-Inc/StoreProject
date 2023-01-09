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
	public interface ICrud<T>
	{
		ResponseModel Add<T>(dynamic id, dynamic value, store_dbContext DbContext);
		ResponseModel Update<T>(dynamic id, dynamic value, store_dbContext DbContext);
		ResponseModel Delete<T>(dynamic id, store_dbContext DbContext);
		T GetSingle<T>(dynamic id, store_dbContext DbContext);
		ObservableCollection<T> Collection(store_dbContext DbContext);
	}
}
