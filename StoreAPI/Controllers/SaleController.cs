using Microsoft.AspNetCore.Mvc;

namespace StoreAPI.Controllers
{
	public class SaleController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
