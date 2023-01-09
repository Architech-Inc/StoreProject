using Microsoft.AspNetCore.Mvc;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;
using System.Collections.ObjectModel;

namespace StoreAPI.Controllers
{
	[Route("api/sale")]
	[ApiController]
	public class SaleController : ControllerBase
	{
		private readonly ISaleService SaleService;
		public SaleController(ISaleService saleService)
		{
			SaleService = saleService;
		}

		[HttpGet]
		[Route("getAllSales")]
		public IActionResult GetAllSales()
		{
			return Ok(SaleService.GetAllSales());
		}
		[HttpGet]
		[Route("getSale")]
		public IActionResult GetSale(long saleId)
		{
			return Ok(SaleService.GetSale(saleId));
		}
		[HttpPost]
		[Route("addSale")]
		public IActionResult AddSale(Sale sale)
		{
			return Ok(SaleService.AddSale(sale));
		}
		[HttpPost]
		[Route("addSales")]
		public IActionResult AddSales(ObservableCollection<Sale> sales)
		{
			return Ok(SaleService.AddSales(sales));
		}
		[HttpPut]
		[Route("updateSale")]
		public IActionResult UpdateSale(Sale sale)
		{
			return Ok(SaleService.UpdateSale(sale));
		}
		[HttpDelete]
		[Route("deleteSale")]
		public IActionResult DeleteSale(long saleId)
		{
			return Ok(SaleService.DeleteSale(saleId));
		}
		[HttpDelete]
		[Route("deleteSales")]
		public IActionResult DeleteSales(ObservableCollection<Sale> sales)
		{
			return Ok(SaleService.DeleteSales(sales));
		}
	}
}
