using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Procurement;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _uow;

    public PurchaseOrderService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<PurchaseOrderDto>> GetAllAsync(PurchaseOrderStatus? status = null, Guid? supplierId = null)
    {
        var query = _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Branch)
            .Include(p => p.RequestedByUser)
            .Include(p => p.ApprovedByUser)
            .Include(p => p.Items).ThenInclude(i => i.Item)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        var rows = await query.OrderByDescending(p => p.DateCreated).ToListAsync();
        return rows.Select(MapToDto).ToList();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        var row = await LoadWithNavsAsync(id);
        return row is null ? null : MapToDto(row);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, Guid requestedByUserId)
    {
        var po = new PurchaseOrder
        {
            SupplierId = request.SupplierId,
            BranchId = request.BranchId,
            ReferenceNumber = request.ReferenceNumber?.Trim().ToUpperInvariant(),
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes = request.Notes?.Trim(),
            Status = PurchaseOrderStatus.Draft,
            RequestedByUserId = requestedByUserId
        };

        await _uow.Repository<PurchaseOrder>().AddAsync(po);
        await _uow.SaveChangesAsync(); // get PurchaseOrderId

        foreach (var line in request.Items)
        {
            var item = new PurchaseOrderItem
            {
                PurchaseOrderId = po.PurchaseOrderId,
                ItemId = line.ItemId,
                OrderedQuantity = line.OrderedQuantity,
                UnitCost = line.UnitCost,
                Notes = line.Notes?.Trim()
            };
            await _uow.Repository<PurchaseOrderItem>().AddAsync(item);
        }

        await _uow.SaveChangesAsync();

        var loaded = await LoadWithNavsAsync(po.PurchaseOrderId);
        return MapToDto(loaded!);
    }

    public async Task<PurchaseOrderDto?> SubmitAsync(int id, Guid userId)
    {
        var po = await _uow.Repository<PurchaseOrder>().Query()
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null || po.Status != PurchaseOrderStatus.Draft)
            return null;

        po.Status = PurchaseOrderStatus.Submitted;
        _uow.Repository<PurchaseOrder>().Update(po);
        await _uow.SaveChangesAsync();

        return MapToDto((await LoadWithNavsAsync(id))!);
    }

    public async Task<PurchaseOrderDto?> ApproveAsync(int id, Guid approvedByUserId)
    {
        var po = await _uow.Repository<PurchaseOrder>().Query()
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null || po.Status != PurchaseOrderStatus.Submitted)
            return null;

        po.Status = PurchaseOrderStatus.Approved;
        po.ApprovedByUserId = approvedByUserId;
        po.ApprovedAt = DateTime.UtcNow;
        _uow.Repository<PurchaseOrder>().Update(po);
        await _uow.SaveChangesAsync();

        return MapToDto((await LoadWithNavsAsync(id))!);
    }

    public async Task<PurchaseOrderDto?> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, Guid receivedByUserId)
    {
        var po = await _uow.Repository<PurchaseOrder>().Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null ||
            (po.Status != PurchaseOrderStatus.Approved &&
             po.Status != PurchaseOrderStatus.PartiallyReceived))
            return null;

        foreach (var line in request.Lines)
        {
            var poItem = po.Items.FirstOrDefault(i => i.PurchaseOrderItemId == line.PurchaseOrderItemId);
            if (poItem is null || line.ReceivedQuantity <= 0) continue;

            // Load item for stock update
            var item = await _uow.Repository<Item>().Query()
                .FirstOrDefaultAsync(i => i.ItemId == poItem.ItemId);
            if (item is null) continue;

            var stockBefore = item.InStock;
            item.InStock += line.ReceivedQuantity;
            _uow.Repository<Item>().Update(item);

            // StockMovement audit
            var movement = new StockMovement
            {
                ItemId = poItem.ItemId,
                MovementType = StockMovementType.Receive,
                QuantityDelta = line.ReceivedQuantity,
                StockBefore = stockBefore,
                StockAfter = item.InStock,
                PerformedByUserId = receivedByUserId,
                UnitCost = poItem.UnitCost,
                Reason = $"Goods receipt against PO #{po.PurchaseOrderId}" +
                         (po.ReferenceNumber is not null ? $" ({po.ReferenceNumber})" : ""),
                ReferenceCode = po.ReferenceNumber
            };
            await _uow.Repository<StockMovement>().AddAsync(movement);

            poItem.ReceivedQuantity += line.ReceivedQuantity;
            _uow.Repository<PurchaseOrderItem>().Update(poItem);
        }

        // Determine new status
        var allFulfilled = po.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
        po.Status = allFulfilled ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        if (allFulfilled) po.ReceivedAt = DateTime.UtcNow;
        _uow.Repository<PurchaseOrder>().Update(po);

        await _uow.SaveChangesAsync();

        return MapToDto((await LoadWithNavsAsync(id))!);
    }

    public async Task<PurchaseOrderDto?> CancelAsync(int id, Guid userId)
    {
        var po = await _uow.Repository<PurchaseOrder>().Query()
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

        if (po is null ||
            (po.Status != PurchaseOrderStatus.Draft &&
             po.Status != PurchaseOrderStatus.Submitted))
            return null;

        po.Status = PurchaseOrderStatus.Cancelled;
        _uow.Repository<PurchaseOrder>().Update(po);
        await _uow.SaveChangesAsync();

        return MapToDto((await LoadWithNavsAsync(id))!);
    }

    // ─── helpers ──────────────────────────────────────────────────────────────

    private async Task<PurchaseOrder?> LoadWithNavsAsync(int id)
        => await _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Branch)
            .Include(p => p.RequestedByUser)
            .Include(p => p.ApprovedByUser)
            .Include(p => p.Items).ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

    private static PurchaseOrderDto MapToDto(PurchaseOrder p) => new()
    {
        PurchaseOrderId = p.PurchaseOrderId,
        ReferenceNumber = p.ReferenceNumber,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        BranchId = p.BranchId,
        BranchName = p.Branch?.Name,
        Status = p.Status.ToString(),
        ExpectedDeliveryDate = p.ExpectedDeliveryDate,
        Notes = p.Notes,
        RequestedByUser = p.RequestedByUser?.Username ?? string.Empty,
        ApprovedByUser = p.ApprovedByUser?.Username,
        ApprovedAt = p.ApprovedAt,
        ReceivedAt = p.ReceivedAt,
        DateCreated = p.DateCreated,
        Items = p.Items.Select(i => new PurchaseOrderItemDto
        {
            PurchaseOrderItemId = i.PurchaseOrderItemId,
            ItemId = i.ItemId,
            ItemName = i.Item?.Name ?? string.Empty,
            ItemCode = i.Item?.Barcode ?? string.Empty,
            OrderedQuantity = i.OrderedQuantity,
            UnitCost = i.UnitCost,
            ReceivedQuantity = i.ReceivedQuantity,
            Notes = i.Notes
        }).ToList()
    };
}
