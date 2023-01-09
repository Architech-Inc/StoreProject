using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;
using System.Collections.ObjectModel;

namespace StoreAPI.Controllers
{
	[Route("api/item")]
	[ApiController]
	public class ItemController : ControllerBase
	{
		private readonly IItemService ItemService;
		public ItemController(IItemService itemService)
		{
			ItemService = itemService;
		}

		[HttpGet]
		[Route("getAllItems")]
		public IActionResult GetAllItems()
		{
			return Ok(ItemService.GetAllItems());
		}
		[HttpGet]
		[Route("getItem")]
		public IActionResult GetItem(long itemId)
		{
			return Ok(ItemService.GetItem(itemId));
		}
		[HttpPost]
		[Route("addItem")]
		public IActionResult AddItem(Item item)
		{
			return Ok(ItemService.AddItem(item));
		}
		[HttpPost]
		[Route("addItems")]
		public IActionResult AddItems(ObservableCollection<Item> items)
		{
			return Ok(ItemService.AddItems(items));
		}
		[HttpPut]
		[Route("updateItem")]
		public IActionResult UpdateItem(Item item)
		{
			return Ok(ItemService.UpdateItem(item));
		}
		[HttpDelete]
		[Route("deleteItem")]
		public IActionResult DeleteItem(long itemId)
		{
			return Ok(ItemService.DeleteItem(itemId));
		}
		[HttpDelete]
		[Route("deleteItems")]
		public IActionResult DeleteItems(ObservableCollection<Item> items)
		{
			return Ok(ItemService.DeleteItems(items));
		}
	}
}
