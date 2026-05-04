using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Discounts;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class DiscountsModel : SecurePageModel
{
    private readonly IDiscountService _discountService;
    private readonly IApiClientService _apiClient;

    public List<DiscountDto> Discounts { get; private set; } = new();
    public bool FilterActiveOnly { get; private set; }

    // Create
    [BindProperty] public string CreateName { get; set; } = string.Empty;
    [BindProperty] public int CreateDiscountType { get; set; } = 0;
    [BindProperty] public decimal CreatePercentage { get; set; }
    [BindProperty] public decimal? CreateFixedAmount { get; set; }
    [BindProperty] public int CreateMinQuantity { get; set; } = 1;
    [BindProperty] public string? CreateTargetSegment { get; set; }
    [BindProperty] public string? CreateCouponCode { get; set; }
    [BindProperty] public int? CreateMaxUses { get; set; }
    [BindProperty] public DateTime? CreateValidFrom { get; set; }
    [BindProperty] public DateTime? CreateValidTo { get; set; }
    [BindProperty] public bool CreateIsActive { get; set; } = true;

    // Edit
    [BindProperty] public int EditDiscountId { get; set; }
    [BindProperty] public string? EditName { get; set; }
    [BindProperty] public int? EditDiscountType { get; set; }
    [BindProperty] public decimal? EditPercentage { get; set; }
    [BindProperty] public decimal? EditFixedAmount { get; set; }
    [BindProperty] public int? EditMinQuantity { get; set; }
    [BindProperty] public string? EditTargetSegment { get; set; }
    [BindProperty] public string? EditCouponCode { get; set; }
    [BindProperty] public int? EditMaxUses { get; set; }
    [BindProperty] public DateTime? EditValidFrom { get; set; }
    [BindProperty] public DateTime? EditValidTo { get; set; }
    [BindProperty] public bool? EditIsActive { get; set; }

    // Delete
    [BindProperty] public int DeleteDiscountId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public DiscountsModel(IDiscountService discountService, IApiClientService apiClient)
    {
        _discountService = discountService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] bool activeOnly = false)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterActiveOnly = activeOnly;
        Discounts = await _discountService.GetAllAsync(activeOnly ? true : null);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        CustomerSegment? segment = null;
        if (!string.IsNullOrWhiteSpace(CreateTargetSegment) && int.TryParse(CreateTargetSegment, out var si))
            segment = (CustomerSegment)si;

        var req = new CreateDiscountRequest
        {
            Name = CreateName.Trim(),
            DiscountType = (DiscountType)CreateDiscountType,
            Percentage = CreatePercentage,
            FixedAmount = CreateFixedAmount,
            MinQuantity = CreateMinQuantity,
            TargetSegment = segment,
            CouponCode = string.IsNullOrWhiteSpace(CreateCouponCode) ? null : CreateCouponCode.Trim().ToUpperInvariant(),
            MaxUses = CreateMaxUses,
            ValidFrom = CreateValidFrom,
            ValidTo = CreateValidTo,
            IsActive = CreateIsActive
        };

        await _discountService.CreateAsync(req, Guid.Empty); // userId resolved server-side by API
        StatusMessage = $"Discount '{req.Name}' created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        CustomerSegment? segment = null;
        if (!string.IsNullOrWhiteSpace(EditTargetSegment) && int.TryParse(EditTargetSegment, out var si))
            segment = (CustomerSegment)si;

        var req = new UpdateDiscountRequest
        {
            Name = EditName?.Trim(),
            DiscountType = EditDiscountType.HasValue ? (DiscountType?)EditDiscountType.Value : null,
            Percentage = EditPercentage,
            FixedAmount = EditFixedAmount,
            MinQuantity = EditMinQuantity,
            TargetSegment = segment,
            CouponCode = EditCouponCode?.Trim().ToUpperInvariant(),
            MaxUses = EditMaxUses,
            ValidFrom = EditValidFrom,
            ValidTo = EditValidTo,
            IsActive = EditIsActive
        };

        await _discountService.UpdateAsync(EditDiscountId, req);
        StatusMessage = $"Discount updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _discountService.DeleteAsync(DeleteDiscountId);
        StatusMessage = "Discount deleted.";
        return RedirectToPage();
    }
}
