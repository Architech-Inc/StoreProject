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
		ResponseModel AddUser(User user);
		ResponseModel UpdateUser(User user);
		ResponseModel DeleteUser(string userId);
		ObservableCollection<User> GetAllUsers();
		User GetUser(string userId);
	}
}
