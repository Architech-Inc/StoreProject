using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category> CreateAsync(string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default);
    Task<Category?> UpdateAsync(int id, string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default);
    Task<bool> RestoreAsync(int id, CancellationToken ct = default);
}
