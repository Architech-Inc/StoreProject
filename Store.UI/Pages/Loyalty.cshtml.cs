using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.DTOs.Loyalty;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class LoyaltyModel : SecurePageModel
{
    private readonly ILoyaltyService _loyaltyService;
    private readonly ICustomerService _customerService;
    private readonly IApiClientService _apiClient;

    public LoyaltyMetricsDto Metrics { get; set; } = new();
    public PagedResult<LoyaltyMemberDto> Members { get; set; } = new(new List<LoyaltyMemberDto>(), 0, 1, 18);
    public LoyaltyMemberProfileDto? SelectedProfile { get; set; }
    public IEnumerable<GlobalLoyaltyTransactionDto> RecentGlobalTransactions { get; set; } = Enumerable.Empty<GlobalLoyaltyTransactionDto>();

    // Filters & Pagination
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Tier { get; set; }
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; } = "points_desc";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 18;
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "grid"; // grid | table | ledger

    // Deep Link Profile
    [BindProperty(SupportsGet = true)] public Guid? CustomerId { get; set; }

    // Unified Manage Points Form
    [BindProperty] public ManagePointsRequest ManageForm { get; set; } = new();

    // Legacy Action Bindings for backwards compatibility
    [BindProperty] public Guid EarnCustomerId { get; set; }
    [BindProperty] public int EarnPoints { get; set; }
    [BindProperty] public string? EarnNote { get; set; }

    [BindProperty] public Guid RedeemCustomerId { get; set; }
    [BindProperty] public int RedeemPoints { get; set; }
    [BindProperty] public string? RedeemNote { get; set; }

    [BindProperty] public Guid AdjustCustomerId { get; set; }
    [BindProperty] public int AdjustPoints { get; set; }
    [BindProperty] public string? AdjustNote { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public LoyaltyModel(
        ILoyaltyService loyaltyService,
        ICustomerService customerService,
        IApiClientService apiClient)
    {
        _loyaltyService = loyaltyService;
        _customerService = customerService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        // 1. Fetch KPI Metrics
        Metrics = await _loyaltyService.GetMetricsAsync(ct);

        // 2. Fetch Paged Member Directory
        Members = await _loyaltyService.GetAllMembersAsync(Search, Tier, SortBy, PageNumber, PageSize, ct);

        // 3. If deep-linked or selected, fetch full 360 profile
        if (CustomerId.HasValue && CustomerId.Value != Guid.Empty)
        {
            SelectedProfile = await _loyaltyService.GetMemberProfileAsync(CustomerId.Value, ct);
        }

        // 4. If Ledger tab active, fetch global transaction stream
        if (ViewMode == "ledger")
        {
            RecentGlobalTransactions = await _loyaltyService.GetGlobalTransactionsAsync(Search, null, null, null, 100, ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var profile = await _loyaltyService.GetMemberProfileAsync(customerId, ct);
        if (profile is null) return NotFound();

        return new JsonResult(profile);
    }

    public async Task<IActionResult> OnGetSearchCustomersAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _customerService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 15, SearchTerm = q?.Trim() }, ct);
        var items = result.Items.Select(c => new
        {
            id = c.CustomerId.ToString(),
            title = c.FullName,
            sub = $"Phone: {(string.IsNullOrEmpty(c.PrimaryPhone) ? "—" : c.PrimaryPhone)} | Tier: {c.LoyaltyTier} ({c.LoyaltyPoints:N0} pts)",
            badge = c.Segment.ToString(),
            points = c.LoyaltyPoints,
            tier = c.LoyaltyTier.ToString()
        });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnGetExportCsvAsync(
        [FromQuery] string? search = null,
        [FromQuery] string? tier = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var paged = await _loyaltyService.GetAllMembersAsync(search, tier, "points_desc", 1, 10000, ct);

        var csv = new StringBuilder();
        csv.AppendLine("Member ID,Customer Name,Phone,Email,Segment,Current Points,Loyalty Tier,Estimated Value (XAF),Lifetime Points Earned,Total Points Redeemed,Last Transaction Date,Enrolled Date");

        foreach (var m in paged.Items)
        {
            csv.AppendLine($"\"{m.LoyaltyAccountId}\",\"{EscapeCsv(m.FullName)}\",\"{EscapeCsv(m.PrimaryPhone)}\",\"{EscapeCsv(m.PrimaryEmail)}\",\"{EscapeCsv(m.Segment)}\",{m.Points},\"{m.Tier}\",{m.EstimatedMonetaryValue},{m.LifetimePointsEarned},{m.TotalPointsRedeemed},\"{m.LastTransactionDate:yyyy-MM-dd HH:mm}\",\"{m.DateEnrolled:yyyy-MM-dd}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var filename = $"Loyalty_Members_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv; charset=utf-8", filename);
    }

    public async Task<IActionResult> OnPostManagePointsAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        if (ManageForm.CustomerId == Guid.Empty || ManageForm.Points <= 0)
        {
            StatusMessage = "Error: Please select a valid customer and specify points greater than 0.";
            return RedirectToPage("/Loyalty", new { search = Search, tier = Tier, sortBy = SortBy, viewMode = ViewMode, customerId = ManageForm.CustomerId });
        }

        try
        {
            var action = ManageForm.ActionType?.Trim().ToLowerInvariant();
            switch (action)
            {
                case "redeem":
                    await _loyaltyService.RedeemPointsAsync(ManageForm.CustomerId, ManageForm.Points, ManageForm.Note, ct);
                    StatusMessage = $"Success: Successfully redeemed {ManageForm.Points} points ({ManageForm.Points * 5:N0} XAF voucher).";
                    break;
                case "adjust":
                    await _loyaltyService.AdjustPointsAsync(ManageForm.CustomerId, ManageForm.Points, ManageForm.Note, ct);
                    StatusMessage = $"Success: Successfully adjusted points balance by {ManageForm.Points} points.";
                    break;
                case "earn":
                default:
                    await _loyaltyService.EarnPointsAsync(ManageForm.CustomerId, ManageForm.Points, ManageForm.InvoiceId, ManageForm.Note, ct);
                    StatusMessage = $"Success: Successfully awarded {ManageForm.Points} points to customer.";
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: An unexpected error occurred ({ex.Message}).";
        }

        return RedirectToPage("/Loyalty", new { search = Search, tier = Tier, sortBy = SortBy, viewMode = ViewMode, customerId = ManageForm.CustomerId });
    }

    // Legacy Fallback Handlers
    public async Task<IActionResult> OnPostEarnAsync(CancellationToken ct = default)
    {
        ManageForm = new ManagePointsRequest
        {
            CustomerId = EarnCustomerId,
            Points = EarnPoints,
            Note = EarnNote,
            ActionType = "Earn"
        };
        return await OnPostManagePointsAsync(ct);
    }

    public async Task<IActionResult> OnPostRedeemAsync(CancellationToken ct = default)
    {
        ManageForm = new ManagePointsRequest
        {
            CustomerId = RedeemCustomerId,
            Points = RedeemPoints,
            Note = RedeemNote,
            ActionType = "Redeem"
        };
        return await OnPostManagePointsAsync(ct);
    }

    public async Task<IActionResult> OnPostAdjustAsync(CancellationToken ct = default)
    {
        ManageForm = new ManagePointsRequest
        {
            CustomerId = AdjustCustomerId,
            Points = AdjustPoints,
            Note = AdjustNote,
            ActionType = "Adjust"
        };
        return await OnPostManagePointsAsync(ct);
    }

    private static string EscapeCsv(string? input) =>
        string.IsNullOrEmpty(input) ? string.Empty : input.Replace("\"", "\"\"");
}
