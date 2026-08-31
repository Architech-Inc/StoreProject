using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
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

    public async Task<PurchaseOrderMetricsDto> GetPurchaseOrderMetricsAsync(CancellationToken ct = default)
    {
        var orders = await _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Items)
            .ToListAsync(ct);

        var metrics = new PurchaseOrderMetricsDto
        {
            TotalOrders = orders.Count,
            PendingApprovalCount = orders.Count(p => p.Status == PurchaseOrderStatus.Submitted || p.Status == PurchaseOrderStatus.Draft),
            AwaitingDeliveryCount = orders.Count(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived),
            FulfilledCount = orders.Count(p => p.Status == PurchaseOrderStatus.Received),
            TotalCommittedValuationXaf = orders.Where(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived || p.Status == PurchaseOrderStatus.Submitted)
                .SelectMany(p => p.Items)
                .Sum(i => Math.Max(0, i.OrderedQuantity - i.ReceivedQuantity) * i.UnitCost),
            TotalReceivedValuationXaf = orders.SelectMany(p => p.Items)
                .Sum(i => i.ReceivedQuantity * i.UnitCost)
        };

        return metrics;
    }

    public async Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseOrderFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier).ThenInclude(s => s.Emails)
            .Include(p => p.Supplier).ThenInclude(s => s.Phones)
            .Include(p => p.Branch)
            .Include(p => p.RequestedByUser)
            .Include(p => p.ApprovedByUser)
            .Include(p => p.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
            .AsQueryable();

        if (request.SupplierId.HasValue && request.SupplierId.Value != Guid.Empty)
        {
            query = query.Where(p => p.SupplierId == request.SupplierId.Value);
        }

        if (request.BranchId.HasValue && request.BranchId.Value > 0)
        {
            query = query.Where(p => p.BranchId == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<PurchaseOrderStatus>(request.Status, ignoreCase: true, out var st))
        {
            query = query.Where(p => p.Status == st);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.DateCreated >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.DateCreated <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            if (int.TryParse(term.Replace("#", "").Replace("PO-", ""), out var searchId))
            {
                query = query.Where(p => p.PurchaseOrderId == searchId ||
                                         (p.ReferenceNumber != null && p.ReferenceNumber.Contains(term)) ||
                                         p.Supplier.Name.Contains(term) ||
                                         (p.Branch != null && p.Branch.Name.Contains(term)) ||
                                         p.RequestedByUser.Username.Contains(term) ||
                                         (p.Notes != null && p.Notes.Contains(term)) ||
                                         p.Items.Any(i => i.Item.Name.Contains(term) || (i.Item.Barcode != null && i.Item.Barcode.Contains(term))));
            }
            else
            {
                query = query.Where(p => (p.ReferenceNumber != null && p.ReferenceNumber.Contains(term)) ||
                                         p.Supplier.Name.Contains(term) ||
                                         (p.Branch != null && p.Branch.Name.Contains(term)) ||
                                         p.RequestedByUser.Username.Contains(term) ||
                                         (p.Notes != null && p.Notes.Contains(term)) ||
                                         p.Items.Any(i => i.Item.Name.Contains(term) || (i.Item.Barcode != null && i.Item.Barcode.Contains(term))));
            }
        }

        var total = await query.CountAsync(ct);
        var pagedItems = await query
            .OrderByDescending(p => p.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<PurchaseOrderDto>
        {
            Items = pagedItems.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<PurchaseOrderDto>> GetAllAsync(PurchaseOrderStatus? status = null, Guid? supplierId = null)
    {
        var query = _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier).ThenInclude(s => s.Emails)
            .Include(p => p.Supplier).ThenInclude(s => s.Phones)
            .Include(p => p.Branch)
            .Include(p => p.RequestedByUser)
            .Include(p => p.ApprovedByUser)
            .Include(p => p.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
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
            ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber)
                ? $"PO-{DateTime.UtcNow:yyyyMM}-{new Random().Next(1000, 9999)}"
                : request.ReferenceNumber.Trim().ToUpperInvariant(),
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
                PerformedByUserId = receivedByUserId != Guid.Empty ? receivedByUserId : null,
                UnitCost = poItem.UnitCost,
                UnitPrice = item.UnitPrice,
                Reason = $"Goods receipt against PO #{po.PurchaseOrderId}" +
                         (po.ReferenceNumber is not null ? $" ({po.ReferenceNumber})" : ""),
                ReferenceCode = po.ReferenceNumber ?? $"PO-{po.PurchaseOrderId}"
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

    public async Task<AutomatedReorderResultDto> ExecuteAutomatedReorderAsync(Guid? actingUserId = null, CancellationToken ct = default)
    {
        var result = new AutomatedReorderResultDto();

        // 1. Fetch depleted items
        var depletedItems = await _uow.Repository<Item>().Query()
            .Where(i => i.IsActive && i.ReorderLevel != null && i.ReorderLevel > 0 && i.InStock <= i.ReorderLevel)
            .Include(i => i.Batches)
            .ToListAsync(ct);

        if (depletedItems.Count == 0)
        {
            result.Message = "Inventory check complete: all items are adequately stocked above reorder levels.";
            return result;
        }

        result.DepletedItemsDetected = depletedItems.Count;

        // 2. Fetch active suppliers
        var allSuppliers = await _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .ToListAsync(ct);

        if (allSuppliers.Count == 0)
        {
            result.Message = $"Detected {depletedItems.Count} depleted items, but no registered suppliers were found.";
            return result;
        }

        var defaultSupplier = allSuppliers.First();

        // 3. Fallback default user if actingUserId is null
        Guid userId = actingUserId ?? Guid.Empty;
        if (userId == Guid.Empty)
        {
            var adminUser = await _uow.Repository<User>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Status == UserStatus.Active, ct);
            userId = adminUser?.UserId ?? Guid.NewGuid();
        }

        // 4. Group depleted items by Supplier (fallback to default supplier)
        var groupedBySupplier = depletedItems.GroupBy(item => defaultSupplier.SupplierId);

        foreach (var group in groupedBySupplier)
        {
            var supplierId = group.Key;
            var supplier = allSuppliers.FirstOrDefault(s => s.SupplierId == supplierId) ?? defaultSupplier;

            // Check if a Draft or Submitted PO already exists for this supplier created today
            var today = DateTime.UtcNow.Date;
            var existingPo = await _uow.Repository<PurchaseOrder>().Query()
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.SupplierId == supplierId && 
                                         (p.Status == PurchaseOrderStatus.Draft || p.Status == PurchaseOrderStatus.Submitted) && 
                                         p.DateCreated >= today, ct);

            if (existingPo != null)
            {
                // Update existing PO
                foreach (var item in group)
                {
                    if (!existingPo.Items.Any(i => i.ItemId == item.ItemId))
                    {
                        var qtyToOrder = Math.Max(1, (item.ReorderLevel!.Value * 2) - item.InStock);
                        var cost = item.CostPrice ?? Math.Round(item.UnitPrice * 0.7m, 2);

                        var line = new PurchaseOrderItem
                        {
                            ItemId = item.ItemId,
                            OrderedQuantity = qtyToOrder,
                            UnitCost = cost,
                            Notes = $"Auto-replenish: Stock {item.InStock} <= Reorder {item.ReorderLevel}"
                        };
                        existingPo.Items.Add(line);
                        result.TotalEstimatedValuationXaf += (qtyToOrder * cost);
                    }
                }

                _uow.Repository<PurchaseOrder>().Update(existingPo);
                result.OrdersUpdatedCount++;
                if (existingPo.ReferenceNumber != null && !result.GeneratedReferences.Contains(existingPo.ReferenceNumber))
                {
                    result.GeneratedReferences.Add(existingPo.ReferenceNumber);
                }
            }
            else
            {
                // Create a new PO
                var refNum = $"PO-AUTO-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}";
                var newPo = new PurchaseOrder
                {
                    SupplierId = supplierId,
                    Status = PurchaseOrderStatus.Draft,
                    ReferenceNumber = refNum,
                    RequestedByUserId = userId,
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    Notes = $"Automated stock replenishment order generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm}."
                };

                foreach (var item in group)
                {
                    var qtyToOrder = Math.Max(1, (item.ReorderLevel!.Value * 2) - item.InStock);
                    var cost = item.CostPrice ?? Math.Round(item.UnitPrice * 0.7m, 2);

                    newPo.Items.Add(new PurchaseOrderItem
                    {
                        ItemId = item.ItemId,
                        OrderedQuantity = qtyToOrder,
                        UnitCost = cost,
                        Notes = $"Auto-replenish: Stock {item.InStock} <= Reorder {item.ReorderLevel}"
                    });
                    result.TotalEstimatedValuationXaf += (qtyToOrder * cost);
                }

                await _uow.Repository<PurchaseOrder>().AddAsync(newPo);
                result.OrdersCreatedCount++;
                result.GeneratedReferences.Add(refNum);
            }
        }

        await _uow.SaveChangesAsync(ct);
        result.Message = $"Auto-reorder evaluated {depletedItems.Count} depleted items. Created {result.OrdersCreatedCount} new POs and updated {result.OrdersUpdatedCount} existing draft POs.";

        return result;
    }

    private async Task<PurchaseOrder?> LoadWithNavsAsync(int id)
        => await _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Supplier).ThenInclude(s => s.Emails)
            .Include(p => p.Supplier).ThenInclude(s => s.Phones)
            .Include(p => p.Branch)
            .Include(p => p.RequestedByUser)
            .Include(p => p.ApprovedByUser)
            .Include(p => p.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Category)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == id);

    private static PurchaseOrderDto MapToDto(PurchaseOrder p) => new()
    {
        PurchaseOrderId = p.PurchaseOrderId,
        ReferenceNumber = p.ReferenceNumber,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        SupplierEmail = p.Supplier?.Emails.FirstOrDefault()?.Email?.Address,
        SupplierPhone = p.Supplier?.Phones.FirstOrDefault()?.Phone?.Number,
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
            CategoryName = i.Item?.Category?.Name,
            OrderedQuantity = i.OrderedQuantity,
            UnitCost = i.UnitCost,
            ReceivedQuantity = i.ReceivedQuantity,
            Notes = i.Notes
        }).ToList()
    };
}
