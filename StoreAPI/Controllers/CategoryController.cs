using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("api/category")]
	[ApiController]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService categoryService;
		public CategoryController(ICategoryService _categoryService)
		{
			this.categoryService = _categoryService;
		}

		[HttpGet]
		[Route("getAllCategories")]
		public IActionResult GetAllCategories()
		{
			return Ok(categoryService.GetAllCategories());
		}

		[HttpPost]
		[Route("addCategory")]
		public IActionResult AddCategory(Category category)
		{
			return Ok(categoryService.AddCategory(category));
		}

		[HttpDelete]
		[Route("deleteCategory")]
		public IActionResult DeleteCategory(int id)
		{
			return Ok(categoryService.DeleteCategory(id));
		}

		[HttpPut]
		[Route("updateCategory")]
		public IActionResult UpdateCategory(Category category)
		{
			return Ok(categoryService.UpdateCategory(category));
		}
	}
}
