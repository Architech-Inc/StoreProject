using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using StoreUI.Services;
using Store.Models.Common;

using Microsoft.Extensions.Logging.Abstractions;
namespace StoreUI.Pages;

public class LookupModel : SecurePageModel
{
    private readonly ILookupManager _lookupManager;
    private readonly IApiClientService _apiClient;

    public string ActiveTab { get; private set; } = "categories";

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public IReadOnlyList<Unit> Units { get; private set; } = Array.Empty<Unit>();
    public IReadOnlyList<Department> Departments { get; private set; } = Array.Empty<Department>();
    public IReadOnlyList<Salary> Salaries { get; private set; } = Array.Empty<Salary>();

    public int TotalCategories => Categories.Count;
    public int TotalUnits => Units.Count;
    public int TotalDepartments => Departments.Count;
    public int TotalSalaries => Salaries.Count;

    // UX-05 — delete actions on lookup tables are admin actions (categories,
    // units, departments, salary grades all affect every branch).
    public bool CanAdmin { get; private set; }

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
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        CanAdmin = HasPermission(permissions, PermissionKeys.AdminUsers);
        _apiClient.SetToken(token);

        ActiveTab = tab is "categories" or "units" or "departments" or "salaries" ? tab : "categories";

        var catTask = _lookupManager.GetCategoriesAsync(ct);
        var unitTask = _lookupManager.GetUnitsAsync(ct);
        var deptTask = _lookupManager.GetDepartmentsAsync(ct);
        var salaryTask = _lookupManager.GetSalariesAsync(ct);

        await Task.WhenAll(catTask, unitTask, deptTask, salaryTask);

        Categories = await catTask;
        Units = await unitTask;
        Departments = await deptTask;
        Salaries = await salaryTask;

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
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "categories" });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        // UX-05 — server-side gate (categories affect every branch).
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: deleting a category requires administrator privileges.";
            return RedirectToPage();
        }
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteCategoryAsync(id, ct);
            StatusMessage = ok ? "Category deleted successfully." : "Error: Could not delete category.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
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
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "units" });
    }

    public async Task<IActionResult> OnPostDeleteUnitAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: deleting a measurement unit requires administrator privileges.";
            return RedirectToPage();
        }
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteUnitAsync(id, ct);
            StatusMessage = ok ? "Unit deleted successfully." : "Error: Could not delete unit.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
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
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "departments" });
    }

    public async Task<IActionResult> OnPostDeleteDepartmentAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: deleting a department requires administrator privileges.";
            return RedirectToPage();
        }
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteDepartmentAsync(id, ct);
            StatusMessage = ok ? "Department deleted successfully." : "Error: Could not delete department.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "departments" });
    }

    // ── Salary Grades ────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveSalaryAsync(int id, string grade, decimal basicAmount, decimal? allowanceAmount, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            await _lookupManager.SaveSalaryAsync(id, grade, basicAmount, allowanceAmount, description, ct);
            StatusMessage = id == 0 ? $"Salary grade '{grade}' created successfully." : $"Salary grade '{grade}' updated successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "salaries" });
    }

    public async Task<IActionResult> OnPostDeleteSalaryAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: deleting a salary grade requires administrator privileges.";
            return RedirectToPage();
        }
        _apiClient.SetToken(token);

        try
        {
            var ok = await _lookupManager.DeleteSalaryAsync(id, ct);
            StatusMessage = ok ? "Salary grade deleted successfully." : "Error: Could not delete salary grade.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {SafeErrorMessage.From(ex, NullLogger<LookupModel>.Instance, "Lookup operation")}";
        }

        return RedirectToPage("/Lookup", new { tab = "salaries" });
    }
}
