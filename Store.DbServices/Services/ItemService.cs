using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _uow;

    public ItemService(IUnitOfWork uow) => _uow = uow;

    public async Task<ItemDto?> GetByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().Query()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Include(i => i.Manufacturer)
            .Include(i => i.Discount)
            .Include(i => i.ItemExpiry)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        return item is null ? null : MapToDto(item);
    }

    public async Task<PagedResult<ItemDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Item>().Query()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Include(i => i.Discount)
            .Where(i => i.IsActive)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(i => i.Name.Contains(request.SearchTerm) ||
                                     (i.Barcode != null && i.Barcode.Contains(request.SearchTerm)));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(i => i.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return new PagedResult<ItemDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<IEnumerable<ItemDto>> GetLowStockAsync(CancellationToken ct = default)
    {
        var items = await _uow.Repository<Item>().Query()
            .Include(i => i.Category)
            .Include(i => i.Unit)
            .Where(i => i.IsActive && i.InStock <= i.ReorderLevel)
            .AsNoTracking()
            .OrderBy(i => i.InStock)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return items;
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default)
    {
        if (request.Barcode is not null &&
            await _uow.Repository<Item>().ExistsAsync(i => i.Barcode == request.Barcode.Trim(), ct))
            throw new InvalidOperationException($"An item with barcode '{request.Barcode}' already exists.");

        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            UnitPrice = request.UnitPrice,
            CostPrice = request.CostPrice,
            InStock = request.InStock,
            ReorderLevel = request.ReorderLevel,
            Barcode = request.Barcode?.Trim(),
            Type = request.Type,
            CategoryId = request.CategoryId,
            UnitId = request.UnitId,
            ManufacturerId = request.ManufacturerId,
            IsActive = true
        };

        await _uow.Repository<Item>().AddAsync(item, ct);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(item.ItemId, ct))!;
    }

    public async Task<ItemDto?> UpdateAsync(Guid itemId, UpdateItemRequest request, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().Query()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        if (item is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name)) item.Name = request.Name.Trim();
        if (request.Description is not null) item.Description = request.Description.Trim();
        if (request.UnitPrice.HasValue) item.UnitPrice = request.UnitPrice.Value;
        if (request.CostPrice.HasValue) item.CostPrice = request.CostPrice.Value;
        if (request.ReorderLevel.HasValue) item.ReorderLevel = request.ReorderLevel.Value;
        if (request.Barcode is not null) item.Barcode = request.Barcode.Trim();
        if (request.Type.HasValue) item.Type = request.Type.Value;
        if (request.CategoryId.HasValue) item.CategoryId = request.CategoryId;
        if (request.UnitId.HasValue) item.UnitId = request.UnitId;
        if (request.ManufacturerId.HasValue) item.ManufacturerId = request.ManufacturerId;
        if (request.IsActive.HasValue) item.IsActive = request.IsActive.Value;

        _uow.Repository<Item>().Update(item);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(itemId, ct);
    }

    public async Task<bool> AdjustStockAsync(Guid itemId, AdjustStockRequest request, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().Query()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        if (item is null) return false;

        item.InStock += request.AdjustmentQuantity;
        if (item.InStock < 0)
            throw new InvalidOperationException("Stock cannot go below zero.");

        _uow.Repository<Item>().Update(item);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().Query()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        if (item is null) return false;

        item.IsActive = false;  // soft delete
        _uow.Repository<Item>().Update(item);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static ItemDto MapToDto(Item i) => new()
    {
        ItemId = i.ItemId,
        Name = i.Name,
        Description = i.Description,
        UnitPrice = i.UnitPrice,
        CostPrice = i.CostPrice,
        InStock = i.InStock,
        ReorderLevel = i.ReorderLevel,
        Barcode = i.Barcode,
        Type = i.Type,
        IsActive = i.IsActive,
        CategoryId = i.CategoryId,
        CategoryName = i.Category?.Name,
        UnitId = i.UnitId,
        UnitAbbreviation = i.Unit?.Abbreviation,
        ManufacturerId = i.ManufacturerId,
        ManufacturerName = i.Manufacturer?.Name,
        ImagePath = i.ImagePath,
        DiscountPercentage = i.Discount?.IsActive == true ? i.Discount.Percentage : null,
        DateCreated = i.DateCreated
    };
}
