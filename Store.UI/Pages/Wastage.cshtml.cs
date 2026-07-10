using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Inventory;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class WastageModel : SecurePageModel
{
    private readonly IWastageService _wastageService;
    private readonly IApiClientService _apiClient;
    private readonly IItemService _itemService;

    public List<WastageEntryDto> Entries { get; private set; } = new();
    public string? FilterType { get; private set; }

    [BindProperty] public Guid RecordItemId { get; set; }
    [BindProperty] public WastageType RecordWastageType { get; set; }
    [BindProperty] public int RecordQuantity { get; set; }
    [BindProperty] public string? RecordNotes { get; set; }
    [BindProperty] public string? RecordReferenceCode { get; set; }

    [BindProperty] public int DeleteEntryId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<WastageType> WastageTypes { get; } = Enum.GetValues<WastageType>();

    public WastageModel(IWastageService wastageService, IApiClientService apiClient, IItemService itemService)
    {
        _wastageService = wastageService;
        _apiClient = apiClient;
        _itemService = itemService;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? wastageType = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterType = wastageType;
        Entries = await _wastageService.GetAllAsync(wastageType: wastageType);
        return Page();
    }

    public async Task<IActionResult> OnPostRecordAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new RecordWastageRequest
        {
            ItemId = RecordItemId,
            WastageType = RecordWastageType,
            Quantity = RecordQuantity,
            Notes = RecordNotes,
            ReferenceCode = RecordReferenceCode
        };

        await _wastageService.RecordAsync(req, Guid.Empty); // userId resolved by API via JWT
        StatusMessage = "Wastage entry recorded successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var ok = await _wastageService.DeleteAsync(DeleteEntryId);
        StatusMessage = ok ? "Wastage entry deleted." : "Entry not found.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetSearchItemsAsync(string q)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        
        var req = new Store.Models.DTOs.Common.PagedRequest 
        { 
            SearchTerm = q, 
            PageSize = 20 
        };
        var result = await _itemService.GetAllAsync(req);
        
        var selectList = result.Items.Select(i => new { id = i.ItemId, text = $"{i.Name} ({i.Barcode})" });
        return new JsonResult(selectList);
    }
}
