using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Cash;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class CashVarianceService : ICashVarianceService
{
    private readonly IUnitOfWork _uow;

    public CashVarianceService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null)
    {
        var query = _uow.Repository<CashVarianceRecord>().Query()
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .Include(v => v.ReviewedByUser)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        var rows = await query.OrderByDescending(v => v.DateCreated).ToListAsync();
        return rows.Select(MapToDto).ToList();
    }

    public async Task<CashVarianceDto?> GetByIdAsync(int id)
    {
        var row = await _uow.Repository<CashVarianceRecord>().Query()
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .Include(v => v.ReviewedByUser)
            .FirstOrDefaultAsync(v => v.CashVarianceRecordId == id);

        return row is null ? null : MapToDto(row);
    }

    public async Task<List<CashVarianceDto>> GetByShiftAsync(Guid cashierShiftId)
    {
        var rows = await _uow.Repository<CashVarianceRecord>().Query()
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .Include(v => v.ReviewedByUser)
            .Where(v => v.CashierShiftId == cashierShiftId)
            .OrderByDescending(v => v.DateCreated)
            .ToListAsync();

        return rows.Select(MapToDto).ToList();
    }

    public async Task<CashVarianceDto> RecordAsync(RecordCashVarianceRequest request, Guid recordedByUserId)
    {
        var record = new CashVarianceRecord
        {
            CashierShiftId = request.CashierShiftId,
            ExpectedAmount = request.ExpectedAmount,
            ActualAmount = request.ActualAmount,
            ReasonCode = request.ReasonCode?.Trim().ToUpperInvariant(),
            Notes = request.Notes?.Trim(),
            Status = CashVarianceStatus.Pending,
            RecordedByUserId = recordedByUserId
        };

        await _uow.Repository<CashVarianceRecord>().AddAsync(record);
        await _uow.SaveChangesAsync();

        var loaded = await _uow.Repository<CashVarianceRecord>().Query()
            .AsNoTracking()
            .Include(v => v.RecordedByUser)
            .FirstAsync(v => v.CashVarianceRecordId == record.CashVarianceRecordId);

        return MapToDto(loaded);
    }

    public async Task<CashVarianceDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewCashVarianceRequest request)
    {
        var record = await _uow.Repository<CashVarianceRecord>().Query()
            .Include(v => v.RecordedByUser)
            .Include(v => v.ReviewedByUser)
            .FirstOrDefaultAsync(v => v.CashVarianceRecordId == id);

        if (record is null || record.Status != CashVarianceStatus.Pending)
            return null;

        record.Status = request.Status;
        record.ReviewedByUserId = reviewedByUserId;
        record.ReviewNotes = request.ReviewNotes?.Trim();
        record.ReviewedAt = DateTime.UtcNow;

        _uow.Repository<CashVarianceRecord>().Update(record);
        await _uow.SaveChangesAsync();

        return MapToDto(record);
    }

    private static CashVarianceDto MapToDto(CashVarianceRecord v) => new()
    {
        CashVarianceRecordId = v.CashVarianceRecordId,
        CashierShiftId = v.CashierShiftId,
        ExpectedAmount = v.ExpectedAmount,
        ActualAmount = v.ActualAmount,
        ReasonCode = v.ReasonCode,
        Notes = v.Notes,
        Status = v.Status.ToString(),
        RecordedByUser = v.RecordedByUser?.Username ?? string.Empty,
        ReviewedByUser = v.ReviewedByUser?.Username,
        ReviewNotes = v.ReviewNotes,
        ReviewedAt = v.ReviewedAt,
        DateCreated = v.DateCreated
    };
}
