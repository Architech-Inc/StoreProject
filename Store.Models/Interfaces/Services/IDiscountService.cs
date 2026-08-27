using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;

namespace Store.Models.Interfaces.Services;

public interface IDiscountService
{
    Task<DiscountMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<DiscountDto>> GetDiscountsPagedAsync(DiscountFilterRequest request, CancellationToken ct = default);
    Task<List<DiscountDto>> GetAllAsync(bool? activeOnly = null, string? couponCode = null);
    Task<DiscountDto?> GetByIdAsync(int id);
    Task<DiscountDto> CreateAsync(CreateDiscountRequest request, Guid managedByUserId);
    Task<DiscountDto?> UpdateAsync(int id, UpdateDiscountRequest request);
    Task<bool> DeleteAsync(int id);
    Task<DiscountDto?> ValidateCouponAsync(string couponCode);
    Task IncrementUsageAsync(int discountId);
    Task<DiscountSimulationResult> SimulateDiscountAsync(DiscountSimulationRequest request, CancellationToken ct = default);
}
