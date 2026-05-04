using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Loyalty;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CampaignsModel : SecurePageModel
{
    private readonly ILoyaltyCampaignService _campaignService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<LoyaltyCampaignDto> Campaigns { get; private set; } = Array.Empty<LoyaltyCampaignDto>();

    // Create
    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public string? Description { get; set; }
    [BindProperty] public string CampaignType { get; set; } = "PointMultiplier";
    [BindProperty] public string? TargetSegment { get; set; }
    [BindProperty] public decimal MultiplierFactor { get; set; } = 1m;
    [BindProperty] public int BonusPoints { get; set; } = 0;
    [BindProperty] public DateTime StartDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
    [BindProperty] public bool IsActive { get; set; } = true;

    // Edit
    [BindProperty] public int EditCampaignId { get; set; }
    [BindProperty] public string EditName { get; set; } = string.Empty;
    [BindProperty] public string? EditDescription { get; set; }
    [BindProperty] public string EditCampaignType { get; set; } = "PointMultiplier";
    [BindProperty] public string? EditTargetSegment { get; set; }
    [BindProperty] public decimal EditMultiplierFactor { get; set; } = 1m;
    [BindProperty] public int EditBonusPoints { get; set; } = 0;
    [BindProperty] public DateTime EditStartDate { get; set; }
    [BindProperty] public DateTime EditEndDate { get; set; }
    [BindProperty] public bool EditIsActive { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public CampaignsModel(ILoyaltyCampaignService campaignService, IApiClientService apiClient)
    {
        _campaignService = campaignService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        var list = await _campaignService.GetAllAsync(activeOnly: null, ct);
        Campaigns = list.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (!Enum.TryParse<LoyaltyCampaignType>(CampaignType, out var type))
            type = LoyaltyCampaignType.PointMultiplier;

        CustomerSegment? segment = null;
        if (!string.IsNullOrWhiteSpace(TargetSegment) && Enum.TryParse<CustomerSegment>(TargetSegment, out var seg))
            segment = seg;

        var req = new CreateCampaignRequest
        {
            Name = Name.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            CampaignType = type,
            TargetSegment = segment,
            MultiplierFactor = MultiplierFactor,
            BonusPoints = BonusPoints,
            StartDate = StartDate,
            EndDate = EndDate,
            IsActive = IsActive
        };

        await _campaignService.CreateAsync(req, ct);
        StatusMessage = $"Campaign '{req.Name}' created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        LoyaltyCampaignType? type = Enum.TryParse<LoyaltyCampaignType>(EditCampaignType, out var t) ? t : null;

        CustomerSegment? segment = null;
        if (!string.IsNullOrWhiteSpace(EditTargetSegment) && Enum.TryParse<CustomerSegment>(EditTargetSegment, out var seg))
            segment = seg;

        var req = new UpdateCampaignRequest
        {
            Name = EditName.Trim(),
            Description = string.IsNullOrWhiteSpace(EditDescription) ? null : EditDescription.Trim(),
            CampaignType = type,
            TargetSegment = segment,
            MultiplierFactor = EditMultiplierFactor,
            BonusPoints = EditBonusPoints,
            StartDate = EditStartDate,
            EndDate = EditEndDate,
            IsActive = EditIsActive
        };

        await _campaignService.UpdateAsync(EditCampaignId, req, ct);
        StatusMessage = $"Campaign '{EditName}' updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int campaignId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        await _campaignService.DeleteAsync(campaignId, ct);
        StatusMessage = "Campaign deleted.";
        return RedirectToPage();
    }
}
