using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService) => _invoiceService = invoiceService;

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] InvoicePagedRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<InvoiceDto>>.Ok(result));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetSummary([FromQuery] InvoicePagedRequest request, CancellationToken ct)
    {
        var result = await _invoiceService.GetSummaryMetricsAsync(request, ct);
        return Ok(ApiResponse<InvoiceSummaryMetricsDto>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var invoice = await _invoiceService.GetByIdAsync(id, ct);
        if (invoice is null) return NotFound(ApiResponse<object>.Fail("Invoice not found."));
        return Ok(ApiResponse<InvoiceDto>.Ok(invoice));
    }

    [HttpGet("public/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicReceipt(Guid id, CancellationToken ct)
    {
        var receipt = await _invoiceService.GetPublicReceiptAsync(id, ct);
        if (receipt is null) return NotFound(ApiResponse<object>.Fail("Receipt not found or invalid reference."));
        return Ok(ApiResponse<PublicReceiptDto>.Ok(receipt));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        var invoice = await _invoiceService.CreateInvoiceAsync(request, userId == Guid.Empty ? null : userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = invoice.InvoiceId }, ApiResponse<InvoiceDto>.Ok(invoice, "Invoice created."));
    }

    [HttpDelete("{id:guid}/void")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Void(Guid id, [FromQuery] string? reason, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        var success = await _invoiceService.VoidInvoiceAsync(id, userId == Guid.Empty ? null : userId, reason, ct);
        if (!success) return NotFound(ApiResponse<object>.Fail("Invoice not found or already voided."));
        return Ok(ApiResponse<object>.Ok(null!, "Invoice voided."));
    }

    [HttpPost("{id:guid}/void")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> VoidCompat(Guid id, [FromQuery] string? reason, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        var success = await _invoiceService.VoidInvoiceAsync(id, userId == Guid.Empty ? null : userId, reason, ct);
        if (!success) return NotFound(ApiResponse<object>.Fail("Invoice not found or already voided."));
        return Ok(ApiResponse<object>.Ok(null!, "Invoice voided."));
    }

    [HttpPost("{id:guid}/refund")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundInvoiceRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var userId);

        try
        {
            var invoice = await _invoiceService.RefundInvoiceAsync(id, request, userId == Guid.Empty ? null : userId, ct);
            if (invoice is null) return NotFound(ApiResponse<object>.Fail("Invoice not found or not paid."));
            return Ok(ApiResponse<InvoiceDto>.Ok(invoice, "Refund processed successfully."));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/tender")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> AddTender(Guid id, [FromBody] AddTenderRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var tender = await _invoiceService.AddTenderAsync(id, request, ct);
            return Ok(ApiResponse<InvoiceTenderDto>.Ok(tender, "Payment recorded."));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Fail("Invoice not found."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
