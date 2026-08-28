using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
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

    public async Task<DiscountOverrideMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var overrides = await _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.Item)
            .Include(r => r.Invoice)
            .ToListAsync(ct);

        var approved = overrides.Where(r => r.Status == DiscountOverrideStatus.Approved).ToList();
        decimal totalImpact = 0;
        foreach (var r in approved)
        {
            if (r.OverrideType == DiscountType.FixedAmount)
            {
                totalImpact += r.OverrideValue;
            }
            else if (r.Item != null)
            {
                totalImpact += Math.Round(r.Item.UnitPrice * (r.OverrideValue / 100m), 2);
            }
            else if (r.Invoice != null)
            {
                totalImpact += Math.Round(r.Invoice.TotalAmount * (r.OverrideValue / 100m), 2);
            }
        }

        return new DiscountOverrideMetricsDto
        {
            TotalRequests = overrides.Count,
            PendingCount = overrides.Count(r => r.Status == DiscountOverrideStatus.Pending),
            ApprovedCount = overrides.Count(r => r.Status == DiscountOverrideStatus.Approved),
            RejectedCount = overrides.Count(r => r.Status == DiscountOverrideStatus.Rejected),
            TotalEstimatedImpactXaf = totalImpact
        };
    }

    public async Task<PagedResult<DiscountOverrideDto>> GetOverridesPagedAsync(DiscountOverrideFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.RequestedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.ReviewedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.Item)
            .Include(r => r.Invoice)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<DiscountOverrideStatus>(request.Status, ignoreCase: true, out var st))
        {
            query = query.Where(r => r.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(request.OverrideType) &&
            Enum.TryParse<DiscountType>(request.OverrideType, ignoreCase: true, out var dt))
        {
            query = query.Where(r => r.OverrideType == dt);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(r =>
                (r.Justification != null && r.Justification.Contains(term)) ||
                (r.ReviewNotes != null && r.ReviewNotes.Contains(term)) ||
                (r.RequestedByUser != null && r.RequestedByUser.Username.Contains(term)) ||
                (r.RequestedByUser != null && r.RequestedByUser.Employee != null && (r.RequestedByUser.Employee.FirstName.Contains(term) || r.RequestedByUser.Employee.LastName.Contains(term))) ||
                (r.ReviewedByUser != null && r.ReviewedByUser.Username.Contains(term)) ||
                (r.Item != null && (r.Item.Name.Contains(term) || (r.Item.Barcode != null && r.Item.Barcode.Contains(term)))) ||
                (r.InvoiceId != null && r.InvoiceId.ToString()!.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var pagedRows = await query
            .OrderByDescending(r => r.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DiscountOverrideDto>
        {
            Items = pagedRows.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<DiscountOverrideDto>> GetAllAsync(string? status = null)
    {
        var query = _uow.Repository<DiscountOverrideRequest>().Query()
            .AsNoTracking()
            .Include(r => r.RequestedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.ReviewedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.Item)
            .Include(r => r.Invoice)
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
            .Include(r => r.RequestedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.ReviewedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.Item)
            .Include(r => r.Invoice)
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
            .Include(r => r.RequestedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.Item)
            .Include(r => r.Invoice)
            .FirstAsync(r => r.DiscountOverrideRequestId == row.DiscountOverrideRequestId);

        return MapToDto(loaded);
    }

    public async Task<DiscountOverrideDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request)
    {
        var row = await _uow.Repository<DiscountOverrideRequest>().Query()
            .Include(r => r.RequestedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.ReviewedByUser).ThenInclude(u => u.Employee)
            .Include(r => r.Item)
            .Include(r => r.Invoice)
            .FirstOrDefaultAsync(r => r.DiscountOverrideRequestId == id);

        if (row is null || row.Status != DiscountOverrideStatus.Pending)
            return null;

        row.Status = request.Approved ? DiscountOverrideStatus.Approved : DiscountOverrideStatus.Rejected;
        row.ReviewedByUserId = reviewedByUserId != Guid.Empty ? reviewedByUserId : null;
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
        InvoiceTotalAmount = r.Invoice?.TotalAmount,
        ItemId = r.ItemId,
        ItemName = r.Item?.Name,
        ItemBarcode = r.Item?.Barcode,
        ItemUnitPrice = r.Item?.UnitPrice,
        OverrideType = r.OverrideType.ToString(),
        OverrideValue = r.OverrideValue,
        Justification = r.Justification,
        Status = r.Status.ToString(),
        RequestedByUserId = r.RequestedByUserId,
        RequestedByUser = r.RequestedByUser?.Username ?? string.Empty,
        RequestedByFullName = r.RequestedByUser?.Employee != null ? $"{r.RequestedByUser.Employee.FirstName} {r.RequestedByUser.Employee.LastName}".Trim() : null,
        ReviewedByUserId = r.ReviewedByUserId,
        ReviewedByUser = r.ReviewedByUser?.Username,
        ReviewedByFullName = r.ReviewedByUser?.Employee != null ? $"{r.ReviewedByUser.Employee.FirstName} {r.ReviewedByUser.Employee.LastName}".Trim() : null,
        ReviewNotes = r.ReviewNotes,
        ReviewedAt = r.ReviewedAt,
        DateCreated = r.DateCreated
    };
}
