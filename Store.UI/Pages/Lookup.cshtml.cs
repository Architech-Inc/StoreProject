using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities;
using StoreUI.Services;

namespace StoreUI.Pages;

public class LookupModel : SecurePageModel
{
    private readonly ILookupManager _lookupManager;
    private readonly IApiClientService _apiClient;

    public string ActiveTab { get; private set; } = "categories";

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public IReadOnlyList<Unit> Units { get; private set; } = Array.Empty<Unit>();
    public IReadOnlyList<Department> Departments { get; private set; } = Array.Empty<Department>();

    public int TotalCategories => Categories.Count;
    public int TotalUnits => Units.Count;
    public int TotalDepartments => Departments.Count;

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public IFormFile? CategoryImageUpload { get; set; }
    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    public LookupModel(ILookupManager lookupManager, IApiClientService apiClient)
    {
        _lookupManager = lookupManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(string tab = "categories", CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        ActiveTab = tab is "categories" or "units" or "departments" ? tab : "categories";

        var catTask = _lookupManager.GetCategoriesAsync(ct);
        var unitTask = _lookupManager.GetUnitsAsync(ct);
        var deptTask = _lookupManager.GetDepartmentsAsync(ct);

        await Task.WhenAll(catTask, unitTask, deptTask);

        Categories = await catTask;
        Units = await unitTask;
        Departments = await deptTask;

        return Page();
    }

    // ── Categories ───────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveCategoryAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            await _lookupManager.SaveCategoryAsync(id, name, description, CategoryImageUpload, CropX, CropY, CropW, CropH, ct);
            StatusMessage = id == 0 ? $"Category '{name}' created successfully." : $"Category '{name}' updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "categories" });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteCategoryAsync(id, ct);
            StatusMessage = ok ? "Category deleted successfully." : "Error: Could not delete category.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "categories" });
    }

    // ── Units ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveUnitAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            await _lookupManager.SaveUnitAsync(id, name, abbreviation, description, ct);
            StatusMessage = id == 0 ? $"Unit '{name}' created successfully." : $"Unit '{name}' updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "units" });
    }

    public async Task<IActionResult> OnPostDeleteUnitAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteUnitAsync(id, ct);
            StatusMessage = ok ? "Unit deleted successfully." : "Error: Could not delete unit.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "units" });
    }

    // ── Departments ──────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveDepartmentAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            await _lookupManager.SaveDepartmentAsync(id, name, description, ct);
            StatusMessage = id == 0 ? $"Department '{name}' created successfully." : $"Department '{name}' updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "departments" });
    }

    public async Task<IActionResult> OnPostDeleteDepartmentAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteDepartmentAsync(id, ct);
            StatusMessage = ok ? "Department deleted successfully." : "Error: Could not delete department.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage("/Lookup", new { tab = "departments" });
    }
}
