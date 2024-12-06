using StoreProjectModels.Data;
using System.Collections.ObjectModel;

namespace StoreProjectModels.CRUD
{
	public interface ICrud
	{
		CrudResponse CreateEntities<T>(string primaryKeyName, ObservableCollection<T> entities) where T : class;
		CrudResponse CreateEntity<T>(dynamic entityId, T entity) where T : class;
		CrudResponse DeleteEntities<T>(dynamic entities) where T : class;
		CrudResponse DeleteEntity<T>(dynamic entityId) where T: class;
		ObservableCollection<T> GetEntities<T>() where T : class;
		T GetEntity<T>(dynamic entityId) where T: class;
		CrudResponse UpdateEntities<T>(string primaryKeyName, ObservableCollection<T> entities) where T : class;
		CrudResponse UpdateEntity<T>(dynamic entityId, T entity) where T : class;
	}
}
