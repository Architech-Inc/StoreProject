using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly EmployeeService employeeService;
		public EmployeeController(EmployeeService employeeService)
		{
			this.employeeService = employeeService;
		}

		[HttpGet]
		[Route("getAllEmployees")]
		public IActionResult GetAllEmployees()
		{
			return Ok(employeeService.GetAllEmployees());
		}
		[HttpPost]
		[Route("getEmployee")]
		public IActionResult GetEmployee(string employeeId)
		{
			return Ok(employeeService.GetEmployee(employeeId));
		}
		[HttpPost]
		[Route("addEmployee")]
		public IActionResult AddUser(Employee employee)
		{
			return Ok(employeeService.AddEmployee(employee));
		}
		[HttpPost]
		[Route("updateEmployee")]
		public IActionResult UpdateUser(Employee employee)
		{
			return Ok(employeeService.UpdateEmployee(employee));
		}
		[HttpDelete]
		[Route("deleteEmployee")]
		public IActionResult DeleteUser(string employeeId)
		{
			return Ok(employeeService.DeleteEmployee(employeeId));
		}
	}
}
