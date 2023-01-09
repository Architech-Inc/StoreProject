using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("api/unit")]
	[ApiController]
	public class UnitController : ControllerBase
	{
		private readonly IUnitService _unitService;
		public UnitController(IUnitService unitService)
		{
			this._unitService = unitService;
		}

		[HttpGet]
		[Route("getUnits")]
		public IActionResult GetAllUnits()
		{
			return Ok(_unitService.GetAllUnits());
		}

		[HttpPost]
		[Route("getUnit")]
		public IActionResult GetUnit(int id)
		{
			return Ok(_unitService.GetUnit(id));
		}

		[HttpPost]
		[Route("addUnit")]
		public IActionResult GetUnit(Unit unit)
		{
			return Ok(_unitService.AddUnit(unit));
		}

		[HttpPut]
		[Route("updateUnit")]
		public IActionResult UpdateUnit(Unit unit)
		{
			return Ok(_unitService.UpdateUnit(unit));
		}

		[HttpDelete]
		[Route("deleteUnit")]
		public IActionResult DeleteUnit(int id)
		{
			return Ok(_unitService.DeleteUnit(id));
		}
	}
}
