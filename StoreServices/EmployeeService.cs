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
	public class EmployeeService : IEmployeeService
	{
		private readonly store_dbContext DbContext;
		public EmployeeService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddEmployee(Employee employee)
		{
			if (employee == null) return new(false, "EmployeeNull");
			try
			{
				employee.EmployeeId = Authentication.GenerateGuid();
				if (DbContext.Users.Where(u => u.UserId == employee.EmployeeId).ToList().FirstOrDefault() == null) return new(false, "UserExists");
				DbContext.Employees.Add(employee);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}

		public ResponseModel DeleteEmployee(string employeeId)
		{
			try
			{
				if (DbContext.Employees.Where(u => u.EmployeeId == employeeId).FirstOrDefault() == null) return new(false, "UserNotFound");
				DbContext.Employees.Remove(DbContext.Employees.Where(u => u.EmployeeId == employeeId).FirstOrDefault());
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}

		public ObservableCollection<Employee> GetAllEmployees()
		{
			return new(DbContext.Employees);
		}

		public Employee GetEmployee(string employeeId)
		{
			return DbContext.Employees.Where(e => e.EmployeeId == employeeId).FirstOrDefault();
		}

		public ResponseModel UpdateEmployee(Employee employee)
		{
			Employee _user = DbContext.Employees.Where(u => u.EmployeeId == employee.EmployeeId).ToList().FirstOrDefault();
			if (employee == null) return new(false, "EmployeeNotFound");
			try
			{
				DbContext.Employees.Update(employee);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
	}
}
