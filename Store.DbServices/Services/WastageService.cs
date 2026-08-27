using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
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

    public async Task<WastageMetricsDto> GetWastageMetricsAsync(CancellationToken ct = default)
    {
        var entries = await _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item)
            .ToListAsync(ct);

        var metrics = new WastageMetricsDto
        {
            TotalEntries = entries.Count,
            TotalQuantity = entries.Sum(w => w.Quantity),
            TotalValuationXaf = entries.Sum(w => w.Quantity * (w.Item?.CostPrice ?? 0)),
            TotalExpiredLossXaf = entries.Where(w => w.WastageType == WastageType.Expiry)
                .Sum(w => w.Quantity * (w.Item?.CostPrice ?? 0)),
            TotalDamagedLossXaf = entries.Where(w => w.WastageType == WastageType.Damage)
                .Sum(w => w.Quantity * (w.Item?.CostPrice ?? 0)),
            TotalSpoiledLossXaf = entries.Where(w => w.WastageType == WastageType.Spoilage)
                .Sum(w => w.Quantity * (w.Item?.CostPrice ?? 0)),
            TotalTheftLossXaf = entries.Where(w => w.WastageType == WastageType.Theft)
                .Sum(w => w.Quantity * (w.Item?.CostPrice ?? 0))
        };

        return metrics;
    }

    public async Task<PagedResult<WastageEntryDto>> GetWastagePagedAsync(WastageFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item).ThenInclude(i => i.Category)
            .Include(w => w.RecordedByUser)
            .AsQueryable();

        if (request.ItemId.HasValue && request.ItemId.Value != Guid.Empty)
        {
            query = query.Where(w => w.ItemId == request.ItemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.WastageType) &&
            Enum.TryParse<WastageType>(request.WastageType, ignoreCase: true, out var wt))
        {
            query = query.Where(w => w.WastageType == wt);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(w => w.DateCreated >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(w => w.DateCreated <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(w => w.Item.Name.Contains(term) ||
                                     (w.Item.Barcode != null && w.Item.Barcode.Contains(term)) ||
                                     (w.ReferenceCode != null && w.ReferenceCode.Contains(term)) ||
                                     (w.Notes != null && w.Notes.Contains(term)) ||
                                     (w.RecordedByUser != null && w.RecordedByUser.Username.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var pagedItems = await query
            .OrderByDescending(w => w.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<WastageEntryDto>
        {
            Items = pagedItems.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<WastageEntryDto>> GetAllAsync(Guid? itemId = null, string? wastageType = null)
    {
        var query = _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item).ThenInclude(i => i.Category)
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
            .Include(w => w.Item).ThenInclude(i => i.Category)
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
            ReferenceCode = string.IsNullOrWhiteSpace(request.ReferenceCode) 
                ? $"WASTE-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}"
                : request.ReferenceCode.Trim().ToUpperInvariant(),
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
            UnitCost = item.CostPrice,
            UnitPrice = item.UnitPrice,
            PerformedByUserId = recordedByUserId != Guid.Empty ? recordedByUserId : null,
            Reason = $"Wastage [{request.WastageType}]: {(request.Notes ?? "No notes")}",
            ReferenceCode = entry.ReferenceCode
        };
        await _uow.Repository<StockMovement>().AddAsync(movement);

        await _uow.SaveChangesAsync();

        // Reload with navigations for DTO
        var loaded = await _uow.Repository<WastageEntry>().Query()
            .AsNoTracking()
            .Include(w => w.Item).ThenInclude(i => i.Category)
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
        CategoryName = w.Item?.Category?.Name,
        WastageType = w.WastageType.ToString(),
        Quantity = w.Quantity,
        UnitCost = w.Item?.CostPrice ?? 0,
        InStock = w.Item?.InStock ?? 0,
        Notes = w.Notes,
        ReferenceCode = w.ReferenceCode,
        RecordedByUser = w.RecordedByUser?.Username ?? string.Empty,
        DateCreated = w.DateCreated
    };
}
