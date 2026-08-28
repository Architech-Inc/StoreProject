using Microsoft.AspNetCore.Http;
using Store.Models.Entities;

namespace StoreUI.Services;

public interface ILookupManager
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken ct = default);
    Task<List<Unit>> GetUnitsAsync(CancellationToken ct = default);
    Task<List<Department>> GetDepartmentsAsync(CancellationToken ct = default);

    Task SaveCategoryAsync(int id, string name, string? description, IFormFile? image, int? cropX, int? cropY, int? cropW, int? cropH, CancellationToken ct = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken ct = default);

    Task SaveUnitAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default);
    Task<bool> DeleteUnitAsync(int id, CancellationToken ct = default);

    Task SaveDepartmentAsync(int id, string name, string? description, CancellationToken ct = default);
    Task<bool> DeleteDepartmentAsync(int id, CancellationToken ct = default);
}
