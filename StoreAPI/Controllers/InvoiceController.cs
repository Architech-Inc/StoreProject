using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;

namespace StoreAPI.Controllers
{
	[Route("api/invoices")]
	[ApiController]
	public class InvoiceController : ControllerBase
	{
		private readonly IInvoiceService InvoiceService;
		public InvoiceController(IInvoiceService invoiceService)
		{
			InvoiceService = invoiceService;
		}
		[HttpGet]
		[Route("getAllInvoices")]
		public IActionResult GetAllInvoices()
		{
			return Ok(InvoiceService.GetAllInvoices());
		}
		[HttpPost]
		[Route("getInvoice")]
		public IActionResult GetInvoice(long invoiceId)
		{
			return Ok(InvoiceService.GetInvoice(invoiceId));
		}
		[HttpPut]
		[Route("updateInvoice")]
		public IActionResult UpdateInvoice(Invoice invoice)
		{
			return Ok(InvoiceService.UpdateInvoice(invoice));
		}
		[HttpPost]
		[Route("addInvoice")]
		public IActionResult AddInvoice(Invoice invoice)
		{
			return Ok(InvoiceService.AddInvoice(invoice));
		}
		[HttpDelete]
		[Route("deleteInvoice")]
		public IActionResult DeleteInvoice(long invoiceId)
		{
			return Ok(InvoiceService.DeleteInvoice(invoiceId));
		}
	}
}
