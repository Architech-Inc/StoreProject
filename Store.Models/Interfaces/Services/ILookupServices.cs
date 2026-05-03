using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category> CreateAsync(string name, string? description, CancellationToken ct = default);
    Task<Category?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IUnitService
{
    Task<IEnumerable<Unit>> GetAllAsync(CancellationToken ct = default);
    Task<Unit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Unit> CreateAsync(string name, string abbreviation, string? description, CancellationToken ct = default);
    Task<Unit?> UpdateAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IDepartmentService
{
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Department> CreateAsync(string name, string? description, CancellationToken ct = default);
    Task<Department?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISalaryService
{
    Task<IEnumerable<Salary>> GetAllAsync(CancellationToken ct = default);
    Task<Salary?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Salary> CreateAsync(string grade, decimal basicAmount, decimal? allowance, string? description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

public interface ISupplierService
{
    Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Supplier> CreateAsync(string name, string? registrationNumber, string? notes = null, CancellationToken ct = default);
    Task<Supplier?> UpdateAsync(Guid id, string name, string? registrationNumber, string? notes = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
