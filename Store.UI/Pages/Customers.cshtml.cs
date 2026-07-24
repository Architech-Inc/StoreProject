using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
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
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalCustomers / PageSize);

    [BindProperty] public string FirstName { get; set; } = string.Empty;
    [BindProperty] public string LastName { get; set; } = string.Empty;
    [BindProperty] public string? MiddleName { get; set; }
    [BindProperty] public Gender Gender { get; set; } = Gender.NotSpecified;
    [BindProperty] public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
    [BindProperty] public string? Phone { get; set; }
    [BindProperty] public string? Email { get; set; }
    [BindProperty] public string? Notes { get; set; }
    [BindProperty] public IFormFile? ImageUpload { get; set; }

    // Edit
    [BindProperty] public Guid EditCustomerId { get; set; }
    [BindProperty] public string EditFirstName { get; set; } = string.Empty;
    [BindProperty] public string EditLastName { get; set; } = string.Empty;
    [BindProperty] public string? EditMiddleName { get; set; }
    [BindProperty] public Gender EditGender { get; set; } = Gender.NotSpecified;
    [BindProperty] public CustomerSegment EditSegment { get; set; } = CustomerSegment.Standard;
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

        var result = await _customerService.GetAllAsync(
            new PagedRequest { Page = PageNumber, PageSize = PageSize }, ct);

        Customers = result.Items.ToList();
        TotalCustomers = result.TotalCount;

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);

        string? imagePath = null;
        if (ImageUpload != null && ImageUpload.Length > 0)
        {
            using var stream = ImageUpload.OpenReadStream();
            imagePath = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "customers", ct);
        }

        var req = new CreateCustomerRequest
        {
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Gender = Gender,
            Segment = Segment,
            Phone = Phone,
            Email = Email,
            Notes = Notes,
            ImagePath = imagePath
        };

        await _customerService.CreateAsync(req, ct);
        StatusMessage = $"Customer {FirstName} {LastName} created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid customerId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        await _customerService.DeleteAsync(customerId, ct);
        StatusMessage = "Customer removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        string? imagePath = null;
        if (ImageUpload != null && ImageUpload.Length > 0)
        {
            var existingCustomer = await _customerService.GetByIdAsync(EditCustomerId, ct);
            if (existingCustomer != null && !string.IsNullOrWhiteSpace(existingCustomer.ImagePath))
            {
                await _fileService.DeleteFileAsync(existingCustomer.ImagePath, ct);
            }

            using var stream = ImageUpload.OpenReadStream();
            imagePath = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "customers", ct);
        }

        var req = new UpdateCustomerRequest
        {
            FirstName = EditFirstName.Trim(),
            LastName = EditLastName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(EditMiddleName) ? null : EditMiddleName.Trim(),
            Gender = EditGender,
            Segment = EditSegment,
            Notes = string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim(),
            ImagePath = imagePath
        };

        await _customerService.UpdateAsync(EditCustomerId, req, ct);
        StatusMessage = $"Customer {EditFirstName} {EditLastName} updated.";
        return RedirectToPage();
    }
}
