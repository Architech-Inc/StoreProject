using Microsoft.EntityFrameworkCore;
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
            ManagedByUserId = managedByUserId
        };

        await _uow.Repository<Discount>().AddAsync(discount);
        await _uow.SaveChangesAsync();
        return MapToDto(discount);
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
        // Use atomic UPDATE to avoid read-modify-write race when multiple POS
        // terminals redeem the same coupon concurrently.
        await _uow.Repository<Discount>().Query()
            .Where(d => d.DiscountId == discountId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.UsedCount, d => d.UsedCount + 1));
    }

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
                               (d.MaxUses == null || d.UsedCount < d.MaxUses)
        };
    }
}
