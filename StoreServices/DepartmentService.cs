using Microsoft.EntityFrameworkCore;
using StoreProjectModels.DatabaseModels;
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
		private readonly store_dbContext DbContext;
		public DepartmentService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddDepartment(Department department)
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

		public ResponseModel DeleteDepartment(int deptId)
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

		public ResponseModel UpdateDepartment(Department department)
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
