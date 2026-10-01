using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface IUnitService
{
    Task<IEnumerable<Unit>> GetAllAsync(CancellationToken ct = default);
    Task<Unit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Unit> CreateAsync(string name, string abbreviation, string? description, CancellationToken ct = default);
    Task<Unit?> UpdateAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
