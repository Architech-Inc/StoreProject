using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiLoyaltyService : ILoyaltyService
{
    private readonly IApiClientService _client;

    public ApiLoyaltyService(IApiClientService client) => _client = client;

    public async Task<CustomerLoyaltyAccount?> GetAccountAsync(Guid customerId, CancellationToken ct = default)
    {
        var dto = await _client.GetAsync<LoyaltyAccountDto>($"/api/loyalty/customers/{customerId}");
        if (dto is null) return null;

        Enum.TryParse<LoyaltyTier>(dto.Tier, ignoreCase: true, out var tier);
        return new CustomerLoyaltyAccount
        {
            LoyaltyAccountId = dto.LoyaltyAccountId,
            CustomerId = dto.CustomerId,
            Points = dto.Points,
            Tier = tier
        };
    }

    public async Task<CustomerLoyaltyAccount> GetOrCreateAccountAsync(Guid customerId, CancellationToken ct = default)
    {
        var account = await GetAccountAsync(customerId, ct);
        if (account is not null) return account;

        return new CustomerLoyaltyAccount
        {
            CustomerId = customerId,
            Points = 0,
            Tier = LoyaltyTier.Bronze
        };
    }

    public async Task<LoyaltyTransaction> EarnPointsAsync(Guid customerId, int points, Guid? invoiceId, string? note, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<LoyaltyTransactionDto>("/api/loyalty/earn", new EarnPointsRequest
        {
            CustomerId = customerId,
            Points = points,
            InvoiceId = invoiceId,
            Note = note
        });

        if (result is null) throw new InvalidOperationException("Failed to award loyalty points.");

        return new LoyaltyTransaction
        {
            LoyaltyTransactionId = result.LoyaltyTransactionId,
            Points = result.Points,
            InvoiceId = result.InvoiceId,
            Note = result.Note,
            DateCreated = result.DateCreated
        };
    }

    public async Task<LoyaltyTransaction> RedeemPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<LoyaltyTransactionDto>("/api/loyalty/redeem", new RedeemPointsRequest
        {
            CustomerId = customerId,
            Points = points,
            Note = note
        });

        if (result is null) throw new InvalidOperationException("Failed to redeem loyalty points.");

        return new LoyaltyTransaction
        {
            LoyaltyTransactionId = result.LoyaltyTransactionId,
            Points = result.Points,
            Note = result.Note,
            DateCreated = result.DateCreated
        };
    }

    public async Task<LoyaltyTransaction> AdjustPointsAsync(Guid customerId, int points, string? note, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<LoyaltyTransactionDto>("/api/loyalty/adjust", new AdjustPointsRequest
        {
            CustomerId = customerId,
            Points = points,
            Note = note
        });

        if (result is null) throw new InvalidOperationException("Failed to adjust loyalty points.");

        return new LoyaltyTransaction
        {
            LoyaltyTransactionId = result.LoyaltyTransactionId,
            Points = result.Points,
            Note = result.Note,
            DateCreated = result.DateCreated
        };
    }

    public async Task<IEnumerable<LoyaltyTransaction>> GetTransactionsAsync(Guid customerId, int take = 50, CancellationToken ct = default)
    {
        var dtos = await _client.GetAsync<List<LoyaltyTransactionDto>>($"/api/loyalty/customers/{customerId}/transactions?take={take}") ?? new();
        return dtos.Select(t =>
        {
            Enum.TryParse<LoyaltyTransactionType>(t.TransactionType, ignoreCase: true, out var tType);
            return new LoyaltyTransaction
            {
                LoyaltyTransactionId = t.LoyaltyTransactionId,
                Points = t.Points,
                TransactionType = tType,
                InvoiceId = t.InvoiceId,
                Note = t.Note,
                DateCreated = t.DateCreated
            };
        });
    }

    public async Task<LoyaltyMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _client.GetAsync<LoyaltyMetricsDto>("/api/loyalty/metrics") ?? new();

    public async Task<PagedResult<LoyaltyMemberDto>> GetAllMembersAsync(
        string? search = null,
        string? tier = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(tier)) queryParams.Add($"tier={Uri.EscapeDataString(tier)}");
        if (!string.IsNullOrWhiteSpace(sortBy)) queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var queryString = "?" + string.Join("&", queryParams);
        return await _client.GetAsync<PagedResult<LoyaltyMemberDto>>($"/api/loyalty/members{queryString}")
               ?? new PagedResult<LoyaltyMemberDto>(new List<LoyaltyMemberDto>(), 0, page, pageSize);
    }

    public async Task<LoyaltyMemberProfileDto?> GetMemberProfileAsync(Guid customerId, CancellationToken ct = default)
        => await _client.GetAsync<LoyaltyMemberProfileDto>($"/api/loyalty/customers/{customerId}/profile");

    public async Task<IEnumerable<GlobalLoyaltyTransactionDto>> GetGlobalTransactionsAsync(
        string? search = null,
        string? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int take = 50,
        CancellationToken ct = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(transactionType)) queryParams.Add($"transactionType={Uri.EscapeDataString(transactionType)}");
        if (fromDate.HasValue) queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
        queryParams.Add($"take={take}");

        var queryString = "?" + string.Join("&", queryParams);
        return await _client.GetAsync<List<GlobalLoyaltyTransactionDto>>($"/api/loyalty/transactions{queryString}") ?? new();
    }
}
