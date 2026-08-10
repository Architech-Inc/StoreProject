using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.DTOs.Loyalty;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CampaignsModel : SecurePageModel
{
    private readonly ILoyaltyCampaignService _campaignService;
    private readonly ICustomerService _customerService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<LoyaltyCampaignDto> Campaigns { get; private set; } = Array.Empty<LoyaltyCampaignDto>();

    // KPI Metrics
    public int LiveCampaignsCount { get; private set; }
    public int ScheduledCampaignsCount { get; private set; }
    public int TotalAudienceReach { get; private set; }
    public decimal PeakMultiplier { get; private set; }

    // Segment Metrics for Audience Calculations
    public int TotalMembersCount { get; private set; }
    public int StandardMembersCount { get; private set; }
    public int WholesaleMembersCount { get; private set; }
    public int VipMembersCount { get; private set; }

    // Query & Filtering
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? SegmentFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "start_asc";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "grid";

    // Status Tab Counters
    public int CountAll { get; private set; }
    public int CountLive { get; private set; }
    public int CountScheduled { get; private set; }
    public int CountCompleted { get; private set; }
    public int CountInactive { get; private set; }

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

    public CampaignsModel(
        ILoyaltyCampaignService campaignService, 
        ICustomerService customerService,
        IApiClientService apiClient)
    {
        _campaignService = campaignService;
        _customerService = customerService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        
        await LoadAudienceMetricsAsync(ct);

        var list = await _campaignService.GetAllAsync(activeOnly: null, ct);
        var campaigns = list.ToList();
        
        CalculateKpis(campaigns);
        ApplyFiltersAndSort(campaigns);

        return Page();
    }

    private async Task LoadAudienceMetricsAsync(CancellationToken ct)
    {
        try 
        {
            var custRes = await _customerService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 10000 }, ct);
            var customers = custRes.Items;
            
            TotalMembersCount = customers.Count();
            StandardMembersCount = customers.Count(c => c.Segment == CustomerSegment.Standard);
            WholesaleMembersCount = customers.Count(c => c.Segment == CustomerSegment.Wholesale);
            VipMembersCount = customers.Count(c => c.Segment == CustomerSegment.Vip);
        }
        catch 
        {
            // Fail silently on audience stats if issues occur
        }
    }

    private void CalculateKpis(List<LoyaltyCampaignDto> allCampaigns)
    {
        var now = DateTime.UtcNow;
        
        CountAll = allCampaigns.Count;
        CountLive = allCampaigns.Count(c => c.IsRunning);
        CountScheduled = allCampaigns.Count(c => c.IsActive && c.StartDate > now);
        CountCompleted = allCampaigns.Count(c => c.IsActive && c.EndDate < now);
        CountInactive = allCampaigns.Count(c => !c.IsActive);

        LiveCampaignsCount = CountLive;
        ScheduledCampaignsCount = CountScheduled;
        
        PeakMultiplier = allCampaigns.Where(c => c.IsRunning && c.CampaignType == "PointMultiplier")
                                     .Select(c => c.MultiplierFactor)
                                     .DefaultIfEmpty(0m).Max();
                                     
        // Calculate audience reach based on live campaigns
        var liveSegments = allCampaigns.Where(c => c.IsRunning)
                                       .Select(c => c.TargetSegment)
                                       .Distinct()
                                       .ToList();
        
        if (liveSegments.Contains(null) || liveSegments.Contains(""))
        {
            TotalAudienceReach = TotalMembersCount;
        }
        else
        {
            int reach = 0;
            if (liveSegments.Contains(CustomerSegment.Standard.ToString())) reach += StandardMembersCount;
            if (liveSegments.Contains(CustomerSegment.Wholesale.ToString())) reach += WholesaleMembersCount;
            if (liveSegments.Contains(CustomerSegment.Vip.ToString())) reach += VipMembersCount;
            TotalAudienceReach = reach;
        }
    }

    private void ApplyFiltersAndSort(List<LoyaltyCampaignDto> allCampaigns)
    {
        IEnumerable<LoyaltyCampaignDto> query = allCampaigns;

        // Apply Status Filter
        var now = DateTime.UtcNow;
        if (StatusFilter == "Live") query = query.Where(c => c.IsRunning);
        else if (StatusFilter == "Scheduled") query = query.Where(c => c.IsActive && c.StartDate > now);
        else if (StatusFilter == "Completed") query = query.Where(c => c.IsActive && c.EndDate < now);
        else if (StatusFilter == "Inactive") query = query.Where(c => !c.IsActive);

        // Apply Segment Filter
        if (!string.IsNullOrWhiteSpace(SegmentFilter))
        {
            query = query.Where(c => string.Equals(c.TargetSegment, SegmentFilter, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(c.TargetSegment));
        }

        // Apply Type Filter
        if (!string.IsNullOrWhiteSpace(TypeFilter))
        {
            query = query.Where(c => string.Equals(c.CampaignType, TypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        // Apply Search
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(s) || 
                (c.Description != null && c.Description.ToLower().Contains(s)));
        }

        // Apply Sorting
        query = SortBy switch
        {
            "start_asc" => query.OrderBy(c => c.StartDate),
            "start_desc" => query.OrderByDescending(c => c.StartDate),
            "multiplier_desc" => query.OrderByDescending(c => c.MultiplierFactor).ThenByDescending(c => c.BonusPoints),
            "name_asc" => query.OrderBy(c => c.Name),
            "newest" => query.OrderByDescending(c => c.LoyaltyCampaignId),
            _ => query.OrderBy(c => c.StartDate)
        };

        Campaigns = query.ToList();
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
            MultiplierFactor = type == LoyaltyCampaignType.PointMultiplier ? MultiplierFactor : 1m,
            BonusPoints = type == LoyaltyCampaignType.FixedBonusPoints ? BonusPoints : 0,
            StartDate = StartDate,
            EndDate = EndDate,
            IsActive = IsActive
        };

        await _campaignService.CreateAsync(req, ct);
        StatusMessage = $"Campaign '{req.Name}' created successfully.";
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
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
            MultiplierFactor = type == LoyaltyCampaignType.PointMultiplier ? EditMultiplierFactor : 1m,
            BonusPoints = type == LoyaltyCampaignType.FixedBonusPoints ? EditBonusPoints : 0,
            StartDate = EditStartDate,
            EndDate = EditEndDate,
            IsActive = EditIsActive
        };

        await _campaignService.UpdateAsync(EditCampaignId, req, ct);
        StatusMessage = $"Campaign '{EditName}' updated successfully.";
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int campaignId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        await _campaignService.DeleteAsync(campaignId, ct);
        StatusMessage = "Campaign deleted.";
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int campaignId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        
        var campaign = await _campaignService.GetByIdAsync(campaignId, ct);
        if (campaign == null)
        {
            StatusMessage = "Error: Campaign not found.";
            return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
        }

        var req = new UpdateCampaignRequest { IsActive = !campaign.IsActive };
        await _campaignService.UpdateAsync(campaignId, req, ct);
        StatusMessage = $"Campaign '{(campaign.Name)}' is now {(req.IsActive.Value ? "Active" : "Paused")}.";
        
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
    }

    public async Task<IActionResult> OnPostExtendAsync(int campaignId, int days, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        
        var campaign = await _campaignService.GetByIdAsync(campaignId, ct);
        if (campaign == null)
        {
            StatusMessage = "Error: Campaign not found.";
            return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
        }

        var newEndDate = campaign.EndDate.AddDays(days);
        var req = new UpdateCampaignRequest { EndDate = newEndDate };
        
        await _campaignService.UpdateAsync(campaignId, req, ct);
        StatusMessage = $"Campaign '{(campaign.Name)}' extended by {days} days (ends {newEndDate:MMM dd, yyyy}).";
        
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
    }

    public async Task<IActionResult> OnPostCloneAsync(int campaignId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        
        var campaign = await _campaignService.GetByIdAsync(campaignId, ct);
        if (campaign == null)
        {
            StatusMessage = "Error: Campaign not found.";
            return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
        }

        CustomerSegment? segment = null;
        if (!string.IsNullOrWhiteSpace(campaign.TargetSegment) && Enum.TryParse<CustomerSegment>(campaign.TargetSegment, out var seg))
            segment = seg;

        Enum.TryParse<LoyaltyCampaignType>(campaign.CampaignType, out var type);

        // Standard clone shifts +7 days forward by default
        var duration = campaign.EndDate - campaign.StartDate;
        var newStart = DateTime.Today.AddDays(7);
        var newEnd = newStart.Add(duration);

        var req = new CreateCampaignRequest
        {
            Name = $"{campaign.Name} (Copy)",
            Description = campaign.Description,
            CampaignType = type,
            TargetSegment = segment,
            MultiplierFactor = campaign.MultiplierFactor,
            BonusPoints = campaign.BonusPoints,
            StartDate = newStart,
            EndDate = newEnd,
            IsActive = false // Start paused
        };

        var newCamp = await _campaignService.CreateAsync(req, ct);
        StatusMessage = $"Campaign cloned as '{newCamp.Name}'. Review dates and activate.";
        return RedirectToPage(new { ViewMode, StatusFilter, SegmentFilter, TypeFilter });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var list = await _campaignService.GetAllAsync(activeOnly: null, ct);
        var campaigns = list.ToList();
        
        ApplyFiltersAndSort(campaigns); // Re-use the existing filter logic based on query string

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Type,Target Segment,Multiplier,Bonus Points,Start Date,End Date,Status");

        foreach (var c in Campaigns)
        {
            var status = c.IsRunning ? "Live" : (!c.IsActive ? "Paused" : (c.EndDate < DateTime.UtcNow ? "Completed" : "Scheduled"));
            var target = string.IsNullOrWhiteSpace(c.TargetSegment) ? "All" : c.TargetSegment;
            var name = $"\"{c.Name.Replace("\"", "\"\"")}\"";
            
            sb.AppendLine($"{c.LoyaltyCampaignId},{name},{c.CampaignType},{target},{c.MultiplierFactor},{c.BonusPoints},{c.StartDate:yyyy-MM-dd},{c.EndDate:yyyy-MM-dd},{status}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"campaigns_export_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
