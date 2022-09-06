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
		ResponseModel AddEmployee(Employee employee);
		ResponseModel DeleteEmployee(string employeeId);
		ResponseModel UpdateEmployee(Employee employee);
		ObservableCollection<Employee> GetAllEmployees();
		Employee GetEmployee(string employeeId);
	}
}
