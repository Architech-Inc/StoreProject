using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using StoreProjectModels.CRUD;
using StoreProjectModels.Data;
using StoreProjectModels.DbContexts;

namespace StoreServices
{
	public class CategoryService: ICategoryService
	{
		private readonly StoreDbContext DBContext;
        private readonly ICrud Crud;
        public CategoryService(StoreDbContext StoreDbContext, ICrud crud)
		{
			this.DBContext = StoreDbContext;
			Crud = crud;
		}

		public CrudResponse AddCategory(Category category)
		{
			var cat = DBContext.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
			if (cat != null) return new(false, "CategoryExists");
			DBContext.Categories.Add(category);
			DBContext.SaveChanges();
			return new(true, "Success");
		}

		public CrudResponse DeleteCategory(int id)
		{
			Category category = DBContext.Categories.Find(id);
			if (category != null) return new(false, "CategoryNotFound");
			try
			{
				DBContext.Entry<Category>(category!).State = EntityState.Detached;
				DBContext.Categories.Remove(category);
				DBContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}

		public ObservableCollection<Category> GetAllCategories()
		{
			return new ObservableCollection<Category>(DBContext.Categories.ToList());
		}

		public Category GetCategory(int id)
		{
			throw new NotImplementedException();
		}

		public CrudResponse UpdateCategory(Category category)
		{
			if (category != null) return new(false, "CategoryNotFound");
			try
			{
				DBContext.Entry<Category>(category!).State = EntityState.Detached;
				DBContext.Categories.Update(category);
				DBContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}
	}
}
