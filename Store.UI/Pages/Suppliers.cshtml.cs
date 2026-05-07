using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Procurement;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class SuppliersModel : SecurePageModel
{
    private readonly ISupplierService _supplierService;
    private readonly IApiClientService _apiClient;

    public List<SupplierDto> Suppliers { get; private set; } = new();

    // ---- Create Supplier ----
    [BindProperty] public Guid CreateSupplierId { get; set; }
    [BindProperty] public string CreateName { get; set; } = string.Empty;
    [BindProperty] public string? CreateRegistrationNumber { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }

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

    public SuppliersModel(ISupplierService supplierService, IApiClientService apiClient)
    {
        _supplierService = supplierService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        Suppliers = await _supplierService.GetAllAsync();
        ViewData["ActivePage"] = "Suppliers";
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var emails = new List<CreateSupplierEmailRequest>();
        for (int i = 0; i < CreateEmails.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(CreateEmails[i]))
            {
                emails.Add(new CreateSupplierEmailRequest
                {
                    Email = CreateEmails[i],
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
                    PhoneNumber = CreatePhones[i],
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
                    AddressLine1 = CreateAddressLines1[i],
                    AddressLine2 = CreateAddressLines2.ElementAtOrDefault(i),
                    City = CreateCities.ElementAtOrDefault(i) ?? string.Empty,
                    State = CreateStates.ElementAtOrDefault(i),
                    PostalCode = CreatePostalCodes.ElementAtOrDefault(i),
                    Country = CreateCountries.ElementAtOrDefault(i) ?? string.Empty,
                    IsPrimary = CreateLocationPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var request = new CreateSupplierRequest
        {
            Name = CreateName,
            RegistrationNumber = CreateRegistrationNumber,
            Notes = CreateNotes,
            Emails = emails,
            Phones = phones,
            Locations = locations
        };

        try
        {
            await _supplierService.CreateAsync(request, Guid.Empty);
            StatusMessage = "Supplier created successfully.";
        }
        catch
        {
            StatusMessage = "Error: Failed to create supplier.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var emails = new List<CreateSupplierEmailRequest>();
        for (int i = 0; i < EditEmails.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(EditEmails[i]))
            {
                emails.Add(new CreateSupplierEmailRequest
                {
                    Email = EditEmails[i],
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
                    PhoneNumber = EditPhones[i],
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
                    AddressLine1 = EditAddressLines1[i],
                    AddressLine2 = EditAddressLines2.ElementAtOrDefault(i),
                    City = EditCities.ElementAtOrDefault(i) ?? string.Empty,
                    State = EditStates.ElementAtOrDefault(i),
                    PostalCode = EditPostalCodes.ElementAtOrDefault(i),
                    Country = EditCountries.ElementAtOrDefault(i) ?? string.Empty,
                    IsPrimary = EditLocationPrimaries.ElementAtOrDefault(i)
                });
            }
        }

        var request = new UpdateSupplierRequest
        {
            Name = EditName,
            RegistrationNumber = EditRegistrationNumber,
            Notes = EditNotes,
            Emails = emails,
            Phones = phones,
            Locations = locations
        };

        var result = await _supplierService.UpdateAsync(EditSupplierId, request);
        StatusMessage = result is not null
            ? "Supplier updated successfully."
            : "Error: Supplier not found.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid supplierId)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var success = await _supplierService.DeleteAsync(supplierId);
        StatusMessage = success
            ? "Supplier deleted successfully."
            : "Error: Could not delete supplier (may have associated orders).";

        return RedirectToPage();
    }
}
