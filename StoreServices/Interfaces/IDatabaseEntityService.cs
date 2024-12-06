using StoreProjectModels.Data;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices.Interfaces
{
	public interface IDatabaseEntityService
	{
		CrudResponse AddEntity<T>(T entity) where T : class;
		CrudResponse DeleteEntity<T>(int id) where T : class;
		CrudResponse DeleteEntity<T>(long id) where T : class;
		CrudResponse DeleteEntity<T>(string id) where T : class;
		CrudResponse UpdateEntity<T>(T entity) where T : class;
	}
}
