using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.Models;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Authorize]
	[Route("api/auth")]
	[ApiController]
	public class AuthenticationController : ControllerBase
	{
		private readonly IAuthenticationService authenticationService;
		public AuthenticationController(IAuthenticationService authenticationService)
		{
			this.authenticationService = authenticationService;
		}
		[AllowAnonymous]
		[HttpPost]
		[Route("authenticate")]
		public IActionResult Authenticate(UserCredential userCredential)
		{
			var token = authenticationService.Authenticate(userCredential);
			if (token == null) return Unauthorized();
			return Ok(token);
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("generateCode")]
		public IActionResult GenerateCode(string username)
		{
			return Ok(authenticationService.GenerateCode(username));
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("reset")]
		public IActionResult Reset(UserCredential userCredential)
		{
			return Ok(authenticationService.Reset(userCredential));
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("checkTokenValidity")]
		public IActionResult IsTokenInvalid(string token)
		{
			return Ok(authenticationService.IsTokenValid(token));
		}
	}
}
