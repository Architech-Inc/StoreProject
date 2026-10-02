using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Department> CreateAsync(string name, string? description, CancellationToken ct = default);
    Task<Department?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default);
    Task<bool> RestoreAsync(int id, CancellationToken ct = default);
}
