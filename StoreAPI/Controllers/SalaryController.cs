using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;
using StoreServices.Interfaces;
using System.Collections.ObjectModel;

namespace StoreAPI.Controllers
{
	[Route("api/salary")]
	[ApiController]
	public class SalaryController : Controller
	{
		private readonly ISalaryService SalaryService;
		public SalaryController(ISalaryService salaryService)
		{
			SalaryService = salaryService;
		}


		[HttpGet]
		[Route("getAllSalaries")]
		public IActionResult GetAllSalaries()
		{
			return Ok(SalaryService.GetAllSalaries());
		}
		[HttpPost]
		[Route("getSalary")]
		public IActionResult GetSalary(int salaryId)
		{
			return Ok(SalaryService.GetSalary(salaryId));
		}
		[HttpPut]
		[Route("updateSalary")]
		public IActionResult UpdateSalary(Salary salary)
		{
			return Ok(SalaryService.UpdateSalary(salary));
		}
		[HttpPost]
		[Route("addSalary")]
		public IActionResult AddSalary(Salary salary)
		{
			return Ok(SalaryService.AddSalary(salary));
		}
		[HttpDelete]
		[Route("deleteSalary")]
		public IActionResult DeleteSalary(int salaryId)
		{
			return Ok(SalaryService.DeleteSalary(salaryId));
		}
		[HttpPost]
		[Route("addSalaries")]
		public IActionResult AddSalaries(ObservableCollection<Salary> salaries)
		{
			return Ok(SalaryService.AddSalaries(salaries));
		}
		[HttpDelete]
		[Route("deleteSalaries")]
		public IActionResult DeleteSalaries(ObservableCollection<Salary> salaries)
		{
			return Ok(SalaryService.DeleteSalaries(salaries));
		}
	}
}
