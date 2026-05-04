using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Transfers;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class StockTransferService : IStockTransferService
{
    private readonly IUnitOfWork _uow;

    public StockTransferService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null)
    {
        var query = _uow.Repository<StockTransfer>().Query()
            .AsNoTracking()
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.RequestedByUser)
            .Include(t => t.Items).ThenInclude(i => i.Item)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(t => t.FromBranchId == branchId.Value || t.ToBranchId == branchId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StockTransferStatus>(status, ignoreCase: true, out var st))
            query = query.Where(t => t.Status == st);

        var transfers = await query.OrderByDescending(t => t.DateCreated).ToListAsync();
        return transfers.Select(MapToDto).ToList();
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .AsNoTracking()
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.RequestedByUser)
            .Include(t => t.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        return transfer is null ? null : MapToDto(transfer);
    }

    public async Task<StockTransferDto> CreateAsync(CreateTransferRequest request, Guid requestedByUserId)
    {
        var transfer = new StockTransfer
        {
            FromBranchId = request.FromBranchId,
            ToBranchId = request.ToBranchId,
            RequestedByUserId = requestedByUserId,
            Status = StockTransferStatus.Requested,
            Notes = request.Notes?.Trim(),
            Items = request.Items.Select(i => new StockTransferItem
            {
                ItemId = i.ItemId,
                RequestedQuantity = i.RequestedQuantity,
                Notes = i.Notes?.Trim()
            }).ToList()
        };

        await _uow.Repository<StockTransfer>().AddAsync(transfer);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(transfer.StockTransferId) ?? MapToDto(transfer);
    }

    public async Task<StockTransferDto?> ApproveAsync(int id, Guid approvedByUserId, ApproveTransferRequest request)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Requested)
            return null;

        transfer.Status = StockTransferStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId;
        transfer.ApprovedAt = DateTime.UtcNow;
        if (request.Notes is not null) transfer.Notes = request.Notes.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> RejectAsync(int id, Guid userId, RejectTransferRequest request)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Requested)
            return false;

        transfer.Status = StockTransferStatus.Cancelled;
        transfer.RejectionReason = request.Reason.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<StockTransferDto?> DispatchAsync(int id, Guid dispatchedByUserId, DispatchTransferRequest request)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Approved)
            return null;

        foreach (var line in request.Items)
        {
            var item = transfer.Items.FirstOrDefault(i => i.StockTransferItemId == line.StockTransferItemId);
            if (item is not null) item.DispatchedQuantity = line.DispatchedQuantity;
        }

        transfer.Status = StockTransferStatus.Dispatched;
        transfer.DispatchedByUserId = dispatchedByUserId;
        transfer.DispatchedAt = DateTime.UtcNow;
        if (request.Notes is not null) transfer.Notes = request.Notes.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Dispatched)
            return null;

        foreach (var line in request.Items)
        {
            var item = transfer.Items.FirstOrDefault(i => i.StockTransferItemId == line.StockTransferItemId);
            if (item is not null) item.ReceivedQuantity = line.ReceivedQuantity;
        }

        transfer.Status = StockTransferStatus.Received;
        transfer.ReceivedByUserId = receivedByUserId;
        transfer.ReceivedAt = DateTime.UtcNow;
        if (request.Notes is not null) transfer.Notes = request.Notes.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> CancelAsync(int id, Guid userId, string? reason)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status is StockTransferStatus.Received or StockTransferStatus.Cancelled)
            return false;

        transfer.Status = StockTransferStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
            transfer.RejectionReason = reason.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static StockTransferDto MapToDto(StockTransfer t) => new()
    {
        StockTransferId = t.StockTransferId,
        FromBranchId = t.FromBranchId,
        FromBranchName = t.FromBranch?.Name ?? string.Empty,
        ToBranchId = t.ToBranchId,
        ToBranchName = t.ToBranch?.Name ?? string.Empty,
        RequestedByUser = t.RequestedByUser?.Username ?? string.Empty,
        Status = t.Status.ToString(),
        Notes = t.Notes,
        RejectionReason = t.RejectionReason,
        DateCreated = t.DateCreated,
        ApprovedAt = t.ApprovedAt,
        DispatchedAt = t.DispatchedAt,
        ReceivedAt = t.ReceivedAt,
        Items = t.Items.Select(i => new StockTransferItemDto
        {
            StockTransferItemId = i.StockTransferItemId,
            ItemId = i.ItemId,
            ItemName = i.Item?.Name ?? string.Empty,
            RequestedQuantity = i.RequestedQuantity,
            DispatchedQuantity = i.DispatchedQuantity,
            ReceivedQuantity = i.ReceivedQuantity,
            Notes = i.Notes
        }).ToList()
    };
}
