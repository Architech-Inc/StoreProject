using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("employee")]
	[ApiController]
	public class EmployeeController : Controller
	{
		private readonly IEmployeeService employeeService;
		public EmployeeController(IEmployeeService employeeService)
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
		public IActionResult AddEmployee(Employee employee)
		{
			return Ok(employeeService.AddEmployee(employee));
		}
		[HttpPost]
		[Route("updateEmployee")]
		public IActionResult UpdateEmployee(Employee employee)
		{
			return Ok(employeeService.UpdateEmployee(employee));
		}
		[HttpDelete]
		[Route("deleteEmployee")]
		public IActionResult DeleteEmployee(string employeeId)
		{
			return Ok(employeeService.DeleteEmployee(employeeId));
		}
	}
}
