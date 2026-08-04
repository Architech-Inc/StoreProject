using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILoyaltyService _loyaltyService;
    private readonly IUnitOfWork _uow;

    public CustomersController(ICustomerService customerService, ILoyaltyService loyaltyService, IUnitOfWork uow)
    {
        _customerService = customerService;
        _loyaltyService = loyaltyService;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _customerService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer is null) return NotFound(ApiResponse<object>.Fail("Customer not found."));
        return Ok(ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpGet("{id:guid}/invoices")]
    public async Task<IActionResult> GetInvoices(Guid id, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var invoices = await _uow.Repository<Store.Models.Entities.Invoice>().Query()
            .AsNoTracking()
            .Where(i => i.CustomerId == id)
            .OrderByDescending(i => i.DateCreated)
            .Take(Math.Min(take, 100))
            .Select(i => new
            {
                i.InvoiceId,
                InvoiceNumber = i.InvoiceId.ToString().Substring(0, 8).ToUpper(),
                i.DateCreated,
                i.TotalAmount,
                i.IsPaid,
                Status = i.IsPaid ? "Paid" : "Pending",
                ItemCount = i.Sales.Count
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(invoices));
    }

    [HttpGet("{id:guid}/loyalty-transactions")]
    public async Task<IActionResult> GetLoyaltyTransactions(Guid id, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var txns = await _loyaltyService.GetTransactionsAsync(id, Math.Min(take, 100), ct);
        var dtos = txns.Select(t => new
        {
            t.LoyaltyTransactionId,
            t.Points,
            TransactionType = t.TransactionType.ToString(),
            t.InvoiceId,
            t.Note,
            t.DateCreated
        });
        return Ok(ApiResponse<object>.Ok(dtos));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, ApiResponse<CustomerDto>.Ok(customer, "Customer created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customerService.UpdateAsync(id, request, ct);
        if (customer is null) return NotFound(ApiResponse<object>.Fail("Customer not found."));
        return Ok(ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _customerService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Customer not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Customer deleted."));
    }
}
