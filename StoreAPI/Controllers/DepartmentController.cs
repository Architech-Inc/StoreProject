using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("api/dept")]
	[ApiController]
	public class DepartmentController : ControllerBase
	{
		private readonly IDepartmentService departmentService;
		public DepartmentController(IDepartmentService departmentService)
		{
			this.departmentService = departmentService;	
		}

		[HttpGet]
		[Route("getDepts")]
		public IActionResult GetAllDepartments()
		{
			return Ok(departmentService.GetAllDepartments());
		}

		[HttpPost]
		[Route("getDept")]
		public IActionResult GetDepartment(int id)
		{
			return Ok(departmentService.GetDepartment(id));
		}

		[HttpPost]
		[Route("addDept")]
		public IActionResult AddDepartment(Department dept)
		{
			return Ok(departmentService.AddDepartment(dept));
		}

		[HttpPut]
		[Route("updateDept")]
		public IActionResult UpdateDepartment(Department department)
		{
			return Ok(departmentService.UpdateDepartment(department));
		}

		[HttpDelete]
		[Route("deleteDept")]
		public IActionResult DeleteDepartment(int id)
		{
			return Ok(departmentService.DeleteDepartment(id));
		}
	}
}
