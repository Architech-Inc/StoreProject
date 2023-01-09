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
				if (DbContext.Users.Where(e => e.EmployeeId == employee.EmployeeId).ToList().FirstOrDefault() != null) return new(false, "EmployeeExists");
				DbContext.Employees.Add(employee);
				DbContext.SaveChanges();
				try
				{
					User user = new()
					{
						UserId = Authentication.GenerateGuid(),
						EmployeeId = employee.EmployeeId,
						Username = $"{employee.FirstName}{Convert.ToString(employee.NidNumber).Substring(Convert.ToString(employee.NidNumber).Length - 4)}",
						AccountType = "user",
						Password = Authentication.EncryptPassword(employee.LastName.ToLower()),
						Employee = null,
						Unit = null,
						Invoices = new HashSet<Invoice>(),
						ItemsOrders = new HashSet<ItemsOrder>(),
						Sales = new HashSet<Sale>()
					};
					DbContext.Users.Add(user);
					DbContext.SaveChanges();
					//return new(true, "Success");
				}
				catch (Exception ex)
				{
					return new(false, $"Failed on adding user: {ex.Message}");
				}
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex.Message}");
			}
		}

		public ResponseModel DeleteEmployee(string employeeId)
		{
			try
			{
				Employee employee = DbContext.Employees.Where(e => e.EmployeeId == employeeId).FirstOrDefault();
				if (employee == null) return new(false, "EmployeeNotFound");
				DbContext.Entry<Employee>(employee).State = EntityState.Deleted;
				DbContext.Employees.Remove(employee);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex.Message}");
			}
		}

		public ResponseModel EmailExists(string email)
		{
			string em = DbContext.Employees.Where(e => e.Email == email).Select(e => e.Email).SingleOrDefault();
			if (em == null || em == "") return new(true, "DoesNotExists");
			return new(false, "Exists");
		}

		public IEnumerable<Employee> GetAllEmployees()
		{
			return new List<Employee>(DbContext.Employees);
		}

		public Employee GetEmployee(string employeeId)
		{
			Employee employee = DbContext.Employees.Where(e => e.EmployeeId == employeeId).FirstOrDefault();
			if (employee == null) return null;
			return employee;
		}

		public ResponseModel UpdateEmployee(Employee employee)
		{
			Employee _employee = DbContext.Employees.Where(e => e.EmployeeId == employee.EmployeeId).ToList().FirstOrDefault();
			if (_employee == null) return new(false, "EmployeeNotFound");
			try
			{
				DbContext.Entry<Employee>(_employee).State = EntityState.Detached;
				_employee = employee;
				DbContext.Employees.Update(_employee);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex.Message}");
			}
		}
	}
}
