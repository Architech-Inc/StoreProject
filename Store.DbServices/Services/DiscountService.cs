using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class DiscountService : IDiscountService
{
    private readonly IUnitOfWork _uow;

    public DiscountService(IUnitOfWork uow) => _uow = uow;

    public async Task<DiscountMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var discounts = await _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .ToListAsync(ct);

        var now = DateTime.UtcNow;

        return new DiscountMetricsDto
        {
            TotalRules = discounts.Count,
            ActiveRulesCount = discounts.Count(d => d.IsActive &&
                                                    (d.ValidFrom == null || d.ValidFrom <= now) &&
                                                    (d.ValidTo == null || d.ValidTo >= now) &&
                                                    (d.MaxUses == null || d.UsedCount < d.MaxUses)),
            CouponCampaignsCount = discounts.Count(d => !string.IsNullOrWhiteSpace(d.CouponCode)),
            SegmentRulesCount = discounts.Count(d => d.TargetSegment != null),
            TotalRedemptionsCount = discounts.Sum(d => d.UsedCount)
        };
    }

    public async Task<PagedResult<DiscountDto>> GetDiscountsPagedAsync(DiscountFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .AsQueryable();

        var now = DateTime.UtcNow;

        if (request.ActiveOnly == true)
        {
            query = query.Where(d => d.IsActive &&
                                     (d.ValidFrom == null || d.ValidFrom <= now) &&
                                     (d.ValidTo == null || d.ValidTo >= now) &&
                                     (d.MaxUses == null || d.UsedCount < d.MaxUses));
        }

        if (!string.IsNullOrWhiteSpace(request.DiscountType) &&
            Enum.TryParse<DiscountType>(request.DiscountType, ignoreCase: true, out var dt))
        {
            query = query.Where(d => d.DiscountType == dt);
        }

        if (!string.IsNullOrWhiteSpace(request.TargetSegment) &&
            Enum.TryParse<CustomerSegment>(request.TargetSegment, ignoreCase: true, out var seg))
        {
            query = query.Where(d => d.TargetSegment == seg);
        }

        if (request.HasCoupon.HasValue)
        {
            query = request.HasCoupon.Value
                ? query.Where(d => d.CouponCode != null && d.CouponCode != "")
                : query.Where(d => d.CouponCode == null || d.CouponCode == "");
        }

        if (!string.IsNullOrWhiteSpace(request.Scope))
        {
            if (request.Scope.Equals("Item", StringComparison.OrdinalIgnoreCase))
                query = query.Where(d => d.ItemId != null);
            else if (request.Scope.Equals("Category", StringComparison.OrdinalIgnoreCase))
                query = query.Where(d => d.CategoryId != null && d.ItemId == null);
            else if (request.Scope.Equals("StoreWide", StringComparison.OrdinalIgnoreCase))
                query = query.Where(d => d.ItemId == null && d.CategoryId == null);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(d => d.Name.Contains(term) ||
                                     (d.CouponCode != null && d.CouponCode.Contains(term)) ||
                                     (d.Item != null && (d.Item.Name.Contains(term) || (d.Item.Barcode != null && d.Item.Barcode.Contains(term)))) ||
                                     (d.Category != null && d.Category.Name.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var pagedRows = await query
            .OrderByDescending(d => d.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DiscountDto>
        {
            Items = pagedRows.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<DiscountDto>> GetAllAsync(bool? activeOnly = null, string? couponCode = null)
    {
        var query = _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .AsQueryable();

        if (activeOnly == true)
            query = query.Where(d => d.IsActive);

        if (!string.IsNullOrWhiteSpace(couponCode))
            query = query.Where(d => d.CouponCode == couponCode);

        var discounts = await query.OrderBy(d => d.Name).ToListAsync();
        return discounts.Select(MapToDto).ToList();
    }

    public async Task<DiscountDto?> GetByIdAsync(int id)
    {
        var discount = await _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.DiscountId == id);

        return discount is null ? null : MapToDto(discount);
    }

    public async Task<DiscountDto> CreateAsync(CreateDiscountRequest request, Guid managedByUserId)
    {
        var discount = new Discount
        {
            Name = request.Name.Trim(),
            DiscountType = request.DiscountType,
            Percentage = request.Percentage,
            FixedAmount = request.FixedAmount,
            ItemId = request.ItemId,
            CategoryId = request.CategoryId,
            MinQuantity = request.MinQuantity,
            TargetSegment = request.TargetSegment,
            CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant(),
            MaxUses = request.MaxUses,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = request.IsActive,
            ManagedByUserId = managedByUserId != Guid.Empty ? managedByUserId : null
        };

        await _uow.Repository<Discount>().AddAsync(discount);
        await _uow.SaveChangesAsync();

        var loaded = await _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.DiscountId == discount.DiscountId);

        return MapToDto(loaded ?? discount);
    }

    public async Task<DiscountDto?> UpdateAsync(int id, UpdateDiscountRequest request)
    {
        var discount = await _uow.Repository<Discount>().Query()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d => d.DiscountId == id);

        if (discount is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name)) discount.Name = request.Name.Trim();
        if (request.DiscountType.HasValue) discount.DiscountType = request.DiscountType.Value;
        if (request.Percentage.HasValue) discount.Percentage = request.Percentage.Value;
        if (request.FixedAmount.HasValue) discount.FixedAmount = request.FixedAmount;
        if (request.ClearItemId) discount.ItemId = null;
        else if (request.ItemId.HasValue) discount.ItemId = request.ItemId;
        if (request.ClearCategoryId) discount.CategoryId = null;
        else if (request.CategoryId.HasValue) discount.CategoryId = request.CategoryId;
        if (request.MinQuantity.HasValue) discount.MinQuantity = request.MinQuantity.Value;
        if (request.TargetSegment.HasValue) discount.TargetSegment = request.TargetSegment;
        if (request.CouponCode is not null)
            discount.CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant();
        if (request.MaxUses.HasValue) discount.MaxUses = request.MaxUses;
        if (request.ValidFrom.HasValue) discount.ValidFrom = request.ValidFrom;
        if (request.ValidTo.HasValue) discount.ValidTo = request.ValidTo;
        if (request.IsActive.HasValue) discount.IsActive = request.IsActive.Value;

        _uow.Repository<Discount>().Update(discount);
        await _uow.SaveChangesAsync();
        return MapToDto(discount);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var discount = await _uow.Repository<Discount>().Query()
            .FirstOrDefaultAsync(d => d.DiscountId == id);

        if (discount is null) return false;

        _uow.Repository<Discount>().Remove(discount);
        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<DiscountDto?> ValidateCouponAsync(string couponCode)
    {
        var code = couponCode.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        var discount = await _uow.Repository<Discount>().Query()
            .AsNoTracking()
            .Include(d => d.Item)
            .Include(d => d.Category)
            .FirstOrDefaultAsync(d =>
                d.CouponCode == code &&
                d.IsActive &&
                (d.ValidFrom == null || d.ValidFrom <= now) &&
                (d.ValidTo == null || d.ValidTo >= now) &&
                (d.MaxUses == null || d.UsedCount < d.MaxUses));

        return discount is null ? null : MapToDto(discount);
    }

    public async Task IncrementUsageAsync(int discountId)
    {
        await _uow.Repository<Discount>().Query()
            .Where(d => d.DiscountId == discountId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.UsedCount, d => d.UsedCount + 1));
    }

    public async Task<DiscountSimulationResult> SimulateDiscountAsync(DiscountSimulationRequest request, CancellationToken ct = default)
    {
        Discount? discount = null;
        if (request.DiscountId.HasValue)
        {
            discount = await _uow.Repository<Discount>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DiscountId == request.DiscountId.Value, ct);
        }
        else if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var code = request.CouponCode.Trim().ToUpperInvariant();
            discount = await _uow.Repository<Discount>().Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.CouponCode == code, ct);
        }

        var origTotal = request.ItemUnitPrice * request.Quantity;

        if (discount == null)
        {
            return new DiscountSimulationResult
            {
                IsEligible = false,
                IneligibilityReason = "No matching discount rule or coupon found.",
                OriginalUnitPrice = request.ItemUnitPrice,
                OriginalTotalPrice = origTotal,
                EffectiveUnitPrice = request.ItemUnitPrice,
                FinalTotalPriceXaf = origTotal,
                TotalDiscountAmountXaf = 0,
                SavingsPercentage = 0
            };
        }

        var now = DateTime.UtcNow;
        if (!discount.IsActive)
        {
            return BuildIneligible(discount, request, origTotal, "Discount rule is currently disabled / inactive.");
        }

        if (discount.ValidFrom.HasValue && discount.ValidFrom.Value > now)
        {
            return BuildIneligible(discount, request, origTotal, $"Discount rule is not yet valid (starts on {discount.ValidFrom.Value:yyyy-MM-dd}).");
        }

        if (discount.ValidTo.HasValue && discount.ValidTo.Value < now)
        {
            return BuildIneligible(discount, request, origTotal, $"Discount rule expired on {discount.ValidTo.Value:yyyy-MM-dd}.");
        }

        if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
        {
            return BuildIneligible(discount, request, origTotal, "Coupon redemptions cap has been reached.");
        }

        if (request.Quantity < discount.MinQuantity)
        {
            return BuildIneligible(discount, request, origTotal, $"Requires minimum quantity of {discount.MinQuantity} units (provided: {request.Quantity}).");
        }

        if (discount.TargetSegment.HasValue && request.CustomerSegment.HasValue && discount.TargetSegment.Value != request.CustomerSegment.Value)
        {
            return BuildIneligible(discount, request, origTotal, $"Targeted exclusively to {discount.TargetSegment.Value} customer tier.");
        }

        decimal discountTotal = 0;
        if (discount.DiscountType == DiscountType.Percentage)
        {
            discountTotal = Math.Round(origTotal * (discount.Percentage / 100m), 2);
        }
        else if (discount.DiscountType == DiscountType.FixedAmount)
        {
            var unitDiscount = discount.FixedAmount ?? 0;
            discountTotal = Math.Min(origTotal, unitDiscount * request.Quantity);
        }

        var finalTotal = Math.Max(0, origTotal - discountTotal);
        var effectiveUnit = request.Quantity > 0 ? Math.Round(finalTotal / request.Quantity, 2) : 0;
        var savingsPct = origTotal > 0 ? Math.Round((discountTotal / origTotal) * 100m, 2) : 0;

        return new DiscountSimulationResult
        {
            IsEligible = true,
            RuleName = discount.Name,
            DiscountType = discount.DiscountType.ToString(),
            OriginalUnitPrice = request.ItemUnitPrice,
            OriginalTotalPrice = origTotal,
            TotalDiscountAmountXaf = discountTotal,
            EffectiveUnitPrice = effectiveUnit,
            FinalTotalPriceXaf = finalTotal,
            SavingsPercentage = savingsPct
        };
    }

    private static DiscountSimulationResult BuildIneligible(Discount d, DiscountSimulationRequest req, decimal origTotal, string reason) => new()
    {
        IsEligible = false,
        IneligibilityReason = reason,
        RuleName = d.Name,
        DiscountType = d.DiscountType.ToString(),
        OriginalUnitPrice = req.ItemUnitPrice,
        OriginalTotalPrice = origTotal,
        EffectiveUnitPrice = req.ItemUnitPrice,
        FinalTotalPriceXaf = origTotal,
        TotalDiscountAmountXaf = 0,
        SavingsPercentage = 0
    };

    private static DiscountDto MapToDto(Discount d)
    {
        var now = DateTime.UtcNow;
        return new DiscountDto
        {
            DiscountId = d.DiscountId,
            Name = d.Name,
            DiscountType = d.DiscountType.ToString(),
            Percentage = d.Percentage,
            FixedAmount = d.FixedAmount,
            ItemId = d.ItemId,
            ItemName = d.Item?.Name,
            ItemBarcode = d.Item?.Barcode,
            CategoryId = d.CategoryId,
            CategoryName = d.Category?.Name,
            MinQuantity = d.MinQuantity,
            TargetSegment = d.TargetSegment?.ToString(),
            CouponCode = d.CouponCode,
            MaxUses = d.MaxUses,
            UsedCount = d.UsedCount,
            ValidFrom = d.ValidFrom,
            ValidTo = d.ValidTo,
            IsActive = d.IsActive,
            IsCurrentlyValid = d.IsActive &&
                               (d.ValidFrom == null || d.ValidFrom <= now) &&
                               (d.ValidTo == null || d.ValidTo >= now) &&
                               (d.MaxUses == null || d.UsedCount < d.MaxUses),
            DateCreated = d.DateCreated
        };
    }
}
