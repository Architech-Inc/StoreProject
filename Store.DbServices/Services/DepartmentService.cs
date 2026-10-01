using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;

    public DepartmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken ct = default) =>
        await _uow.Repository<Department>().Query().AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);

    public async Task<Department?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _uow.Repository<Department>().GetByIdAsync(id, ct);

    public async Task<Department> CreateAsync(string name, string? description, CancellationToken ct = default)
    {
        var trimmedName = name.Trim();
        if (await _uow.Repository<Department>().ExistsAsync(d => d.Name == trimmedName, ct))
            throw new InvalidOperationException($"Department '{name}' already exists.");

        var dept = new Department
        {
            Name = trimmedName,
            Description = description?.Trim()
        };

        await _uow.Repository<Department>().AddAsync(dept, ct);
        await _uow.SaveChangesAsync(ct);
        return dept;
    }

    public async Task<Department?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        var dept = await _uow.Repository<Department>().GetByIdAsync(id, ct);
        if (dept is null) return null;

        var trimmedName = name.Trim();
        if (await _uow.Repository<Department>().ExistsAsync(d => d.Name == trimmedName && d.DepartmentId != id, ct))
            throw new InvalidOperationException($"Department '{name}' already exists.");

        dept.Name = trimmedName;
        dept.Description = description?.Trim();

        _uow.Repository<Department>().Update(dept);
        await _uow.SaveChangesAsync(ct);
        return dept;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var dept = await _uow.Repository<Department>().GetByIdAsync(id, ct);
        if (dept is null) return false;

        var hasEmployees = await _uow.Repository<Employee>().ExistsAsync(e => e.DepartmentId == id, ct);
        if (hasEmployees)
            throw new InvalidOperationException("Cannot delete department because it is assigned to one or more employees.");

        _uow.Repository<Department>().Remove(dept);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
