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
	public interface IEmployeeService
	{
		CrudResponse EmailExists(string email);
		CrudResponse AddEmployee(Employee employee);
		CrudResponse DeleteEmployee(string employeeId);
		CrudResponse UpdateEmployee(Employee employee);
		IEnumerable<Employee> GetAllEmployees();
		Employee GetEmployee(string employeeId);
	}
}
