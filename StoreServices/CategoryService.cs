using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System.Collections.ObjectModel;

namespace StoreServices
{
	public class CategoryService: ICategoryService
	{
		private readonly store_dbContext DBContext;
		public CategoryService(store_dbContext store_DbContext)
		{
			this.DBContext = store_DbContext;
		}

		public ResponseModel AddCategory(Category category)
		{
			var cat = DBContext.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
			if (cat != null) return new(false, "CategoryExists");
			DBContext.Categories.Add(category);
			DBContext.SaveChanges();
			return new(true, "Success");
		}

		public ResponseModel DeleteCategory(int id)
		{
			throw new NotImplementedException();
		}

		public ObservableCollection<Category> GetAllCategories()
		{
			return new ObservableCollection<Category>(DBContext.Categories.ToList());
		}

		public Category GetCategory(int id)
		{
			throw new NotImplementedException();
		}

		public ResponseModel UpdateCategory(int id, Category category)
		{
			throw new NotImplementedException();
		}
	}
}
