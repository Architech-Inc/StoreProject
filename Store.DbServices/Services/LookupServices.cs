using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    public CategoryService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _uow.Repository<Category>().Query().AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _uow.Repository<Category>().GetByIdAsync(id, ct);

    public async Task<Category> CreateAsync(string name, string? description, CancellationToken ct = default)
    {
        if (await _uow.Repository<Category>().ExistsAsync(c => c.Name == name.Trim(), ct))
            throw new InvalidOperationException($"Category '{name}' already exists.");

        var category = new Category { Name = name.Trim(), Description = description?.Trim() };
        await _uow.Repository<Category>().AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        var category = await _uow.Repository<Category>().GetByIdAsync(id, ct);
        if (category is null) return null;

        category.Name = name.Trim();
        category.Description = description?.Trim();
        _uow.Repository<Category>().Update(category);
        await _uow.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _uow.Repository<Category>().GetByIdAsync(id, ct);
        if (category is null) return false;
        _uow.Repository<Category>().Remove(category);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

public class UnitService : IUnitService
{
    private readonly IUnitOfWork _uow;
    public UnitService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Unit>> GetAllAsync(CancellationToken ct = default) =>
        await _uow.Repository<Unit>().Query().AsNoTracking().OrderBy(u => u.Name).ToListAsync(ct);

    public async Task<Unit?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _uow.Repository<Unit>().GetByIdAsync(id, ct);

    public async Task<Unit> CreateAsync(string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        if (await _uow.Repository<Unit>().ExistsAsync(u => u.Abbreviation == abbreviation.Trim(), ct))
            throw new InvalidOperationException($"Unit abbreviation '{abbreviation}' already exists.");

        var unit = new Unit { Name = name.Trim(), Abbreviation = abbreviation.Trim(), Description = description?.Trim() };
        await _uow.Repository<Unit>().AddAsync(unit, ct);
        await _uow.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<Unit?> UpdateAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        var unit = await _uow.Repository<Unit>().GetByIdAsync(id, ct);
        if (unit is null) return null;
        unit.Name = name.Trim();
        unit.Abbreviation = abbreviation.Trim();
        unit.Description = description?.Trim();
        _uow.Repository<Unit>().Update(unit);
        await _uow.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var unit = await _uow.Repository<Unit>().GetByIdAsync(id, ct);
        if (unit is null) return false;
        _uow.Repository<Unit>().Remove(unit);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

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
        if (await _uow.Repository<Department>().ExistsAsync(d => d.Name == name.Trim(), ct))
            throw new InvalidOperationException($"Department '{name}' already exists.");

        var dept = new Department { Name = name.Trim(), Description = description?.Trim() };
        await _uow.Repository<Department>().AddAsync(dept, ct);
        await _uow.SaveChangesAsync(ct);
        return dept;
    }

    public async Task<Department?> UpdateAsync(int id, string name, string? description, CancellationToken ct = default)
    {
        var dept = await _uow.Repository<Department>().GetByIdAsync(id, ct);
        if (dept is null) return null;
        dept.Name = name.Trim();
        dept.Description = description?.Trim();
        _uow.Repository<Department>().Update(dept);
        await _uow.SaveChangesAsync(ct);
        return dept;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var dept = await _uow.Repository<Department>().GetByIdAsync(id, ct);
        if (dept is null) return false;
        _uow.Repository<Department>().Remove(dept);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow;
    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Supplier>> GetAllAsync(CancellationToken ct = default) =>
        await _uow.Repository<Supplier>().Query().AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _uow.Repository<Supplier>().GetByIdAsync(id, ct);

    public async Task<Supplier> CreateAsync(string name, string? registrationNumber, CancellationToken ct = default)
    {
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = name.Trim(),
            RegistrationNumber = registrationNumber?.Trim()
        };
        await _uow.Repository<Supplier>().AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task<Supplier?> UpdateAsync(Guid id, string name, string? registrationNumber, CancellationToken ct = default)
    {
        var supplier = await _uow.Repository<Supplier>().GetByIdAsync(id, ct);
        if (supplier is null) return null;
        supplier.Name = name.Trim();
        supplier.RegistrationNumber = registrationNumber?.Trim();
        _uow.Repository<Supplier>().Update(supplier);
        await _uow.SaveChangesAsync(ct);
        return supplier;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _uow.Repository<Supplier>().GetByIdAsync(id, ct);
        if (supplier is null) return false;
        _uow.Repository<Supplier>().Remove(supplier);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
