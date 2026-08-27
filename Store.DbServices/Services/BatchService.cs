using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class BatchService : IBatchService
{
    private readonly IUnitOfWork _uow;

    public BatchService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<BatchDto>> GetAllAsync(Guid? itemId = null, string? expiryStatus = null)
    {
        var query = _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .AsQueryable();

        if (itemId.HasValue)
            query = query.Where(b => b.ItemId == itemId.Value);

        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(30);

        if (!string.IsNullOrWhiteSpace(expiryStatus))
        {
            if (expiryStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date < today);
            else if (expiryStatus.Equals("Expiring", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date >= today && b.ExpiryDate.Value.Date <= cutoff);
            else if (expiryStatus.Equals("OK", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate == null || b.ExpiryDate.Value.Date > cutoff);
        }

        var batches = await query.OrderByDescending(b => b.ReceivedDate).ToListAsync();
        return batches.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<BatchDto>> GetBatchesPagedAsync(BatchFilterRequest request, CancellationToken ct = default)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 500);

        var query = _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .AsQueryable();

        if (request.ItemId.HasValue)
            query = query.Where(b => b.ItemId == request.ItemId.Value);

        if (request.FromExpiry.HasValue)
            query = query.Where(b => b.ExpiryDate >= request.FromExpiry.Value);

        if (request.ToExpiry.HasValue)
            query = query.Where(b => b.ExpiryDate <= request.ToExpiry.Value);

        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(30);

        if (!string.IsNullOrWhiteSpace(request.ExpiryStatus))
        {
            if (request.ExpiryStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date < today);
            else if (request.ExpiryStatus.Equals("Expiring", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date >= today && b.ExpiryDate.Value.Date <= cutoff);
            else if (request.ExpiryStatus.Equals("OK", StringComparison.OrdinalIgnoreCase))
                query = query.Where(b => b.ExpiryDate == null || b.ExpiryDate.Value.Date > cutoff);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(b => b.BatchNumber.Contains(term) ||
                                     b.Item.Name.Contains(term) ||
                                     (b.Item.Barcode != null && b.Item.Barcode.Contains(term)) ||
                                     (b.Notes != null && b.Notes.Contains(term)));
        }

        var total = await query.CountAsync(ct);

        var batches = await query
            .OrderByDescending(b => b.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = batches.Select(MapToDto).ToList();
        return new PagedResult<BatchDto>(dtos, total, page, pageSize);
    }

    public async Task<BatchMetricsDto> GetBatchMetricsAsync(CancellationToken ct = default)
    {
        var query = _uow.Repository<Batch>().Query().AsNoTracking();
        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(30);

        var totalBatches = await query.CountAsync(ct);
        var totalUnits = await query.SumAsync(b => (int?)b.Quantity, ct) ?? 0;
        var totalValuation = await query.SumAsync(b => (decimal?)(b.Quantity * b.CostPrice), ct) ?? 0;
        var totalExpired = await query.CountAsync(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date < today, ct);
        var totalExpiring = await query.CountAsync(b => b.ExpiryDate != null && b.ExpiryDate.Value.Date >= today && b.ExpiryDate.Value.Date <= cutoff, ct);

        return new BatchMetricsDto
        {
            TotalBatches = totalBatches,
            TotalExpiring30Days = totalExpiring,
            TotalExpired = totalExpired,
            TotalTrackedUnits = totalUnits,
            TotalBatchValuationXaf = totalValuation
        };
    }

    public async Task<BatchDto?> GetByIdAsync(Guid id)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(b => b.BatchId == id);

        return batch is null ? null : MapToDto(batch);
    }

    public async Task<BatchDto> CreateAsync(CreateBatchRequest request)
    {
        var batch = new Batch
        {
            BatchId = Guid.NewGuid(),
            ItemId = request.ItemId,
            BatchNumber = request.BatchNumber.Trim(),
            Quantity = request.Quantity,
            CostPrice = request.CostPrice,
            ReceivedDate = request.ReceivedDate,
            ExpiryDate = request.ExpiryDate,
            Notes = request.Notes?.Trim()
        };

        await _uow.Repository<Batch>().AddAsync(batch);
        await _uow.SaveChangesAsync();

        // Reload with Item & Category navigation
        var loaded = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .FirstAsync(b => b.BatchId == batch.BatchId);

        return MapToDto(loaded);
    }

    public async Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(b => b.BatchId == id);

        if (batch is null) return null;

        if (!string.IsNullOrWhiteSpace(request.BatchNumber)) batch.BatchNumber = request.BatchNumber.Trim();
        if (request.Quantity.HasValue) batch.Quantity = request.Quantity.Value;
        if (request.CostPrice.HasValue) batch.CostPrice = request.CostPrice.Value;
        if (request.ExpiryDate.HasValue) batch.ExpiryDate = request.ExpiryDate.Value;
        if (request.Notes is not null) batch.Notes = request.Notes.Trim();

        _uow.Repository<Batch>().Update(batch);
        await _uow.SaveChangesAsync();
        return MapToDto(batch);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .FirstOrDefaultAsync(b => b.BatchId == id);

        if (batch is null) return false;

        _uow.Repository<Batch>().Remove(batch);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WriteOffBatchAsync(WriteOffBatchRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .FirstOrDefaultAsync(b => b.BatchId == request.BatchId, ct);

        if (batch is null) return false;

        if (request.Quantity <= 0 || request.Quantity > batch.Quantity)
            throw new InvalidOperationException($"Write-off quantity must be between 1 and available batch quantity ({batch.Quantity}).");

        batch.Quantity -= request.Quantity;
        _uow.Repository<Batch>().Update(batch);

        var item = await _uow.Repository<Item>().GetByIdAsync(batch.ItemId, ct);
        if (item != null)
        {
            var before = item.InStock;
            item.InStock = Math.Max(0, item.InStock - request.Quantity);
            _uow.Repository<Item>().Update(item);

            // Audit Movement
            var movement = new StockMovement
            {
                ItemId = item.ItemId,
                PerformedByUserId = actingUserId,
                MovementType = StockMovementType.Wastage,
                QuantityDelta = -request.Quantity,
                StockBefore = before,
                StockAfter = item.InStock,
                UnitCost = batch.CostPrice,
                UnitPrice = item.UnitPrice,
                Reason = $"Batch {batch.BatchNumber} write-off: {request.Reason}",
                ReferenceCode = $"BATCH-WRITEOFF-{DateTime.UtcNow:yyyyMMddHHmmss}"
            };
            await _uow.Repository<StockMovement>().AddAsync(movement, ct);

            // Wastage Entry
            if (actingUserId.HasValue)
            {
                var wastage = new WastageEntry
                {
                    ItemId = item.ItemId,
                    WastageType = request.WastageType,
                    Quantity = request.Quantity,
                    Notes = $"Batch #{batch.BatchNumber}: {request.Reason}. {request.Notes}".Trim(),
                    ReferenceCode = $"BATCH-{batch.BatchNumber}",
                    RecordedByUserId = actingUserId.Value
                };
                await _uow.Repository<WastageEntry>().AddAsync(wastage, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(withinDays);

        var batches = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
                .ThenInclude(i => i.Category)
            .Where(b => b.ExpiryDate != null && b.ExpiryDate >= now && b.ExpiryDate <= cutoff)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        return batches.Select(MapToDto).ToList();
    }

    private static BatchDto MapToDto(Batch b)
    {
        var today = DateTime.UtcNow.Date;
        var daysUntil = b.ExpiryDate.HasValue
            ? (int)(b.ExpiryDate.Value.Date - today).TotalDays
            : int.MaxValue;

        string expiryStatus = "OK";
        if (b.ExpiryDate.HasValue)
        {
            if (daysUntil < 0) expiryStatus = "Expired";
            else if (daysUntil <= 30) expiryStatus = "Expiring";
        }

        return new BatchDto
        {
            BatchId = b.BatchId,
            ItemId = b.ItemId,
            ItemName = b.Item?.Name ?? string.Empty,
            ItemCode = b.Item?.Barcode ?? string.Empty,
            CategoryName = b.Item?.Category?.Name,
            BatchNumber = b.BatchNumber,
            Quantity = b.Quantity,
            CostPrice = b.CostPrice,
            ReceivedDate = b.ReceivedDate,
            ExpiryDate = b.ExpiryDate,
            Notes = b.Notes,
            DaysUntilExpiry = b.ExpiryDate.HasValue ? Math.Max(daysUntil, -999) : int.MaxValue,
            ExpiryStatus = expiryStatus
        };
    }
}
