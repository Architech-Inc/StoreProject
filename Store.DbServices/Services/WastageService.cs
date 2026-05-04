using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Inventory;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class WastageService : IWastageService
{
    private readonly IUnitOfWork _uow;

    public WastageService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<WastageEntryDto>> GetAllAsync(Guid? itemId = null, string? wastageType = null)
    {
        var query = _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item)
            .Include(w => w.RecordedByUser)
            .AsQueryable();

        if (itemId.HasValue)
            query = query.Where(w => w.ItemId == itemId.Value);

        if (!string.IsNullOrWhiteSpace(wastageType) &&
            Enum.TryParse<WastageType>(wastageType, ignoreCase: true, out var wt))
            query = query.Where(w => w.WastageType == wt);

        var entries = await query.OrderByDescending(w => w.DateCreated).ToListAsync();
        return entries.Select(MapToDto).ToList();
    }

    public async Task<WastageEntryDto?> GetByIdAsync(int id)
    {
        var entry = await _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item)
            .Include(w => w.RecordedByUser)
            .FirstOrDefaultAsync(w => w.WastageEntryId == id);

        return entry is null ? null : MapToDto(entry);
    }

    public async Task<WastageEntryDto> RecordAsync(RecordWastageRequest request, Guid recordedByUserId)
    {
        // Load the item to decrement stock
        var item = await _uow.Repository<Item>().Query()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId)
            ?? throw new InvalidOperationException($"Item {request.ItemId} not found.");

        var stockBefore = item.InStock;

        // Decrement stock (floor at 0 — negative stock not allowed)
        item.InStock = Math.Max(0, item.InStock - request.Quantity);
        _uow.Repository<Item>().Update(item);

        // Write the WastageEntry
        var entry = new WastageEntry
        {
            ItemId = request.ItemId,
            WastageType = request.WastageType,
            Quantity = request.Quantity,
            Notes = request.Notes?.Trim(),
            ReferenceCode = request.ReferenceCode?.Trim().ToUpperInvariant(),
            RecordedByUserId = recordedByUserId
        };
        await _uow.Repository<WastageEntry>().AddAsync(entry);

        // Write a StockMovement audit record (type Adjustment, negative delta)
        var movement = new StockMovement
        {
            ItemId = request.ItemId,
            MovementType = StockMovementType.Adjustment,
            QuantityDelta = -request.Quantity,
            StockBefore = stockBefore,
            StockAfter = item.InStock,
            PerformedByUserId = recordedByUserId,
            Reason = $"Wastage [{request.WastageType}]: {(request.Notes ?? "No notes")}",
            ReferenceCode = request.ReferenceCode
        };
        await _uow.Repository<StockMovement>().AddAsync(movement);

        await _uow.SaveChangesAsync();

        // Reload with navigations for DTO
        var loaded = await _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item)
            .Include(w => w.RecordedByUser)
            .FirstAsync(w => w.WastageEntryId == entry.WastageEntryId);

        return MapToDto(loaded);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entry = await _uow.Repository<WastageEntry>().Query()
            .FirstOrDefaultAsync(w => w.WastageEntryId == id);

        if (entry is null) return false;

        _uow.Repository<WastageEntry>().Remove(entry);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static WastageEntryDto MapToDto(WastageEntry w) => new()
    {
        WastageEntryId = w.WastageEntryId,
        ItemId = w.ItemId,
        ItemName = w.Item?.Name ?? string.Empty,
        ItemCode = w.Item?.Barcode ?? string.Empty,
        WastageType = w.WastageType.ToString(),
        Quantity = w.Quantity,
        Notes = w.Notes,
        ReferenceCode = w.ReferenceCode,
        RecordedByUser = w.RecordedByUser?.Username ?? string.Empty,
        DateCreated = w.DateCreated
    };
}
