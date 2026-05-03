using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class StoreOperationsService : IStoreOperationsService
{
    private readonly IUnitOfWork _uow;

    public StoreOperationsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetStockMovementsAsync(int page, int pageSize, StockMovementType? type = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);

        var query = _uow.Repository<StockMovement>().Query()
            .Include(x => x.Item)
            .Include(x => x.PerformedByUser)
            .AsNoTracking();

        if (type.HasValue)
            query = query.Where(x => x.MovementType == type.Value);

        var rows = await query
            .OrderByDescending(x => x.DateCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockMovementDto
            {
                StockMovementId = x.StockMovementId,
                ItemId = x.ItemId,
                ItemName = x.Item.Name,
                MovementType = x.MovementType,
                QuantityDelta = x.QuantityDelta,
                StockBefore = x.StockBefore,
                StockAfter = x.StockAfter,
                Reason = x.Reason,
                ReferenceCode = x.ReferenceCode,
                PerformedByUserName = x.PerformedByUser != null ? x.PerformedByUser.Username : null,
                DateCreated = x.DateCreated
            })
            .ToListAsync(ct);

        return rows;
    }

    public async Task<InventoryOperationResultDto> ReceiveGoodsAsync(GoodsReceiptRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        if (request.Lines.Count == 0)
        {
            throw new InvalidOperationException("Goods receipt requires at least one line.");
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            foreach (var line in request.Lines)
            {
                var item = await _uow.Repository<Item>().GetByIdAsync(line.ItemId, ct)
                    ?? throw new InvalidOperationException($"Item '{line.ItemId}' not found.");

                var before = item.InStock;
                item.InStock += line.Quantity;
                _uow.Repository<Item>().Update(item);

                var movement = new StockMovement
                {
                    ItemId = item.ItemId,
                    ItemsOrderId = request.ItemsOrderId,
                    PerformedByUserId = actingUserId,
                    MovementType = StockMovementType.Receive,
                    QuantityDelta = line.Quantity,
                    StockBefore = before,
                    StockAfter = item.InStock,
                    UnitCost = line.UnitCost ?? item.CostPrice,
                    UnitPrice = item.UnitPrice,
                    Reason = string.IsNullOrWhiteSpace(request.Notes) ? "Goods receipt" : request.Notes.Trim(),
                    ReferenceCode = request.ReferenceCode
                };

                await _uow.Repository<StockMovement>().AddAsync(movement, ct);

                await AddChangeLogAsync(actingUserId, "Item", item.ItemId.ToString(), ChangeLogAction.Updated,
                    null,
                    $"Goods receipt +{line.Quantity}. Before={before}, After={item.InStock}, Ref={request.ReferenceCode}",
                    ct);
            }

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return new InventoryOperationResultDto
            {
                Success = true,
                Message = "Goods receipt processed successfully."
            };
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<InventoryOperationResultDto> ProcessReturnAsync(StockReturnRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().GetByIdAsync(request.ItemId, ct)
            ?? throw new InvalidOperationException("Item not found.");

        var before = item.InStock;
        item.InStock += request.Quantity;
        _uow.Repository<Item>().Update(item);

        var movement = new StockMovement
        {
            ItemId = item.ItemId,
            InvoiceId = request.InvoiceId,
            PerformedByUserId = actingUserId,
            MovementType = StockMovementType.Return,
            QuantityDelta = request.Quantity,
            StockBefore = before,
            StockAfter = item.InStock,
            UnitCost = item.CostPrice,
            UnitPrice = item.UnitPrice,
            Reason = request.Reason,
            ReferenceCode = request.InvoiceId?.ToString()
        };

        await _uow.Repository<StockMovement>().AddAsync(movement, ct);

        await AddChangeLogAsync(actingUserId, "Item", item.ItemId.ToString(), ChangeLogAction.Updated,
            null,
            $"Stock return +{request.Quantity}. Before={before}, After={item.InStock}",
            ct);

        await _uow.SaveChangesAsync(ct);

        return new InventoryOperationResultDto
        {
            Success = true,
            Message = "Return processed and stock restored.",
            ItemId = item.ItemId,
            UpdatedStock = item.InStock
        };
    }

    public async Task<InventoryOperationResultDto> AdjustStockAsync(StockAdjustmentAuditRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().GetByIdAsync(request.ItemId, ct)
            ?? throw new InvalidOperationException("Item not found.");

        var before = item.InStock;
        var after = before + request.QuantityDelta;
        if (after < 0)
        {
            throw new InvalidOperationException("Adjustment would result in negative stock.");
        }

        item.InStock = after;
        _uow.Repository<Item>().Update(item);

        var movement = new StockMovement
        {
            ItemId = item.ItemId,
            PerformedByUserId = actingUserId,
            MovementType = StockMovementType.Adjustment,
            QuantityDelta = request.QuantityDelta,
            StockBefore = before,
            StockAfter = after,
            UnitCost = item.CostPrice,
            UnitPrice = item.UnitPrice,
            Reason = request.Reason,
            ReferenceCode = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        await _uow.Repository<StockMovement>().AddAsync(movement, ct);

        await AddChangeLogAsync(actingUserId, "Item", item.ItemId.ToString(), ChangeLogAction.Updated,
            null,
            $"Stock adjustment {request.QuantityDelta:+#;-#;0}. Before={before}, After={after}, Reason={request.Reason}",
            ct);

        await _uow.SaveChangesAsync(ct);

        return new InventoryOperationResultDto
        {
            Success = true,
            Message = "Stock adjusted.",
            ItemId = item.ItemId,
            UpdatedStock = item.InStock
        };
    }

    public async Task<IReadOnlyList<ReorderSuggestionDto>> GetLowStockReorderSuggestionsAsync(CancellationToken ct = default)
    {
        var items = await _uow.Repository<Item>().Query()
            .AsNoTracking()
            .Where(i => i.IsActive && i.ReorderLevel.HasValue && i.InStock <= i.ReorderLevel.Value)
            .OrderBy(i => i.InStock)
            .ToListAsync(ct);

        return items.Select(i =>
        {
            var reorderLevel = i.ReorderLevel ?? 0;
            var suggested = Math.Max((reorderLevel * 2) - i.InStock, 1);
            var unitCost = i.CostPrice ?? Math.Round(i.UnitPrice * 0.60m, 2);
            return new ReorderSuggestionDto
            {
                ItemId = i.ItemId,
                ItemName = i.Name,
                CurrentStock = i.InStock,
                ReorderLevel = reorderLevel,
                SuggestedOrderQuantity = suggested,
                UnitCost = unitCost,
                EstimatedCost = Math.Round(unitCost * suggested, 2)
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<TaxProfileDto>> GetTaxProfilesAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<TaxProfile>().Query()
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TaxProfileDto
            {
                TaxProfileId = x.TaxProfileId,
                Name = x.Name,
                RatePercent = x.RatePercent,
                ApplicationType = x.ApplicationType,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<TaxProfileDto> UpsertTaxProfileAsync(UpsertTaxProfileRequest request, CancellationToken ct = default)
    {
        TaxProfile entity;
        if (request.TaxProfileId.HasValue)
        {
            entity = await _uow.Repository<TaxProfile>().GetByIdAsync(request.TaxProfileId.Value, ct)
                ?? throw new InvalidOperationException("Tax profile not found.");
        }
        else
        {
            entity = new TaxProfile();
            await _uow.Repository<TaxProfile>().AddAsync(entity, ct);
        }

        entity.Name = request.Name.Trim();
        entity.RatePercent = request.RatePercent;
        entity.ApplicationType = request.ApplicationType;
        entity.IsActive = request.IsActive;

        await _uow.SaveChangesAsync(ct);

        return new TaxProfileDto
        {
            TaxProfileId = entity.TaxProfileId,
            Name = entity.Name,
            RatePercent = entity.RatePercent,
            ApplicationType = entity.ApplicationType,
            IsActive = entity.IsActive
        };
    }

    public async Task<IReadOnlyList<BundleRuleDto>> GetBundleRulesAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<BundleRule>().Query()
            .Include(x => x.TriggerItem)
            .Include(x => x.RewardItem)
            .AsNoTracking()
            .OrderByDescending(x => x.DateCreated)
            .Select(x => new BundleRuleDto
            {
                BundleRuleId = x.BundleRuleId,
                Name = x.Name,
                TriggerItemId = x.TriggerItemId,
                TriggerItemName = x.TriggerItem.Name,
                RewardItemId = x.RewardItemId,
                RewardItemName = x.RewardItem.Name,
                TriggerQuantity = x.TriggerQuantity,
                RewardQuantity = x.RewardQuantity,
                RewardDiscountPercent = x.RewardDiscountPercent,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<BundleRuleDto> UpsertBundleRuleAsync(UpsertBundleRuleRequest request, CancellationToken ct = default)
    {
        BundleRule entity;
        if (request.BundleRuleId.HasValue)
        {
            entity = await _uow.Repository<BundleRule>().GetByIdAsync(request.BundleRuleId.Value, ct)
                ?? throw new InvalidOperationException("Bundle rule not found.");
        }
        else
        {
            entity = new BundleRule();
            await _uow.Repository<BundleRule>().AddAsync(entity, ct);
        }

        entity.Name = request.Name.Trim();
        entity.TriggerItemId = request.TriggerItemId;
        entity.RewardItemId = request.RewardItemId;
        entity.TriggerQuantity = request.TriggerQuantity;
        entity.RewardQuantity = request.RewardQuantity;
        entity.RewardDiscountPercent = request.RewardDiscountPercent;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;
        entity.IsActive = request.IsActive;

        await _uow.SaveChangesAsync(ct);

        var triggerName = await _uow.Repository<Item>().Query().Where(i => i.ItemId == entity.TriggerItemId).Select(i => i.Name).FirstAsync(ct);
        var rewardName = await _uow.Repository<Item>().Query().Where(i => i.ItemId == entity.RewardItemId).Select(i => i.Name).FirstAsync(ct);

        return new BundleRuleDto
        {
            BundleRuleId = entity.BundleRuleId,
            Name = entity.Name,
            TriggerItemId = entity.TriggerItemId,
            TriggerItemName = triggerName,
            RewardItemId = entity.RewardItemId,
            RewardItemName = rewardName,
            TriggerQuantity = entity.TriggerQuantity,
            RewardQuantity = entity.RewardQuantity,
            RewardDiscountPercent = entity.RewardDiscountPercent,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsActive = entity.IsActive
        };
    }

    public async Task<IReadOnlyList<SegmentPricingDto>> GetSegmentPricingsAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<CustomerSegmentPrice>().Query()
            .Include(x => x.Item)
            .AsNoTracking()
            .OrderByDescending(x => x.DateCreated)
            .Select(x => new SegmentPricingDto
            {
                CustomerSegmentPriceId = x.CustomerSegmentPriceId,
                ItemId = x.ItemId,
                ItemName = x.Item.Name,
                Segment = x.Segment,
                PriceOverride = x.PriceOverride,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<SegmentPricingDto> UpsertSegmentPricingAsync(UpsertSegmentPricingRequest request, CancellationToken ct = default)
    {
        CustomerSegmentPrice entity;
        if (request.CustomerSegmentPriceId.HasValue)
        {
            entity = await _uow.Repository<CustomerSegmentPrice>().GetByIdAsync(request.CustomerSegmentPriceId.Value, ct)
                ?? throw new InvalidOperationException("Segment pricing not found.");
        }
        else
        {
            entity = new CustomerSegmentPrice();
            await _uow.Repository<CustomerSegmentPrice>().AddAsync(entity, ct);
        }

        entity.ItemId = request.ItemId;
        entity.Segment = request.Segment;
        entity.PriceOverride = request.PriceOverride;
        entity.ValidFrom = request.ValidFrom;
        entity.ValidTo = request.ValidTo;
        entity.IsActive = request.IsActive;

        await _uow.SaveChangesAsync(ct);

        var itemName = await _uow.Repository<Item>().Query()
            .Where(i => i.ItemId == entity.ItemId)
            .Select(i => i.Name)
            .FirstAsync(ct);

        return new SegmentPricingDto
        {
            CustomerSegmentPriceId = entity.CustomerSegmentPriceId,
            ItemId = entity.ItemId,
            ItemName = itemName,
            Segment = entity.Segment,
            PriceOverride = entity.PriceOverride,
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            IsActive = entity.IsActive
        };
    }

    public async Task<PricingPreviewDto?> GetPricingPreviewAsync(PricingPreviewRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var item = await _uow.Repository<Item>().Query()
            .Include(i => i.Discount)
            .Include(i => i.TaxProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ItemId == request.ItemId && i.IsActive, ct);

        if (item is null)
        {
            return null;
        }

        var segmentPrice = await _uow.Repository<CustomerSegmentPrice>().Query()
            .AsNoTracking()
            .Where(x => x.ItemId == request.ItemId && x.Segment == request.Segment && x.IsActive)
            .Where(x => x.ValidFrom == null || x.ValidFrom <= now)
            .Where(x => x.ValidTo == null || x.ValidTo >= now)
            .OrderByDescending(x => x.DateCreated)
            .FirstOrDefaultAsync(ct);

        var unitBase = item.UnitPrice;
        var segmentUnit = segmentPrice?.PriceOverride ?? unitBase;

        decimal discountPerUnit = 0;
        if (item.Discount is not null && item.Discount.IsActive)
        {
            var within = (item.Discount.ValidFrom == null || item.Discount.ValidFrom <= now)
                && (item.Discount.ValidTo == null || item.Discount.ValidTo >= now);
            if (within)
            {
                discountPerUnit = Math.Round(segmentUnit * (item.Discount.Percentage / 100m), 4);
            }
        }

        var bundleRules = await _uow.Repository<BundleRule>().Query()
            .AsNoTracking()
            .Where(b => b.IsActive && b.TriggerItemId == request.ItemId)
            .Where(b => b.ValidFrom == null || b.ValidFrom <= now)
            .Where(b => b.ValidTo == null || b.ValidTo >= now)
            .ToListAsync(ct);

        decimal bundleDiscount = 0;
        foreach (var rule in bundleRules)
        {
            if (request.Quantity >= rule.TriggerQuantity && rule.RewardItemId == request.ItemId)
            {
                var bundleCount = request.Quantity / rule.TriggerQuantity;
                var rewardedUnits = bundleCount * rule.RewardQuantity;
                var perUnitBundleDiscount = segmentUnit * (rule.RewardDiscountPercent / 100m);
                bundleDiscount += rewardedUnits * perUnitBundleDiscount;
            }
        }

        var preTaxSubtotal = (segmentUnit - discountPerUnit) * request.Quantity;
        preTaxSubtotal = Math.Max(preTaxSubtotal - bundleDiscount, 0);

        var taxRate = item.TaxProfile?.IsActive == true ? item.TaxProfile.RatePercent : 0;
        var taxAmount = 0m;
        var grandTotal = preTaxSubtotal;

        if (taxRate > 0)
        {
            if (item.TaxProfile?.ApplicationType == TaxApplicationType.Exclusive)
            {
                taxAmount = Math.Round(preTaxSubtotal * (taxRate / 100m), 2);
                grandTotal = preTaxSubtotal + taxAmount;
            }
            else
            {
                taxAmount = Math.Round(preTaxSubtotal - (preTaxSubtotal / (1 + (taxRate / 100m))), 2);
            }
        }

        return new PricingPreviewDto
        {
            ItemId = item.ItemId,
            ItemName = item.Name,
            BaseUnitPrice = unitBase,
            TaxRatePercent = taxRate,
            SegmentPrice = segmentUnit,
            DiscountPerUnit = Math.Round(discountPerUnit, 2),
            BundleDiscountTotal = Math.Round(bundleDiscount, 2),
            Subtotal = Math.Round(preTaxSubtotal, 2),
            TaxAmount = taxAmount,
            GrandTotal = Math.Round(grandTotal, 2)
        };
    }

    public async Task<CashierShiftDto> OpenShiftAsync(ShiftOpenRequest request, Guid actingUserId, CancellationToken ct = default)
    {
        var existingOpen = await _uow.Repository<CashierShift>().Query()
            .FirstOrDefaultAsync(x => x.OpenedByUserId == actingUserId && x.Status == ShiftStatus.Open, ct);

        if (existingOpen is not null)
        {
            throw new InvalidOperationException("User already has an open shift.");
        }

        var shift = new CashierShift
        {
            CashierShiftId = Guid.NewGuid(),
            OpenedByUserId = actingUserId,
            OpenedAtUtc = DateTime.UtcNow,
            OpeningFloat = request.OpeningFloat,
            Notes = request.Notes,
            Status = ShiftStatus.Open
        };

        await _uow.Repository<CashierShift>().AddAsync(shift, ct);
        await _uow.SaveChangesAsync(ct);

        return MapShift(shift);
    }

    public async Task<CashierShiftDto?> GetActiveShiftAsync(Guid actingUserId, CancellationToken ct = default)
    {
        var shift = await _uow.Repository<CashierShift>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OpenedByUserId == actingUserId && x.Status == ShiftStatus.Open, ct);

        return shift is null ? null : MapShift(shift);
    }

    public async Task<CashierShiftDto?> CloseShiftAsync(ShiftCloseRequest request, Guid actingUserId, CancellationToken ct = default)
    {
        var shift = await _uow.Repository<CashierShift>().Query()
            .FirstOrDefaultAsync(x => x.OpenedByUserId == actingUserId && x.Status == ShiftStatus.Open, ct);

        if (shift is null)
        {
            return null;
        }

        var cashSales = await _uow.Repository<Invoice>().Query()
            .Where(x => x.DateCreated >= shift.OpenedAtUtc && x.IsPaid && x.PaymentType == PaymentType.Cash)
            .SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0m;

        shift.ClosedAtUtc = DateTime.UtcNow;
        shift.ClosedByUserId = actingUserId;
        shift.ClosingFloat = request.ClosingFloat;
        shift.ExpectedClosingAmount = shift.OpeningFloat + cashSales;
        shift.VarianceAmount = request.ClosingFloat - shift.ExpectedClosingAmount;
        shift.Notes = string.IsNullOrWhiteSpace(request.Notes)
            ? shift.Notes
            : request.Notes.Trim();
        shift.Status = ShiftStatus.Closed;

        _uow.Repository<CashierShift>().Update(shift);
        await _uow.SaveChangesAsync(ct);

        return MapShift(shift);
    }

    public async Task<DailyZReportDto> GetDailyZReportAsync(DateTime dateUtc, CancellationToken ct = default)
    {
        var from = dateUtc.Date;
        var to = from.AddDays(1);

        var invoices = await _uow.Repository<Invoice>().Query()
            .AsNoTracking()
            .Where(x => x.DateCreated >= from && x.DateCreated < to && x.IsPaid)
            .ToListAsync(ct);

        var invoiceIds = invoices.Select(i => i.InvoiceId).ToHashSet();

        var sales = await _uow.Repository<Sale>().Query()
            .AsNoTracking()
            .Where(x => invoiceIds.Contains(x.InvoiceId))
            .ToListAsync(ct);

        var itemCostMap = await _uow.Repository<Item>().Query()
            .AsNoTracking()
            .Select(i => new { i.ItemId, Cost = i.CostPrice ?? 0m })
            .ToDictionaryAsync(x => x.ItemId, x => x.Cost, ct);

        var grossSales = invoices.Sum(x => x.TotalAmount);
        var discounts = sales.Sum(x => (x.DiscountAmount ?? 0m) * x.Quantity);
        var netSales = grossSales - discounts;
        var cogs = sales.Sum(x => itemCostMap.TryGetValue(x.ItemId, out var c) ? c * x.Quantity : 0m);
        var grossMargin = netSales - cogs;

        var paymentBreakdown = invoices
            .GroupBy(x => x.PaymentType)
            .Select(g => new PaymentBreakdownDto
            {
                PaymentType = g.Key,
                TotalAmount = g.Sum(x => x.TotalAmount),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        var topProducts = sales
            .GroupBy(x => new { x.ItemId, x.ItemName })
            .Select(g =>
            {
                var qty = g.Sum(x => x.Quantity);
                var revenue = g.Sum(x => x.LineTotal);
                var unitCost = itemCostMap.TryGetValue(g.Key.ItemId, out var c) ? c : 0m;
                var margin = revenue - (unitCost * qty);
                return new TopProductDto
                {
                    ItemId = g.Key.ItemId,
                    ItemName = g.Key.ItemName,
                    QuantitySold = qty,
                    Revenue = revenue,
                    GrossMargin = margin
                };
            })
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        return new DailyZReportDto
        {
            Date = from,
            GrossSales = Math.Round(grossSales, 2),
            Discounts = Math.Round(discounts, 2),
            NetSales = Math.Round(netSales, 2),
            Cogs = Math.Round(cogs, 2),
            GrossMargin = Math.Round(grossMargin, 2),
            InvoiceCount = invoices.Count,
            AverageBasket = invoices.Count == 0 ? 0 : Math.Round(grossSales / invoices.Count, 2),
            PaymentBreakdown = paymentBreakdown,
            TopProducts = topProducts
        };
    }

    public async Task<IReadOnlyList<RoleMatrixDto>> GetRoleMatrixAsync(CancellationToken ct = default)
    {
        var roles = await _uow.Repository<Role>().Query().AsNoTracking().OrderBy(r => r.RoleId).ToListAsync(ct);
        var permissions = await _uow.Repository<RolePermission>().Query().AsNoTracking().ToListAsync(ct);

        var list = new List<RoleMatrixDto>(roles.Count);
        foreach (var role in roles)
        {
            var dict = PermissionKeys.All.ToDictionary(x => x, _ => false);
            foreach (var p in permissions.Where(x => x.RoleId == role.RoleId))
            {
                dict[p.PermissionKey] = p.IsAllowed;
            }

            list.Add(new RoleMatrixDto
            {
                RoleId = role.RoleId,
                RoleName = role.Name,
                Permissions = dict
            });
        }

        return list;
    }

    public async Task<RolePermissionDto> UpdateRolePermissionAsync(UpdateRolePermissionRequest request, CancellationToken ct = default)
    {
        var role = await _uow.Repository<Role>().GetByIdAsync(request.RoleId, ct)
            ?? throw new InvalidOperationException("Role not found.");

        var existing = await _uow.Repository<RolePermission>().Query()
            .FirstOrDefaultAsync(x => x.RoleId == request.RoleId && x.PermissionKey == request.PermissionKey, ct);

        if (existing is null)
        {
            existing = new RolePermission
            {
                RoleId = request.RoleId,
                PermissionKey = request.PermissionKey,
                IsAllowed = request.IsAllowed
            };
            await _uow.Repository<RolePermission>().AddAsync(existing, ct);
        }
        else
        {
            existing.IsAllowed = request.IsAllowed;
            _uow.Repository<RolePermission>().Update(existing);
        }

        await _uow.SaveChangesAsync(ct);

        return new RolePermissionDto
        {
            RolePermissionId = existing.RolePermissionId,
            RoleId = existing.RoleId,
            RoleName = role.Name,
            PermissionKey = existing.PermissionKey,
            IsAllowed = existing.IsAllowed
        };
    }

    private async Task AddChangeLogAsync(
        Guid? actingUserId,
        string entityName,
        string entityId,
        ChangeLogAction action,
        string? oldValues,
        string? newValues,
        CancellationToken ct)
    {
        if (!actingUserId.HasValue)
        {
            return;
        }

        var log = new ChangeLog
        {
            UserId = actingUserId.Value,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = null
        };

        await _uow.Repository<ChangeLog>().AddAsync(log, ct);
    }

    private static CashierShiftDto MapShift(CashierShift x) => new()
    {
        CashierShiftId = x.CashierShiftId,
        OpenedByUserId = x.OpenedByUserId,
        ClosedByUserId = x.ClosedByUserId,
        OpenedAtUtc = x.OpenedAtUtc,
        ClosedAtUtc = x.ClosedAtUtc,
        OpeningFloat = x.OpeningFloat,
        ClosingFloat = x.ClosingFloat,
        ExpectedClosingAmount = x.ExpectedClosingAmount,
        VarianceAmount = x.VarianceAmount,
        Status = x.Status,
        Notes = x.Notes
    };

    // ─── Promotion Effectiveness ──────────────────────────────────────────────

    public async Task<PromotionEffectivenessDto> GetPromotionEffectivenessAsync(DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct = default)
    {
        var toInclusive = toDateUtc.Date.AddDays(1);

        // Sales in date range with discount amounts
        var sales = await _uow.Repository<Sale>().Query()
            .Include(s => s.Invoice)
            .Include(s => s.Item).ThenInclude(i => i.Category)
            .Include(s => s.Item).ThenInclude(i => i.Discount)
            .AsNoTracking()
            .Where(s => s.Invoice.DateCreated >= fromDateUtc && s.Invoice.DateCreated < toInclusive)
            .ToListAsync(ct);

        // Total discount given across all lines
        var totalDiscountGiven = sales.Sum(s => s.DiscountAmount ?? 0m);
        var invoicesWithDiscount = sales
            .Where(s => s.DiscountAmount.HasValue && s.DiscountAmount.Value > 0)
            .Select(s => s.InvoiceId)
            .Distinct()
            .Count();

        // Items with active discounts — summarise sales
        var discountedItemIds = sales
            .Where(s => s.Item.Discount is not null && s.Item.Discount.IsActive)
            .Select(s => s.ItemId)
            .Distinct()
            .ToHashSet();

        var topDiscountedItems = sales
            .Where(s => discountedItemIds.Contains(s.ItemId))
            .GroupBy(s => new { s.ItemId, s.ItemName, s.Item.Category?.Name })
            .Select(g => new ItemDiscountSummaryDto
            {
                ItemId = g.Key.ItemId,
                ItemName = g.Key.ItemName,
                CategoryName = g.Key.Name,
                DiscountPercent = g.First().Item.Discount?.Percentage ?? 0m,
                UnitsSold = g.Sum(s => s.Quantity),
                TotalRevenue = g.Sum(s => s.LineTotal),
                TotalDiscountGiven = g.Sum(s => s.DiscountAmount ?? 0m)
            })
            .OrderByDescending(x => x.TotalDiscountGiven)
            .Take(20)
            .ToList();

        // Bundle rule hits — count invoices that included the trigger item
        var bundleRules = await _uow.Repository<BundleRule>().Query()
            .Include(b => b.TriggerItem)
            .Include(b => b.RewardItem)
            .AsNoTracking()
            .ToListAsync(ct);

        var invoiceTriggerItems = sales
            .GroupBy(s => s.InvoiceId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.ItemId).ToHashSet());

        var bundleHits = bundleRules.Select(b => new BundleHitSummaryDto
        {
            BundleRuleId = b.BundleRuleId,
            BundleName = b.Name,
            TriggerItemName = b.TriggerItem.Name,
            RewardItemName = b.RewardItem.Name,
            RewardDiscountPercent = b.RewardDiscountPercent,
            TriggerInvoiceCount = invoiceTriggerItems.Values.Count(itemSet => itemSet.Contains(b.TriggerItemId))
        })
        .OrderByDescending(x => x.TriggerInvoiceCount)
        .ToList();

        // Segment pricing effectiveness — sales of segment-priced items
        var segmentPrices = await _uow.Repository<CustomerSegmentPrice>().Query()
            .Include(sp => sp.Item).ThenInclude(i => i.Category)
            .AsNoTracking()
            .Where(sp => sp.IsActive)
            .ToListAsync(ct);

        var salesByItemId = sales.GroupBy(s => s.ItemId).ToDictionary(g => g.Key, g => g.ToList());

        var segmentSummary = segmentPrices.Select(sp =>
        {
            var itemSales = salesByItemId.GetValueOrDefault(sp.ItemId, new List<Sale>());
            return new SegmentEffectivenessSummaryDto
            {
                Segment = sp.Segment.ToString(),
                ItemName = sp.Item.Name,
                CategoryName = sp.Item.Category?.Name,
                StandardPrice = sp.Item.UnitPrice,
                SegmentPrice = sp.PriceOverride,
                UnitsSold = itemSales.Sum(s => s.Quantity),
                TotalRevenue = itemSales.Sum(s => s.LineTotal)
            };
        })
        .OrderByDescending(x => x.UnitsSold)
        .ToList();

        return new PromotionEffectivenessDto
        {
            FromDate = fromDateUtc,
            ToDate = toDateUtc,
            TotalDiscountGiven = totalDiscountGiven,
            InvoicesWithDiscount = invoicesWithDiscount,
            TopDiscountedItems = topDiscountedItems,
            BundleHits = bundleHits,
            SegmentSummary = segmentSummary
        };
    }

    // ---- Branch management ----

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken ct = default)
    {
        var branches = await _uow.Repository<Branch>().Query()
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        return branches.Select(b => new BranchDto
        {
            BranchId = b.BranchId,
            Name = b.Name,
            Code = b.Code,
            Address = b.Address,
            IsActive = b.IsActive
        }).ToList();
    }

    public async Task<BranchDto> UpsertBranchAsync(UpsertBranchRequest request, CancellationToken ct = default)
    {
        Branch branch;
        if (request.BranchId.HasValue)
        {
            branch = await _uow.Repository<Branch>().Query()
                .FirstOrDefaultAsync(b => b.BranchId == request.BranchId.Value, ct)
                ?? throw new KeyNotFoundException($"Branch {request.BranchId} not found.");
            branch.Name = request.Name;
            branch.Code = request.Code;
            branch.Address = request.Address;
            branch.IsActive = request.IsActive;
            _uow.Repository<Branch>().Update(branch);
        }
        else
        {
            branch = new Branch
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                IsActive = request.IsActive
            };
            await _uow.Repository<Branch>().AddAsync(branch, ct);
        }
        await _uow.SaveChangesAsync(ct);
        return new BranchDto
        {
            BranchId = branch.BranchId,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            IsActive = branch.IsActive
        };
    }

    public async Task<IReadOnlyList<UserBranchRoleDto>> GetUserBranchRolesAsync(int? branchId, Guid? userId, CancellationToken ct = default)
    {
        var query = _uow.Repository<UserBranchRole>().Query()
            .Include(x => x.User)
            .Include(x => x.Branch)
            .Include(x => x.Role)
            .AsNoTracking();

        if (branchId.HasValue) query = query.Where(x => x.BranchId == branchId.Value);
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);

        var rows = await query.OrderBy(x => x.Branch.Name).ThenBy(x => x.User.Username).ToListAsync(ct);

        return rows.Select(x => new UserBranchRoleDto
        {
            UserBranchRoleId = x.UserBranchRoleId,
            UserId = x.UserId,
            UserName = x.User.Username,
            BranchId = x.BranchId,
            BranchName = x.Branch.Name,
            RoleId = x.RoleId,
            RoleName = x.Role.Name
        }).ToList();
    }

    public async Task<UserBranchRoleDto> AssignUserBranchRoleAsync(AssignUserBranchRoleRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.Repository<UserBranchRole>().Query()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.BranchId == request.BranchId && x.RoleId == request.RoleId, ct);

        if (existing is not null)
        {
            var existingBranch = await _uow.Repository<Branch>().Query().AsNoTracking().FirstAsync(b => b.BranchId == existing.BranchId, ct);
            var existingUser = await _uow.Repository<User>().Query().AsNoTracking().FirstAsync(u => u.UserId == existing.UserId, ct);
            var existingRole = await _uow.Repository<Role>().Query().AsNoTracking().FirstAsync(r => r.RoleId == existing.RoleId, ct);
            return new UserBranchRoleDto
            {
                UserBranchRoleId = existing.UserBranchRoleId,
                UserId = existing.UserId,
                UserName = existingUser.Username,
                BranchId = existing.BranchId,
                BranchName = existingBranch.Name,
                RoleId = existing.RoleId,
                RoleName = existingRole.Name
            };
        }

        var ubr = new UserBranchRole
        {
            UserId = request.UserId,
            BranchId = request.BranchId,
            RoleId = request.RoleId
        };
        await _uow.Repository<UserBranchRole>().AddAsync(ubr, ct);
        await _uow.SaveChangesAsync(ct);

        var branch = await _uow.Repository<Branch>().Query().AsNoTracking().FirstAsync(b => b.BranchId == ubr.BranchId, ct);
        var user = await _uow.Repository<User>().Query().AsNoTracking().FirstAsync(u => u.UserId == ubr.UserId, ct);
        var role = await _uow.Repository<Role>().Query().AsNoTracking().FirstAsync(r => r.RoleId == ubr.RoleId, ct);

        return new UserBranchRoleDto
        {
            UserBranchRoleId = ubr.UserBranchRoleId,
            UserId = ubr.UserId,
            UserName = user.Username,
            BranchId = ubr.BranchId,
            BranchName = branch.Name,
            RoleId = ubr.RoleId,
            RoleName = role.Name
        };
    }

    public async Task<bool> RemoveUserBranchRoleAsync(long userBranchRoleId, CancellationToken ct = default)
    {
        var ubr = await _uow.Repository<UserBranchRole>().Query()
            .FirstOrDefaultAsync(x => x.UserBranchRoleId == userBranchRoleId, ct);
        if (ubr is null) return false;
        _uow.Repository<UserBranchRole>().Remove(ubr);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
