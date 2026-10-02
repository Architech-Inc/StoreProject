using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

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
        var trimmedAbbr = abbreviation.Trim();
        if (await _uow.Repository<Unit>().ExistsAsync(u => u.Abbreviation == trimmedAbbr, ct))
            throw new InvalidOperationException($"Unit abbreviation '{abbreviation}' already exists.");

        var unit = new Unit
        {
            Name = name.Trim(),
            Abbreviation = trimmedAbbr,
            Description = description?.Trim()
        };

        await _uow.Repository<Unit>().AddAsync(unit, ct);
        await _uow.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<Unit?> UpdateAsync(int id, string name, string abbreviation, string? description, CancellationToken ct = default)
    {
        var unit = await _uow.Repository<Unit>().GetByIdAsync(id, ct);
        if (unit is null) return null;

        var trimmedAbbr = abbreviation.Trim();
        if (await _uow.Repository<Unit>().ExistsAsync(u => u.Abbreviation == trimmedAbbr && u.UnitId != id, ct))
            throw new InvalidOperationException($"Unit abbreviation '{abbreviation}' already exists.");

        unit.Name = name.Trim();
        unit.Abbreviation = trimmedAbbr;
        unit.Description = description?.Trim();

        _uow.Repository<Unit>().Update(unit);
        await _uow.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default)
    {
        var unit = await _uow.Repository<Unit>().Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.UnitId == id, ct);
        if (unit is null) return false;
        if (unit.IsDeleted) return true; // idempotent

        var hasItems = await _uow.Repository<Item>().ExistsAsync(i => i.UnitId == id, ct);
        if (hasItems)
            throw new InvalidOperationException("Cannot delete unit because it is assigned to one or more items.");

        unit.IsDeleted = true;
        unit.DeletedAt = DateTime.UtcNow;
        unit.DeletedById = deletedById;

        _uow.Repository<Unit>().Update(unit);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken ct = default)
    {
        var unit = await _uow.Repository<Unit>().Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.UnitId == id, ct);
        if (unit is null) return false;
        if (!unit.IsDeleted) return true; // already active

        unit.IsDeleted = false;
        unit.DeletedAt = null;
        unit.DeletedById = null;

        _uow.Repository<Unit>().Update(unit);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
