using Microsoft.AspNetCore.Mvc;

namespace StoreAPI.Controllers
{
	public class ItemController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
