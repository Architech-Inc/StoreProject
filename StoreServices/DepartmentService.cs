using Microsoft.EntityFrameworkCore;
using StoreProjectModels.CRUD;
using StoreProjectModels.Data;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.DbContexts;
using StoreProjectModels.Models;
using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices
{
	public class DepartmentService: IDepartmentService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public DepartmentService(StoreDbContext dbContext, ICrud crud)
		{
			DbContext = dbContext;
			Crud = crud;
		}

		public CrudResponse AddDepartment(Department department)
		{
			if (department == null) return new(false, "DepartmentNull");
			try
			{
				DbContext.Departments.Add(department);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}

		public CrudResponse DeleteDepartment(int deptId)
		{
			Department department = DbContext.Departments.Find(deptId);
			if (department == null) return new(false, "DepartmentNotFound");
			try
			{
				DbContext.Entry(department).State = EntityState.Detached;
				DbContext.Remove(department);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}

		public ObservableCollection<Department> GetAllDepartments()
		{
			return new ObservableCollection<Department>(DbContext.Departments);
		}

		public Department GetDepartment(int deptId)
		{
			return DbContext.Departments.Find(deptId);
		}

		public CrudResponse UpdateDepartment(Department department)
		{
			if (department == null) return new(false, "DepartmentNull");
			try
			{
				DbContext.Entry(department).State = EntityState.Detached;
				DbContext.Update(department);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}
	}
}
