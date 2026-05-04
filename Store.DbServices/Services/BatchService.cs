using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Inventory;
using Store.Models.Entities;
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
            .AsQueryable();

        if (itemId.HasValue)
            query = query.Where(b => b.ItemId == itemId.Value);

        var batches = await query.OrderByDescending(b => b.ReceivedDate).ToListAsync();
        var dtos = batches.Select(MapToDto).ToList();

        if (!string.IsNullOrWhiteSpace(expiryStatus))
            dtos = dtos.Where(d => d.ExpiryStatus.Equals(expiryStatus, StringComparison.OrdinalIgnoreCase)).ToList();

        return dtos;
    }

    public async Task<BatchDto?> GetByIdAsync(Guid id)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
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

        // Reload with Item navigation
        var loaded = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
            .FirstAsync(b => b.BatchId == batch.BatchId);

        return MapToDto(loaded);
    }

    public async Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request)
    {
        var batch = await _uow.Repository<Batch>().Query()
            .Include(b => b.Item)
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

    public async Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(withinDays);

        var batches = await _uow.Repository<Batch>().Query()
            .AsNoTracking()
            .Include(b => b.Item)
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
