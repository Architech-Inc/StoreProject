using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	//[Authorize]
	[Route("api/employee")]
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
		[HttpGet]
		[Route("getEmployee")]
		public IActionResult GetEmployee(string employeeId)
		{
			return Ok(employeeService.GetEmployee(employeeId));
		}
		[AllowAnonymous]
		[HttpPost]
		[Route("emailCheck")]
		public IActionResult AddEmployee(string email)
		{
			return Ok(employeeService.EmailExists(email));
		}
		[HttpPost]
		[Route("addEmployee")]
		public IActionResult AddEmployee(Employee employee)
		{
			return Ok(employeeService.AddEmployee(employee));
		}
		[HttpPut]
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
