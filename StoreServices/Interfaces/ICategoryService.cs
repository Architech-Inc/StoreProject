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
	public interface ICategoryService
	{
		Category GetCategory(int id);
		ObservableCollection<Category> GetAllCategories();
		ResponseModel AddCategory(Category category);
		ResponseModel DeleteCategory(int id);
		ResponseModel UpdateCategory(Category category);
	}
}
