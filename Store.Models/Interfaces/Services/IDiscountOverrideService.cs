using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;

namespace Store.Models.Interfaces.Services;

public interface IDiscountOverrideService
{
    Task<DiscountOverrideMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<DiscountOverrideDto>> GetOverridesPagedAsync(DiscountOverrideFilterRequest request, CancellationToken ct = default);
    Task<List<DiscountOverrideDto>> GetAllAsync(string? status = null);
    Task<DiscountOverrideDto?> GetByIdAsync(int id);
    Task<DiscountOverrideDto> CreateAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId);
    Task<DiscountOverrideDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request);
    Task<bool> CancelAsync(int id, Guid userId);
}
