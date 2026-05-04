using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Inventory;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class BatchTrackingModel : SecurePageModel
{
    private readonly IBatchService _batchService;
    private readonly IApiClientService _apiClient;

    public List<BatchDto> Batches { get; private set; } = new();
    public List<BatchDto> ExpiringBatches { get; private set; } = new();
    public string? FilterStatus { get; private set; }

    // Create
    [BindProperty] public Guid CreateItemId { get; set; }
    [BindProperty] public string CreateBatchNumber { get; set; } = string.Empty;
    [BindProperty] public int CreateQuantity { get; set; }
    [BindProperty] public decimal CreateCostPrice { get; set; }
    [BindProperty] public DateTime CreateReceivedDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime? CreateExpiryDate { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }

    // Edit
    [BindProperty] public Guid EditBatchId { get; set; }
    [BindProperty] public string? EditBatchNumber { get; set; }
    [BindProperty] public int? EditQuantity { get; set; }
    [BindProperty] public decimal? EditCostPrice { get; set; }
    [BindProperty] public DateTime? EditExpiryDate { get; set; }
    [BindProperty] public string? EditNotes { get; set; }

    // Delete
    [BindProperty] public Guid DeleteBatchId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public BatchTrackingModel(IBatchService batchService, IApiClientService apiClient)
    {
        _batchService = batchService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? expiryStatus = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterStatus = expiryStatus;

        var tasks = new[]
        {
            _batchService.GetAllAsync(expiryStatus: expiryStatus),
            _batchService.GetExpiringAsync(30)
        };

        await Task.WhenAll(tasks);
        Batches = tasks[0].Result;
        ExpiringBatches = tasks[1].Result;

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new CreateBatchRequest
        {
            ItemId = CreateItemId,
            BatchNumber = CreateBatchNumber.Trim(),
            Quantity = CreateQuantity,
            CostPrice = CreateCostPrice,
            ReceivedDate = CreateReceivedDate,
            ExpiryDate = CreateExpiryDate,
            Notes = string.IsNullOrWhiteSpace(CreateNotes) ? null : CreateNotes.Trim()
        };

        await _batchService.CreateAsync(req);
        StatusMessage = $"Batch '{req.BatchNumber}' recorded.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new UpdateBatchRequest
        {
            BatchNumber = EditBatchNumber?.Trim(),
            Quantity = EditQuantity,
            CostPrice = EditCostPrice,
            ExpiryDate = EditExpiryDate,
            Notes = EditNotes?.Trim()
        };

        await _batchService.UpdateAsync(EditBatchId, req);
        StatusMessage = "Batch updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _batchService.DeleteAsync(DeleteBatchId);
        StatusMessage = "Batch deleted.";
        return RedirectToPage();
    }
}
