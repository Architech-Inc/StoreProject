using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    private readonly ILoyaltyService _loyaltyService;

    public LoyaltyController(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    /// <summary>Get loyalty account for a customer.</summary>
    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetAccount(Guid customerId, CancellationToken ct)
    {
        var account = await _loyaltyService.GetAccountAsync(customerId, ct);
        if (account is null)
            return NotFound(ApiErrorResponse.From("not_found", "No loyalty account found for this customer.", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<LoyaltyAccountDto>.Ok(new LoyaltyAccountDto
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            CustomerId = account.CustomerId,
            Points = account.Points,
            Tier = account.Tier.ToString()
        }));
    }

    /// <summary>Get transaction history for a customer's loyalty account.</summary>
    [HttpGet("customers/{customerId:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid customerId, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var txns = await _loyaltyService.GetTransactionsAsync(customerId, Math.Min(take, 200), ct);
        var dtos = txns.Select(t => new LoyaltyTransactionDto
        {
            LoyaltyTransactionId = t.LoyaltyTransactionId,
            Points = t.Points,
            TransactionType = t.TransactionType.ToString(),
            InvoiceId = t.InvoiceId,
            Note = t.Note,
            DateCreated = t.DateCreated
        });
        return Ok(ApiResponse<IEnumerable<LoyaltyTransactionDto>>.Ok(dtos));
    }

    /// <summary>Earn points for a customer (e.g. after a sale).</summary>
    [HttpPost("earn")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Earn([FromBody] EarnPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var txn = await _loyaltyService.EarnPointsAsync(request.CustomerId, request.Points, request.InvoiceId, request.Note, ct);
            return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), $"{request.Points} points earned."));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiErrorResponse.From("invalid_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    /// <summary>Redeem points for a customer.</summary>
    [HttpPost("redeem")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<IActionResult> Redeem([FromBody] RedeemPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var txn = await _loyaltyService.RedeemPointsAsync(request.CustomerId, request.Points, request.Note, ct);
            return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), $"{request.Points} points redeemed."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From("insufficient_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    /// <summary>Manually adjust loyalty points (admin).</summary>
    [HttpPost("adjust")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Adjust([FromBody] AdjustPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var txn = await _loyaltyService.AdjustPointsAsync(request.CustomerId, request.Points, request.Note, ct);
        return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), "Points adjusted."));
    }

    private static LoyaltyTransactionDto ToDto(Store.Models.Entities.LoyaltyTransaction t) => new()
    {
        LoyaltyTransactionId = t.LoyaltyTransactionId,
        Points = t.Points,
        TransactionType = t.TransactionType.ToString(),
        InvoiceId = t.InvoiceId,
        Note = t.Note,
        DateCreated = t.DateCreated
    };
}
