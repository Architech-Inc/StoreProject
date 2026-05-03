using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Suppliers;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class SuppliersModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<SupplierDto> Suppliers { get; private set; } = Array.Empty<SupplierDto>();

    // Create form
    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public string? RegistrationNumber { get; set; }
    [BindProperty] public string? Notes { get; set; }

    // Edit form
    [BindProperty] public Guid EditSupplierId { get; set; }
    [BindProperty] public string EditName { get; set; } = string.Empty;
    [BindProperty] public string? EditRegistrationNumber { get; set; }
    [BindProperty] public string? EditNotes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public SuppliersModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        await LoadDataAsync(ct);
        return Page();
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var result = await _apiClient.GetAsync<IEnumerable<SupplierDto>>("api/suppliers", ct);
        Suppliers = result?.ToList() ?? new List<SupplierDto>();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Error: Supplier name is required.";
            _apiClient.SetToken(token);
            await LoadDataAsync(ct);
            return Page();
        }

        _apiClient.SetToken(token);
        var request = new CreateSupplierRequest
        {
            Name = Name,
            RegistrationNumber = RegistrationNumber,
            Notes = Notes
        };

        var ok = await _apiClient.PostAsync("api/suppliers", request, ct);
        StatusMessage = ok ? "Supplier created successfully." : "Error: Failed to create supplier.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Error: Supplier name is required.";
            _apiClient.SetToken(token);
            await LoadDataAsync(ct);
            return Page();
        }

        _apiClient.SetToken(token);
        var request = new UpdateSupplierRequest
        {
            Name = EditName,
            RegistrationNumber = EditRegistrationNumber,
            Notes = EditNotes
        };

        var updated = await _apiClient.PutAsync<SupplierDto>($"api/suppliers/{EditSupplierId}", request, ct);
        StatusMessage = updated is not null ? "Supplier updated." : "Error: Update failed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        var ok = await _apiClient.DeleteAsync($"api/suppliers/{id}", ct);
        StatusMessage = ok ? "Supplier deleted." : "Error: Delete failed.";
        return RedirectToPage();
    }
}
