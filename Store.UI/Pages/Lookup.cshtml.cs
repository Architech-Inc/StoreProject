using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities;
using StoreUI.Services;

namespace StoreUI.Pages;

public class LookupModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public string ActiveTab { get; private set; } = "categories";

    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public IReadOnlyList<Unit> Units { get; private set; } = Array.Empty<Unit>();
    public IReadOnlyList<Department> Departments { get; private set; } = Array.Empty<Department>();

    [TempData] public string? StatusMessage { get; set; }

    public LookupModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(string tab = "categories", CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        ActiveTab = tab is "categories" or "units" or "departments" ? tab : "categories";

        switch (ActiveTab)
        {
            case "categories":
                Categories = (await _apiClient.GetAsync<List<Category>>("/api/categories", ct)) ?? new();
                break;
            case "units":
                Units = (await _apiClient.GetAsync<List<Unit>>("/api/units", ct)) ?? new();
                break;
            case "departments":
                Departments = (await _apiClient.GetAsync<List<Department>>("/api/departments", ct)) ?? new();
                break;
        }

        return Page();
    }

    // ── Categories ───────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveCategoryAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            if (id == 0)
                await _apiClient.PostAsync<Category>("/api/categories", new { name, description }, ct);
            else
                await _apiClient.PutAsync<Category>($"/api/categories/{id}", new { name, description }, ct);

            StatusMessage = id == 0 ? $"Category '{name}' created." : $"Category '{name}' updated.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }

        return RedirectToPage(new { tab = "categories" });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            var ok = await _apiClient.DeleteAsync($"/api/categories/{id}", ct);
            StatusMessage = ok ? "Category deleted." : "Error: Could not delete category.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        return RedirectToPage(new { tab = "categories" });
    }

    // ── Units ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveUnitAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            if (id == 0)
                await _apiClient.PostAsync<Unit>("/api/units", new { name, abbreviation, description }, ct);
            else
                await _apiClient.PutAsync<Unit>($"/api/units/{id}", new { name, abbreviation, description }, ct);

            StatusMessage = id == 0 ? $"Unit '{name}' created." : $"Unit '{name}' updated.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        return RedirectToPage(new { tab = "units" });
    }

    public async Task<IActionResult> OnPostDeleteUnitAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            var ok = await _apiClient.DeleteAsync($"/api/units/{id}", ct);
            StatusMessage = ok ? "Unit deleted." : "Error: Could not delete unit.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        return RedirectToPage(new { tab = "units" });
    }

    // ── Departments ──────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveDepartmentAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            if (id == 0)
                await _apiClient.PostAsync<Department>("/api/departments", new { name, description }, ct);
            else
                await _apiClient.PutAsync<Department>($"/api/departments/{id}", new { name, description }, ct);

            StatusMessage = id == 0 ? $"Department '{name}' created." : $"Department '{name}' updated.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        return RedirectToPage(new { tab = "departments" });
    }

    public async Task<IActionResult> OnPostDeleteDepartmentAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();
        try
        {
            var ok = await _apiClient.DeleteAsync($"/api/departments/{id}", ct);
            StatusMessage = ok ? "Department deleted." : "Error: Could not delete department.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        return RedirectToPage(new { tab = "departments" });
    }
}
