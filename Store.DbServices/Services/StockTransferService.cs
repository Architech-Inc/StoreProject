using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
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

    public async Task<TransferMetricsDto> GetTransferMetricsAsync(CancellationToken ct = default)
    {
        var transfers = await _uow.Repository<StockTransfer>().Query()
            .AsNoTracking()
            .Include(t => t.Items).ThenInclude(i => i.Item)
            .ToListAsync(ct);

        var metrics = new TransferMetricsDto
        {
            TotalTransfers = transfers.Count,
            TotalRequested = transfers.Count(t => t.Status == StockTransferStatus.Requested),
            TotalApproved = transfers.Count(t => t.Status == StockTransferStatus.Approved),
            TotalInTransit = transfers.Count(t => t.Status == StockTransferStatus.Dispatched),
            TotalReceived = transfers.Count(t => t.Status == StockTransferStatus.Received),
            TotalCancelled = transfers.Count(t => t.Status == StockTransferStatus.Cancelled),
            TotalTransferredUnits = transfers.Where(t => t.Status == StockTransferStatus.Received)
                .SelectMany(t => t.Items)
                .Sum(i => i.ReceivedQuantity ?? i.DispatchedQuantity ?? i.RequestedQuantity),
            TotalInTransitValuationXaf = transfers.Where(t => t.Status == StockTransferStatus.Dispatched)
                .SelectMany(t => t.Items)
                .Sum(i => (i.DispatchedQuantity ?? i.RequestedQuantity) * (i.Item?.CostPrice ?? 0))
        };

        return metrics;
    }

    public async Task<PagedResult<StockTransferDto>> GetTransfersPagedAsync(TransferFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<StockTransfer>().Query()
            .AsNoTracking()
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.RequestedByUser)
            .Include(t => t.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
            .AsQueryable();

        if (request.BranchId.HasValue && request.BranchId.Value > 0)
        {
            query = query.Where(t => t.FromBranchId == request.BranchId.Value || t.ToBranchId == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<StockTransferStatus>(request.Status, ignoreCase: true, out var st))
        {
            query = query.Where(t => t.Status == st);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.DateCreated >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.DateCreated <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            if (int.TryParse(term.Replace("#", "").Replace("TRF-", ""), out var searchId))
            {
                query = query.Where(t => t.StockTransferId == searchId ||
                                         t.FromBranch.Name.Contains(term) ||
                                         t.ToBranch.Name.Contains(term) ||
                                         t.RequestedByUser.Username.Contains(term) ||
                                         t.Notes!.Contains(term) ||
                                         t.Items.Any(i => i.Item.Name.Contains(term) || (i.Item.Barcode != null && i.Item.Barcode.Contains(term))));
            }
            else
            {
                query = query.Where(t => t.FromBranch.Name.Contains(term) ||
                                         t.ToBranch.Name.Contains(term) ||
                                         t.RequestedByUser.Username.Contains(term) ||
                                         t.Notes!.Contains(term) ||
                                         t.Items.Any(i => i.Item.Name.Contains(term) || (i.Item.Barcode != null && i.Item.Barcode.Contains(term))));
            }
        }

        var total = await query.CountAsync(ct);
        var pagedItems = await query
            .OrderByDescending(t => t.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<StockTransferDto>
        {
            Items = pagedItems.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<StockTransferDto>> GetAllAsync(int? branchId = null, string? status = null)
    {
        var query = _uow.Repository<StockTransfer>().Query()
            .AsNoTracking()
            .Include(t => t.FromBranch)
            .Include(t => t.ToBranch)
            .Include(t => t.RequestedByUser)
            .Include(t => t.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
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
            .Include(t => t.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
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

        var hasRoles = await _uow.Repository<UserBranchRole>().Query().AnyAsync(ubr => ubr.UserId == approvedByUserId);
        if (hasRoles)
        {
            var hasAccess = await _uow.Repository<UserBranchRole>().Query()
                .AnyAsync(ubr => ubr.UserId == approvedByUserId && (ubr.BranchId == transfer.FromBranchId || ubr.BranchId == transfer.ToBranchId));
            if (!hasAccess) throw new UnauthorizedAccessException("You do not have access to approve transfers for this branch.");
        }

        transfer.Status = StockTransferStatus.Approved;
        transfer.ApprovedByUserId = approvedByUserId != Guid.Empty ? approvedByUserId : null;
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
            .Include(t => t.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Approved)
            return null;

        var hasRoles = await _uow.Repository<UserBranchRole>().Query().AnyAsync(ubr => ubr.UserId == dispatchedByUserId);
        if (hasRoles)
        {
            var hasAccess = await _uow.Repository<UserBranchRole>().Query()
                .AnyAsync(ubr => ubr.UserId == dispatchedByUserId && ubr.BranchId == transfer.FromBranchId);
            if (!hasAccess) throw new UnauthorizedAccessException("You do not have access to dispatch transfers from this branch.");
        }

        foreach (var line in request.Items)
        {
            var item = transfer.Items.FirstOrDefault(i => i.StockTransferItemId == line.StockTransferItemId);
            if (item is not null)
            {
                item.DispatchedQuantity = line.DispatchedQuantity;

                // Immutable Stock Movement Audit (Dispatch Outflow)
                var catalogItem = item.Item ?? await _uow.Repository<Item>().GetByIdAsync(item.ItemId);
                if (catalogItem is not null)
                {
                    var stockBefore = catalogItem.InStock;
                    catalogItem.InStock = Math.Max(0, catalogItem.InStock - line.DispatchedQuantity);

                    var movement = new StockMovement
                    {
                        ItemId = catalogItem.ItemId,
                        MovementType = StockMovementType.Transfer,
                        QuantityDelta = -line.DispatchedQuantity,
                        StockBefore = stockBefore,
                        StockAfter = catalogItem.InStock,
                        UnitCost = catalogItem.CostPrice,
                        UnitPrice = catalogItem.UnitPrice,
                        Reason = $"Stock Transfer #{transfer.StockTransferId} Dispatched to Branch #{transfer.ToBranchId}",
                        ReferenceCode = $"TRF-{transfer.StockTransferId}-OUT",
                        PerformedByUserId = dispatchedByUserId != Guid.Empty ? dispatchedByUserId : null
                    };

                    await _uow.Repository<StockMovement>().AddAsync(movement);
                    _uow.Repository<Item>().Update(catalogItem);
                }
            }
        }

        transfer.Status = StockTransferStatus.Dispatched;
        transfer.DispatchedByUserId = dispatchedByUserId != Guid.Empty ? dispatchedByUserId : null;
        transfer.DispatchedAt = DateTime.UtcNow;
        if (request.Notes is not null) transfer.Notes = request.Notes.Trim();

        _uow.Repository<StockTransfer>().Update(transfer);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<StockTransferDto?> ReceiveAsync(int id, Guid receivedByUserId, ReceiveTransferRequest request)
    {
        var transfer = await _uow.Repository<StockTransfer>().Query()
            .Include(t => t.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(t => t.StockTransferId == id);

        if (transfer is null || transfer.Status != StockTransferStatus.Dispatched)
            return null;

        var hasRoles = await _uow.Repository<UserBranchRole>().Query().AnyAsync(ubr => ubr.UserId == receivedByUserId);
        if (hasRoles)
        {
            var hasAccess = await _uow.Repository<UserBranchRole>().Query()
                .AnyAsync(ubr => ubr.UserId == receivedByUserId && ubr.BranchId == transfer.ToBranchId);
            if (!hasAccess) throw new UnauthorizedAccessException("You do not have access to receive transfers at this branch.");
        }

        foreach (var line in request.Items)
        {
            var item = transfer.Items.FirstOrDefault(i => i.StockTransferItemId == line.StockTransferItemId);
            if (item is not null)
            {
                item.ReceivedQuantity = line.ReceivedQuantity;

                // Immutable Stock Movement Audit (Receive Inflow)
                var catalogItem = item.Item ?? await _uow.Repository<Item>().GetByIdAsync(item.ItemId);
                if (catalogItem is not null)
                {
                    var stockBefore = catalogItem.InStock;
                    catalogItem.InStock += line.ReceivedQuantity;

                    var movement = new StockMovement
                    {
                        ItemId = catalogItem.ItemId,
                        MovementType = StockMovementType.Transfer,
                        QuantityDelta = line.ReceivedQuantity,
                        StockBefore = stockBefore,
                        StockAfter = catalogItem.InStock,
                        UnitCost = catalogItem.CostPrice,
                        UnitPrice = catalogItem.UnitPrice,
                        Reason = $"Stock Transfer #{transfer.StockTransferId} Received from Branch #{transfer.FromBranchId}",
                        ReferenceCode = $"TRF-{transfer.StockTransferId}-IN",
                        PerformedByUserId = receivedByUserId != Guid.Empty ? receivedByUserId : null
                    };

                    await _uow.Repository<StockMovement>().AddAsync(movement);
                    _uow.Repository<Item>().Update(catalogItem);
                }
            }
        }

        transfer.Status = StockTransferStatus.Received;
        transfer.ReceivedByUserId = receivedByUserId != Guid.Empty ? receivedByUserId : null;
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
            ItemCode = i.Item?.Barcode,
            CategoryName = i.Item?.Category?.Name,
            UnitCost = i.Item?.CostPrice ?? 0,
            RequestedQuantity = i.RequestedQuantity,
            DispatchedQuantity = i.DispatchedQuantity,
            ReceivedQuantity = i.ReceivedQuantity,
            Notes = i.Notes
        }).ToList()
    };
}
