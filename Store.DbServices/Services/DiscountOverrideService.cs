using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Discounts;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class DiscountOverrideService : IDiscountOverrideService
{
    private readonly IUnitOfWork _uow;

    public DiscountOverrideService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<DiscountOverrideDto>> GetAllAsync(string? status = null)
    {
        var query = _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Item)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<DiscountOverrideStatus>(status, ignoreCase: true, out var st))
            query = query.Where(r => r.Status == st);

        var rows = await query.OrderByDescending(r => r.DateCreated).ToListAsync();
        return rows.Select(MapToDto).ToList();
    }

    public async Task<DiscountOverrideDto?> GetByIdAsync(int id)
    {
        var row = await _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.DiscountOverrideRequestId == id);

        return row is null ? null : MapToDto(row);
    }

    public async Task<DiscountOverrideDto> CreateAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId)
    {
        var row = new DiscountOverrideRequest
        {
            InvoiceId = request.InvoiceId,
            ItemId = request.ItemId,
            OverrideType = request.OverrideType,
            OverrideValue = request.OverrideValue,
            Justification = request.Justification?.Trim(),
            Status = DiscountOverrideStatus.Pending,
            RequestedByUserId = requestedByUserId
        };

        await _uow.Repository<DiscountOverrideRequest>().AddAsync(row);
        await _uow.SaveChangesAsync();

        var loaded = await _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.Item)
            .FirstAsync(r => r.DiscountOverrideRequestId == row.DiscountOverrideRequestId);

        return MapToDto(loaded);
    }

    public async Task<DiscountOverrideDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request)
    {
        var row = await _uow.Repository<DiscountOverrideRequest>().Query()
            .Include(r => r.RequestedByUser)
            .Include(r => r.ReviewedByUser)
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.DiscountOverrideRequestId == id);

        if (row is null || row.Status != DiscountOverrideStatus.Pending)
            return null;

        row.Status = request.Approved ? DiscountOverrideStatus.Approved : DiscountOverrideStatus.Rejected;
        row.ReviewedByUserId = reviewedByUserId;
        row.ReviewNotes = request.ReviewNotes?.Trim();
        row.ReviewedAt = DateTime.UtcNow;

        _uow.Repository<DiscountOverrideRequest>().Update(row);
        await _uow.SaveChangesAsync();

        return MapToDto(row);
    }

    public async Task<bool> CancelAsync(int id, Guid userId)
    {
        var row = await _uow.Repository<DiscountOverrideRequest>().Query()
            .FirstOrDefaultAsync(r => r.DiscountOverrideRequestId == id);

        if (row is null || row.Status != DiscountOverrideStatus.Pending)
            return false;

        row.Status = DiscountOverrideStatus.Cancelled;
        _uow.Repository<DiscountOverrideRequest>().Update(row);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static DiscountOverrideDto MapToDto(DiscountOverrideRequest r) => new()
    {
        DiscountOverrideRequestId = r.DiscountOverrideRequestId,
        InvoiceId = r.InvoiceId,
        ItemId = r.ItemId,
        ItemName = r.Item?.Name,
        OverrideType = r.OverrideType.ToString(),
        OverrideValue = r.OverrideValue,
        Justification = r.Justification,
        Status = r.Status.ToString(),
        RequestedByUser = r.RequestedByUser?.Username ?? string.Empty,
        ReviewedByUser = r.ReviewedByUser?.Username,
        ReviewNotes = r.ReviewNotes,
        ReviewedAt = r.ReviewedAt,
        DateCreated = r.DateCreated
    };
}
