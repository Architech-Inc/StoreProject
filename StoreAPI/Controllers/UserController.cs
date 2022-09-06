using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices;

namespace StoreAPI.Controllers
{
	[Route("user")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		public UserController(UserService userService)
		{
			_userService = userService;
		}

		[HttpGet]
		[Route("getAllUsers")]
		public IActionResult GetAllUsers()
		{
			return Ok(_userService.GetAllUsers());
		}
		[HttpPost]
		[Route("getUser")]
		public IActionResult GetUser(string userId)
		{
			return Ok(_userService.GetUser(userId));
		}
		[HttpPost]
		[Route("addUser")]
		public IActionResult AddUser(User user)
		{
			return Ok(_userService.AddUser(user));
		}
		[HttpPost]
		[Route("updateUser")]
		public IActionResult UpdateUser(User user)
		{
			return Ok(_userService.UpdateUser(user));
		}
		[HttpDelete]
		[Route("deleteUser")]
		public IActionResult DeleteUser(string userId)
		{
			return Ok(_userService.DeleteUser(userId));
		}
	}
}
