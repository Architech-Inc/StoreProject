using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.DTOs.Loyalty;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CustomersModel : SecurePageModel
{
    private readonly ICustomerService _customerService;
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;

    public IReadOnlyList<CustomerDto> Customers { get; private set; } = Array.Empty<CustomerDto>();
    public int TotalCustomers { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 24;
    public int TotalPages => (int)Math.Ceiling((double)TotalCustomers / PageSize);

    // KPI Metrics
    public int TotalCustomerCount { get; private set; }
    public int VipCustomerCount { get; private set; }
    public int WholesaleCustomerCount { get; private set; }
    public int TotalLoyaltyPoints { get; private set; }
    public decimal TotalReceivables { get; private set; }

    // Query & Filtering
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? SegmentFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? TierFilter { get; set; }
    [BindProperty(SupportsGet = true)] public bool HasDebtOnly { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "name_asc";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "grid";
    [BindProperty(SupportsGet = true)] public Guid? CustomerId { get; set; }

    // Create Form
    [BindProperty] public string FirstName { get; set; } = string.Empty;
    [BindProperty] public string LastName { get; set; } = string.Empty;
    [BindProperty] public string? MiddleName { get; set; }
    [BindProperty] public Gender Gender { get; set; } = Gender.NotSpecified;
    [BindProperty] public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
    [BindProperty] public string? Phone { get; set; }
    [BindProperty] public string? Email { get; set; }
    [BindProperty] public string? Notes { get; set; }
    [BindProperty] public IFormFile? ImageUpload { get; set; }
    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    // Edit Form
    [BindProperty] public Guid EditCustomerId { get; set; }
    [BindProperty] public string EditFirstName { get; set; } = string.Empty;
    [BindProperty] public string EditLastName { get; set; } = string.Empty;
    [BindProperty] public string? EditMiddleName { get; set; }
    [BindProperty] public Gender EditGender { get; set; } = Gender.NotSpecified;
    [BindProperty] public CustomerSegment EditSegment { get; set; } = CustomerSegment.Standard;
    [BindProperty] public string? EditPhone { get; set; }
    [BindProperty] public string? EditEmail { get; set; }
    [BindProperty] public string? EditNotes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public CustomersModel(ICustomerService customerService, IApiClientService apiClient, IFileService fileService)
    {
        _customerService = customerService;
        _apiClient = apiClient;
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        PageNumber = Math.Max(1, page);

        // Fetch wide pool or paginated list
        var fetchResult = await _customerService.GetAllAsync(
            new PagedRequest { Page = 1, PageSize = 1000, SearchTerm = Search }, ct);

        var allFetched = fetchResult.Items.ToList();

        // Calculate KPI Metrics
        TotalCustomerCount = allFetched.Count;
        VipCustomerCount = allFetched.Count(c => c.Segment == CustomerSegment.Vip);
        WholesaleCustomerCount = allFetched.Count(c => c.Segment == CustomerSegment.Wholesale);
        TotalLoyaltyPoints = allFetched.Sum(c => c.LoyaltyPoints);
        TotalReceivables = allFetched.Sum(c => c.OutstandingBalance);

        // Apply filters in memory
        IEnumerable<CustomerDto> filtered = allFetched;

        if (!string.IsNullOrWhiteSpace(SegmentFilter) && Enum.TryParse<CustomerSegment>(SegmentFilter, true, out var seg))
        {
            filtered = filtered.Where(c => c.Segment == seg);
        }

        if (!string.IsNullOrWhiteSpace(TierFilter) && Enum.TryParse<LoyaltyTier>(TierFilter, true, out var tier))
        {
            filtered = filtered.Where(c => c.LoyaltyTier == tier);
        }

        if (HasDebtOnly)
        {
            filtered = filtered.Where(c => c.OutstandingBalance > 0);
        }

        // Apply sorting
        filtered = SortBy switch
        {
            "name_desc" => filtered.OrderByDescending(c => c.LastName).ThenByDescending(c => c.FirstName),
            "ltv_desc" => filtered.OrderByDescending(c => c.LifetimeValue),
            "points_desc" => filtered.OrderByDescending(c => c.LoyaltyPoints),
            "debt_desc" => filtered.OrderByDescending(c => c.OutstandingBalance),
            "newest" => filtered.OrderByDescending(c => c.DateCreated),
            _ => filtered.OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
        };

        var filteredList = filtered.ToList();
        TotalCustomers = filteredList.Count;

        Customers = filteredList
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetCustomerDrawerAsync(Guid id, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };

        _apiClient.SetToken(token);
        var customer = await _customerService.GetByIdAsync(id, ct);
        if (customer is null)
            return new JsonResult(new { success = false, message = "Customer not found" }) { StatusCode = 404 };

        // Fetch recent invoices
        var invoices = await _apiClient.GetAsync<List<CustomerInvoiceSummaryDto>>($"/api/customers/{id}/invoices?take=10", ct)
                       ?? new List<CustomerInvoiceSummaryDto>();

        // Fetch loyalty transactions
        var transactions = await _apiClient.GetAsync<List<CustomerLoyaltyTxnDto>>($"/api/customers/{id}/loyalty-transactions?take=10", ct)
                           ?? new List<CustomerLoyaltyTxnDto>();

        return new JsonResult(new
        {
            success = true,
            customer,
            invoices,
            transactions
        });
    }

    public async Task<IActionResult> OnPostAdjustLoyaltyAsync(Guid customerId, int points, string? note, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };

        _apiClient.SetToken(token);
        try
        {
            var req = new AdjustPointsRequest
            {
                CustomerId = customerId,
                Points = points,
                Note = note ?? "Manual Adjustment from Customer 360"
            };
            var result = await _apiClient.PostAsync<LoyaltyTransactionDto>("/api/loyalty/adjust", req, ct);
            return new JsonResult(new { success = true, transaction = result });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 400 };
        }
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);

        string? thumbUrl = null;
        string? fullUrl = null;
        if (ImageUpload != null && ImageUpload.Length > 0)
        {
            using var stream = ImageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "customers", CropX, CropY, CropW, CropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var req = new CreateCustomerRequest
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Gender = Gender,
            Segment = Segment,
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl
        };

        var created = await _customerService.CreateAsync(req, ct);
        StatusMessage = $"Customer {created.FullName} registered successfully.";
        return RedirectToPage(new { customerId = created.CustomerId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid customerId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        await _customerService.DeleteAsync(customerId, ct);
        StatusMessage = "Customer profile removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        string? thumbUrl = null;
        string? fullUrl = null;
        if (ImageUpload != null && ImageUpload.Length > 0)
        {
            var existingCustomer = await _customerService.GetByIdAsync(EditCustomerId, ct);
            if (existingCustomer != null)
            {
                if (!string.IsNullOrWhiteSpace(existingCustomer.ThumbnailUrl))
                    await _fileService.DeleteFileAsync(existingCustomer.ThumbnailUrl, ct);
                if (!string.IsNullOrWhiteSpace(existingCustomer.FullImageUrl))
                    await _fileService.DeleteFileAsync(existingCustomer.FullImageUrl, ct);
            }

            using var stream = ImageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "customers", CropX, CropY, CropW, CropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var req = new UpdateCustomerRequest
        {
            FirstName = EditFirstName.Trim(),
            LastName = EditLastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(EditMiddleName) ? null : EditMiddleName.Trim(),
            Gender = EditGender,
            Segment = EditSegment,
            Phone = string.IsNullOrWhiteSpace(EditPhone) ? null : EditPhone.Trim(),
            Email = string.IsNullOrWhiteSpace(EditEmail) ? null : EditEmail.Trim(),
            Notes = string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim(),
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl
        };

        var updated = await _customerService.UpdateAsync(EditCustomerId, req, ct);
        StatusMessage = $"Customer {updated?.FullName ?? EditFirstName} updated.";
        return RedirectToPage(new { customerId = EditCustomerId });
    }
}

public class CustomerInvoiceSummaryDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class CustomerLoyaltyTxnDto
{
    public Guid LoyaltyTransactionId { get; set; }
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? Note { get; set; }
    public DateTime DateCreated { get; set; }
}
