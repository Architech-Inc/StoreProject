using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class SuppliersModel : SecurePageModel
{
    private readonly ISupplierService _supplierService;
    private readonly IApiClientService _apiClient;
    private readonly IFileService _fileService;

    public List<SupplierDto> Suppliers { get; private set; } = new();
    public SupplierMetricsDto Metrics { get; private set; } = new();
    public List<string> AvailableCities { get; private set; } = new();
    public List<string> AvailableCountries { get; private set; } = new();

    // Query & Filtering
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? CityFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? CountryFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "name_asc";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "grid";
    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? SupplierId { get; set; }

    // ---- Create Supplier ----
    [BindProperty] public string CreateName { get; set; } = string.Empty;
    [BindProperty] public string? CreateRegistrationNumber { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }
    [BindProperty] public IFormFile? CreateImageUpload { get; set; }
    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    // Contacts
    [BindProperty] public List<string> CreateEmails { get; set; } = new();
    [BindProperty] public List<EmailType> CreateEmailTypes { get; set; } = new();
    [BindProperty] public List<bool> CreateEmailPrimaries { get; set; } = new();

    [BindProperty] public List<string> CreatePhones { get; set; } = new();
    [BindProperty] public List<PhoneType> CreatePhoneTypes { get; set; } = new();
    [BindProperty] public List<bool> CreatePhonePrimaries { get; set; } = new();

    [BindProperty] public List<string> CreateAddressLines1 { get; set; } = new();
    [BindProperty] public List<string?> CreateAddressLines2 { get; set; } = new();
    [BindProperty] public List<string> CreateCities { get; set; } = new();
    [BindProperty] public List<string?> CreateStates { get; set; } = new();
    [BindProperty] public List<string?> CreatePostalCodes { get; set; } = new();
    [BindProperty] public List<string> CreateCountries { get; set; } = new();
    [BindProperty] public List<bool> CreateLocationPrimaries { get; set; } = new();

    // ---- Edit Supplier ----
    [BindProperty] public Guid EditSupplierId { get; set; }
    [BindProperty] public string EditName { get; set; } = string.Empty;
    [BindProperty] public string? EditRegistrationNumber { get; set; }
    [BindProperty] public string? EditNotes { get; set; }
    [BindProperty] public IFormFile? EditImageUpload { get; set; }
    [BindProperty] public int? EditCropX { get; set; }
    [BindProperty] public int? EditCropY { get; set; }
    [BindProperty] public int? EditCropW { get; set; }
    [BindProperty] public int? EditCropH { get; set; }

    [BindProperty] public List<string> EditEmails { get; set; } = new();
    [BindProperty] public List<EmailType> EditEmailTypes { get; set; } = new();
    [BindProperty] public List<bool> EditEmailPrimaries { get; set; } = new();

    [BindProperty] public List<string> EditPhones { get; set; } = new();
    [BindProperty] public List<PhoneType> EditPhoneTypes { get; set; } = new();
    [BindProperty] public List<bool> EditPhonePrimaries { get; set; } = new();

    [BindProperty] public List<string> EditAddressLines1 { get; set; } = new();
    [BindProperty] public List<string?> EditAddressLines2 { get; set; } = new();
    [BindProperty] public List<string> EditCities { get; set; } = new();
    [BindProperty] public List<string?> EditStates { get; set; } = new();
    [BindProperty] public List<string?> EditPostalCodes { get; set; } = new();
    [BindProperty] public List<string> EditCountries { get; set; } = new();
    [BindProperty] public List<bool> EditLocationPrimaries { get; set; } = new();

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<EmailType> EmailTypes { get; } = Enum.GetValues<EmailType>();
    public IEnumerable<PhoneType> PhoneTypes { get; } = Enum.GetValues<PhoneType>();

    private readonly ILogger<SuppliersModel> _logger;

    public SuppliersModel(
        ISupplierService supplierService,
        IApiClientService apiClient,
        IFileService fileService,
        ILogger<SuppliersModel> logger)
    {
        _supplierService = supplierService;
        _apiClient = apiClient;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        // Normalize ID deep-linking
        if (SupplierId.HasValue && !Id.HasValue)
            Id = SupplierId;

        // Load KPI Metrics
        try
        {
            Metrics = await _supplierService.GetMetricsAsync();
        }
        catch
        {
            Metrics = new SupplierMetricsDto();
        }

        // Load filtered suppliers
        Suppliers = await _supplierService.GetAllAsync(Search, CityFilter, CountryFilter, SortBy);

        // Load available cities & countries for filtering
        var allSuppliers = await _supplierService.GetAllAsync();
        AvailableCities = allSuppliers
            .SelectMany(s => s.Locations)
            .Where(l => !string.IsNullOrWhiteSpace(l.City))
            .Select(l => l.City.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        AvailableCountries = allSuppliers
            .SelectMany(s => s.Locations)
            .Where(l => !string.IsNullOrWhiteSpace(l.Country))
            .Select(l => l.Country.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();

        ViewData["ActivePage"] = "Suppliers";
        return Page();
    }

    public async Task<IActionResult> OnGetProfileAsync(Guid id)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };

        _apiClient.SetToken(token);
        var profile = await _supplierService.GetProfileAsync(id);
        if (profile is null)
            return new JsonResult(new { success = false, message = "Supplier not found" }) { StatusCode = 404 };

        return new JsonResult(new { success = true, profile });
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        string? thumbUrl = null;
        string? fullUrl = null;
        if (CreateImageUpload != null && CreateImageUpload.Length > 0)
        {
            using var stream = CreateImageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, CreateImageUpload.FileName, CreateImageUpload.ContentType, "suppliers", CropX, CropY, CropW, CropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var emails = new List<CreateSupplierEmailRequest>();
        for (int i = 0; i < CreateEmails.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(CreateEmails[i]))
            {
                emails.Add(new CreateSupplierEmailRequest
                {
                    Email = CreateEmails[i].Trim(),
                    EmailType = CreateEmailTypes.ElementAtOrDefault(i),
                    IsPrimary = CreateEmailPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var phones = new List<CreateSupplierPhoneRequest>();
        for (int i = 0; i < CreatePhones.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(CreatePhones[i]))
            {
                phones.Add(new CreateSupplierPhoneRequest
                {
                    PhoneNumber = CreatePhones[i].Trim(),
                    PhoneType = CreatePhoneTypes.ElementAtOrDefault(i),
                    IsPrimary = CreatePhonePrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var locations = new List<CreateSupplierLocationRequest>();
        for (int i = 0; i < CreateAddressLines1.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(CreateAddressLines1[i]))
            {
                locations.Add(new CreateSupplierLocationRequest
                {
                    AddressLine1 = CreateAddressLines1[i].Trim(),
                    AddressLine2 = string.IsNullOrWhiteSpace(CreateAddressLines2.ElementAtOrDefault(i)) ? null : CreateAddressLines2[i]?.Trim(),
                    City = CreateCities.ElementAtOrDefault(i)?.Trim() ?? string.Empty,
                    State = string.IsNullOrWhiteSpace(CreateStates.ElementAtOrDefault(i)) ? null : CreateStates[i]?.Trim(),
                    PostalCode = string.IsNullOrWhiteSpace(CreatePostalCodes.ElementAtOrDefault(i)) ? null : CreatePostalCodes[i]?.Trim(),
                    Country = CreateCountries.ElementAtOrDefault(i)?.Trim() ?? string.Empty,
                    IsPrimary = CreateLocationPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var request = new CreateSupplierRequest
        {
            Name = CreateName.Trim(),
            RegistrationNumber = string.IsNullOrWhiteSpace(CreateRegistrationNumber) ? null : CreateRegistrationNumber.Trim(),
            Notes = string.IsNullOrWhiteSpace(CreateNotes) ? null : CreateNotes.Trim(),
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl,
            Emails = emails,
            Phones = phones,
            Locations = locations
        };

        try
        {
            var created = await _supplierService.CreateAsync(request, Guid.Empty);
            StatusMessage = $"Supplier '{created.Name}' registered successfully.";
            return RedirectToPage(new { id = created.SupplierId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create supplier with name '{SupplierName}'", CreateName);
            StatusMessage = $"Error: Failed to create supplier ({ex.Message}).";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        string? thumbUrl = null;
        string? fullUrl = null;
        if (EditImageUpload != null && EditImageUpload.Length > 0)
        {
            using var stream = EditImageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, EditImageUpload.FileName, EditImageUpload.ContentType, "suppliers", EditCropX, EditCropY, EditCropW, EditCropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var emails = new List<CreateSupplierEmailRequest>();
        for (int i = 0; i < EditEmails.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(EditEmails[i]))
            {
                emails.Add(new CreateSupplierEmailRequest
                {
                    Email = EditEmails[i].Trim(),
                    EmailType = EditEmailTypes.ElementAtOrDefault(i),
                    IsPrimary = EditEmailPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var phones = new List<CreateSupplierPhoneRequest>();
        for (int i = 0; i < EditPhones.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(EditPhones[i]))
            {
                phones.Add(new CreateSupplierPhoneRequest
                {
                    PhoneNumber = EditPhones[i].Trim(),
                    PhoneType = EditPhoneTypes.ElementAtOrDefault(i),
                    IsPrimary = EditPhonePrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var locations = new List<CreateSupplierLocationRequest>();
        for (int i = 0; i < EditAddressLines1.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(EditAddressLines1[i]))
            {
                locations.Add(new CreateSupplierLocationRequest
                {
                    AddressLine1 = EditAddressLines1[i].Trim(),
                    AddressLine2 = string.IsNullOrWhiteSpace(EditAddressLines2.ElementAtOrDefault(i)) ? null : EditAddressLines2[i]?.Trim(),
                    City = EditCities.ElementAtOrDefault(i)?.Trim() ?? string.Empty,
                    State = string.IsNullOrWhiteSpace(EditStates.ElementAtOrDefault(i)) ? null : EditStates[i]?.Trim(),
                    PostalCode = string.IsNullOrWhiteSpace(EditPostalCodes.ElementAtOrDefault(i)) ? null : EditPostalCodes[i]?.Trim(),
                    Country = EditCountries.ElementAtOrDefault(i)?.Trim() ?? string.Empty,
                    IsPrimary = EditLocationPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var request = new UpdateSupplierRequest
        {
            Name = EditName.Trim(),
            RegistrationNumber = string.IsNullOrWhiteSpace(EditRegistrationNumber) ? null : EditRegistrationNumber.Trim(),
            Notes = string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim(),
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl,
            Emails = emails,
            Phones = phones,
            Locations = locations
        };

        var result = await _supplierService.UpdateAsync(EditSupplierId, request);
        StatusMessage = result is not null
            ? $"Supplier '{result.Name}' updated successfully."
            : "Error: Supplier not found.";

        return RedirectToPage(new { id = EditSupplierId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid supplierId)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var success = await _supplierService.DeleteAsync(supplierId);
        StatusMessage = success
            ? "Supplier deleted successfully."
            : "Error: Could not delete supplier (supplier has associated purchase orders or item orders).";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        var list = await _supplierService.GetAllAsync(Search, CityFilter, CountryFilter, SortBy);

        var sb = new StringBuilder();
        sb.AppendLine("Supplier ID,Name,Registration Number,Primary Phone,Primary Email,Primary Location,Date Created");

        foreach (var s in list)
        {
            var primaryPhone = s.Phones.FirstOrDefault(p => p.IsPrimary)?.PhoneNumber ?? s.Phones.FirstOrDefault()?.PhoneNumber ?? "";
            var primaryEmail = s.Emails.FirstOrDefault(e => e.IsPrimary)?.Email ?? s.Emails.FirstOrDefault()?.Email ?? "";
            var primaryLoc = s.Locations.FirstOrDefault(l => l.IsPrimary) ?? s.Locations.FirstOrDefault();
            var locStr = primaryLoc != null ? $"{primaryLoc.AddressLine1}, {primaryLoc.City}, {primaryLoc.Country}" : "";

            sb.AppendLine($"\"{s.SupplierId}\",\"{EscapeCsv(s.Name)}\",\"{EscapeCsv(s.RegistrationNumber ?? "")}\",\"{EscapeCsv(primaryPhone)}\",\"{EscapeCsv(primaryEmail)}\",\"{EscapeCsv(locStr)}\",\"{s.DateCreated:yyyy-MM-dd HH:mm:ss}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"suppliers_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private static string EscapeCsv(string val) => val.Replace("\"", "\"\"");
}
