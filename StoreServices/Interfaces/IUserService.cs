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
	public interface IUserService
	{
		CrudResponse AddUser(User user);
		CrudResponse UpdateUser(User user);
		CrudResponse DeleteUser(string userId);
		ObservableCollection<User> GetAllUsers();
		User GetUser(string userId);
	}
}
