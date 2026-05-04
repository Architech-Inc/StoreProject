using Store.Models.DTOs.Discounts;

namespace Store.Models.Interfaces.Services;

public interface IDiscountService
{
    Task<List<DiscountDto>> GetAllAsync(bool? activeOnly = null, string? couponCode = null);
    Task<DiscountDto?> GetByIdAsync(int id);
    Task<DiscountDto> CreateAsync(CreateDiscountRequest request, Guid managedByUserId);
    Task<DiscountDto?> UpdateAsync(int id, UpdateDiscountRequest request);
    Task<bool> DeleteAsync(int id);

    /// <summary>Validates a coupon code and returns the discount if valid.</summary>
    Task<DiscountDto?> ValidateCouponAsync(string couponCode);

    /// <summary>Increment UsedCount after a coupon is applied.</summary>
    Task IncrementUsageAsync(int discountId);
}
