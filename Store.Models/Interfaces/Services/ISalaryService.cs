using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface ISalaryService
{
    Task<IEnumerable<Salary>> GetAllAsync(CancellationToken ct = default);
    Task<Salary?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Salary> CreateAsync(string grade, decimal basicAmount, decimal? allowance, string? description, CancellationToken ct = default);
    Task<Salary?> UpdateAsync(int id, string grade, decimal basicAmount, decimal? allowance, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default);
    Task<bool> RestoreAsync(int id, CancellationToken ct = default);
}
