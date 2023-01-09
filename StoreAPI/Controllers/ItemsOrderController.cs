using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("api/itemsOrder")]
	[ApiController]
	public class ItemsOrderController : ControllerBase
	{
		private readonly IOrderService OrderServie;
		public ItemsOrderController(IOrderService orderServie)
		{
			OrderServie = orderServie;
		}

		[HttpGet]
		[Route("getItemsOrders")]
		public IActionResult GetAllItemsOrders()
		{
			return Ok(OrderServie.GetAllItemsOrders());
		}

		[HttpPost]
		[Route("getItemsOrder")]
		public IActionResult GetItemsOrder(int id)
		{
			return Ok(OrderServie.GetItemsOrder(id));
		}

		[HttpPost]
		[Route("addItemsOrder")]
		public IActionResult GetItemsOrder(ItemsOrder itemsOrder)
		{
			return Ok(OrderServie.AddItemsOrder(itemsOrder));
		}

		[HttpPut]
		[Route("updateItemsOrder")]
		public IActionResult UpdateItemsOrder(ItemsOrder itemsOrder)
		{
			return Ok(OrderServie.UpdateItemsOrder(itemsOrder));
		}

		[HttpDelete]
		[Route("deleteItemsOrder")]
		public IActionResult DeleteItemsOrder(int id)
		{
			return Ok(OrderServie.DeleteItemsOrder(id));
		}
	}
}
