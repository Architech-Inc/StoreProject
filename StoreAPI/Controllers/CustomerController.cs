using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;
using System.Collections.ObjectModel;

namespace StoreAPI.Controllers
{
	[Route("api/customer")]
	[ApiController]
	public class CustomerController : Controller
	{
		private readonly ICustomerService CustomerService;
		public CustomerController(ICustomerService customerService)
		{
			CustomerService = customerService;
		}

		[HttpGet]
		[Route("getAllCustomers")]
		public IActionResult GetAllCustomers()
		{
			return Ok(CustomerService.GetAllCustomers());
		}
		[HttpPost]
		[Route("getCustomer")]
		public IActionResult GetCustomer(string customerId)
		{
			return Ok(CustomerService.GetCustomer(customerId));
		}
		[HttpPut]
		[Route("updateCustomer")]
		public IActionResult UpdateCustomer(Customer customer)
		{
			return Ok(CustomerService.UpdateCustomer(customer));
		}
		[HttpPost]
		[Route("addCustomer")]
		public IActionResult AddCustomer(Customer customer)
		{
			return Ok(CustomerService.AddCustomer(customer));
		}
		[HttpDelete]
		[Route("deleteCustomer")]
		public IActionResult DeleteCustomer(string customerId)
		{
			return Ok(CustomerService.DeleteCustomer(customerId));
		}
		[HttpDelete]
		[Route("deleteCustomers")]
		public IActionResult DeleteCustomers(ObservableCollection<Customer> customers)
		{
			return Ok(CustomerService.DeleteCustomers(customers));
		}
	}
}
