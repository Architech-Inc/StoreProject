using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Loyalty;
using StoreUI.Services;

namespace StoreUI.Pages;

public class LoyaltyModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public LoyaltyAccountDto? Account { get; private set; }
    public IReadOnlyList<LoyaltyTransactionDto> Transactions { get; private set; } = Array.Empty<LoyaltyTransactionDto>();

    // Lookup
    [BindProperty(SupportsGet = true)]
    public Guid? CustomerId { get; set; }

    // Earn
    [BindProperty] public Guid EarnCustomerId { get; set; }
    [BindProperty] public int EarnPoints { get; set; }
    [BindProperty] public string? EarnNote { get; set; }

    // Redeem
    [BindProperty] public Guid RedeemCustomerId { get; set; }
    [BindProperty] public int RedeemPoints { get; set; }
    [BindProperty] public string? RedeemNote { get; set; }

    // Adjust
    [BindProperty] public Guid AdjustCustomerId { get; set; }
    [BindProperty] public int AdjustPoints { get; set; }
    [BindProperty] public string? AdjustNote { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public LoyaltyModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (CustomerId.HasValue)
        {
            Account = await _apiClient.GetAsync<LoyaltyAccountDto>($"api/loyalty/customers/{CustomerId}", ct);
            if (Account is not null)
            {
                var txns = await _apiClient.GetAsync<IEnumerable<LoyaltyTransactionDto>>(
                    $"api/loyalty/customers/{CustomerId}/transactions?take=30", ct);
                Transactions = txns?.ToList() ?? new List<LoyaltyTransactionDto>();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEarnAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var ok = await _apiClient.PostAsync("api/loyalty/earn",
            new EarnPointsRequest { CustomerId = EarnCustomerId, Points = EarnPoints, Note = EarnNote }, ct);
        StatusMessage = ok ? $"Earned {EarnPoints} points." : "Error: Could not earn points.";
        return RedirectToPage(new { customerId = EarnCustomerId });
    }

    public async Task<IActionResult> OnPostRedeemAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var ok = await _apiClient.PostAsync("api/loyalty/redeem",
            new RedeemPointsRequest { CustomerId = RedeemCustomerId, Points = RedeemPoints, Note = RedeemNote }, ct);
        StatusMessage = ok ? $"Redeemed {RedeemPoints} points." : "Error: Could not redeem points (check balance).";
        return RedirectToPage(new { customerId = RedeemCustomerId });
    }

    public async Task<IActionResult> OnPostAdjustAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var ok = await _apiClient.PostAsync("api/loyalty/adjust",
            new AdjustPointsRequest { CustomerId = AdjustCustomerId, Points = AdjustPoints, Note = AdjustNote }, ct);
        StatusMessage = ok ? "Points adjusted." : "Error: Adjustment failed.";
        return RedirectToPage(new { customerId = AdjustCustomerId });
    }
}
