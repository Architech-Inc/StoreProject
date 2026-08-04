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

    /// <summary>Get aggregate store-wide loyalty metrics and KPI indicators.</summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _loyaltyService.GetMetricsAsync(ct);
        return Ok(ApiResponse<LoyaltyMetricsDto>.Ok(metrics));
    }

    /// <summary>Get paginated and searchable list of enrolled loyalty members.</summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetAllMembers(
        [FromQuery] string? search = null,
        [FromQuery] string? tier = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _loyaltyService.GetAllMembersAsync(search, tier, sortBy, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<LoyaltyMemberDto>>.Ok(result));
    }

    /// <summary>Get full 360 profile for a loyalty member including ledger and active campaigns.</summary>
    [HttpGet("customers/{customerId:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid customerId, CancellationToken ct)
    {
        var profile = await _loyaltyService.GetMemberProfileAsync(customerId, ct);
        if (profile is null)
            return NotFound(ApiErrorResponse.From("not_found", "No loyalty account found for this customer.", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<LoyaltyMemberProfileDto>.Ok(profile));
    }

    /// <summary>Get loyalty account for a specific customer.</summary>
    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetAccount(Guid customerId, CancellationToken ct)
    {
        var account = await _loyaltyService.GetAccountAsync(customerId, ct);
        if (account is null)
            return NotFound(ApiErrorResponse.From("not_found", "No loyalty account found for this customer.", traceId: HttpContext.TraceIdentifier));

        var custName = account.Customer != null ? $"{account.Customer.FirstName} {account.Customer.LastName}".Trim() : null;
        return Ok(ApiResponse<LoyaltyAccountDto>.Ok(new LoyaltyAccountDto
        {
            LoyaltyAccountId = account.LoyaltyAccountId,
            CustomerId = account.CustomerId,
            Points = account.Points,
            Tier = account.Tier.ToString(),
            CustomerName = custName
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

    /// <summary>Get global store-wide loyalty transaction audit stream.</summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetGlobalTransactions(
        [FromQuery] string? search = null,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var txns = await _loyaltyService.GetGlobalTransactionsAsync(search, transactionType, fromDate, toDate, take, ct);
        return Ok(ApiResponse<IEnumerable<GlobalLoyaltyTransactionDto>>.Ok(txns));
    }

    /// <summary>Earn points for a customer (e.g. after a purchase or promotion).</summary>
    [HttpPost("earn")]
    public async Task<IActionResult> Earn([FromBody] EarnPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var txn = await _loyaltyService.EarnPointsAsync(request.CustomerId, request.Points, request.InvoiceId, request.Note, ct);
            return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), $"{request.Points} points earned successfully."));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiErrorResponse.From("invalid_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    /// <summary>Redeem points for a customer reward / discount voucher.</summary>
    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var txn = await _loyaltyService.RedeemPointsAsync(request.CustomerId, request.Points, request.Note, ct);
            return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), $"{request.Points} points redeemed successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From("insufficient_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
    }

    /// <summary>Manually adjust loyalty points (admin or manager correction).</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustPointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var txn = await _loyaltyService.AdjustPointsAsync(request.CustomerId, request.Points, request.Note, ct);
        return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(txn), "Points adjusted successfully."));
    }

    /// <summary>Unified management endpoint for Earn, Redeem, and Adjust.</summary>
    [HttpPost("manage")]
    public async Task<IActionResult> ManagePoints([FromBody] ManagePointsRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var action = request.ActionType?.Trim().ToLowerInvariant();
            switch (action)
            {
                case "redeem":
                    var rTxn = await _loyaltyService.RedeemPointsAsync(request.CustomerId, request.Points, request.Note, ct);
                    return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(rTxn), $"{request.Points} points redeemed successfully."));
                case "adjust":
                    var aTxn = await _loyaltyService.AdjustPointsAsync(request.CustomerId, request.Points, request.Note, ct);
                    return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(aTxn), "Points adjusted successfully."));
                case "earn":
                default:
                    var eTxn = await _loyaltyService.EarnPointsAsync(request.CustomerId, request.Points, request.InvoiceId, request.Note, ct);
                    return Ok(ApiResponse<LoyaltyTransactionDto>.Ok(ToDto(eTxn), $"{request.Points} points earned successfully."));
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiErrorResponse.From("invalid_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From("insufficient_points", ex.Message, traceId: HttpContext.TraceIdentifier));
        }
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
