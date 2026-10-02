using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Interfaces;
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

    public async Task<Category> CreateAsync(string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default)
    {
        var trimmedName = name.Trim();
        if (await _uow.Repository<Category>().ExistsAsync(c => c.Name == trimmedName, ct))
            throw new InvalidOperationException($"Category '{name}' already exists.");

        var category = new Category
        {
            Name = trimmedName,
            Description = description?.Trim(),
            ThumbnailUrl = thumbnailUrl?.Trim(),
            FullImageUrl = fullImageUrl?.Trim()
        };

        await _uow.Repository<Category>().AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, string name, string? description, string? thumbnailUrl = null, string? fullImageUrl = null, CancellationToken ct = default)
    {
        var category = await _uow.Repository<Category>().GetByIdAsync(id, ct);
        if (category is null) return null;

        var trimmedName = name.Trim();
        if (await _uow.Repository<Category>().ExistsAsync(c => c.Name == trimmedName && c.CategoryId != id, ct))
            throw new InvalidOperationException($"Category '{name}' already exists.");

        category.Name = trimmedName;
        category.Description = description?.Trim();
        if (thumbnailUrl != null) category.ThumbnailUrl = thumbnailUrl.Trim();
        if (fullImageUrl != null) category.FullImageUrl = fullImageUrl.Trim();

        _uow.Repository<Category>().Update(category);
        await _uow.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default)
    {
        var category = await _uow.Repository<Category>().Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (category is null) return false;
        if (category.IsDeleted) return true; // idempotent

        var hasItems = await _uow.Repository<Item>().ExistsAsync(i => i.CategoryId == id, ct) ||
                       await _uow.Repository<ItemCategory>().ExistsAsync(ic => ic.CategoryId == id, ct);
        if (hasItems)
            throw new InvalidOperationException("Cannot delete category because it is assigned to one or more items.");

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedById = deletedById;

        _uow.Repository<Category>().Update(category);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RestoreAsync(int id, CancellationToken ct = default)
    {
        var category = await _uow.Repository<Category>().Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
        if (category is null) return false;
        if (!category.IsDeleted) return true; // already active

        category.IsDeleted = false;
        category.DeletedAt = null;
        category.DeletedById = null;

        _uow.Repository<Category>().Update(category);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
