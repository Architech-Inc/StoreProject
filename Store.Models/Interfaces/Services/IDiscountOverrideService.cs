using Store.Models.DTOs.Discounts;

namespace Store.Models.Interfaces.Services;

public interface IDiscountOverrideService
{
    /// <summary>Get override requests. Optionally filter by status ("Pending", "Approved", etc.).</summary>
    Task<List<DiscountOverrideDto>> GetAllAsync(string? status = null);

    Task<DiscountOverrideDto?> GetByIdAsync(int id);

    /// <summary>Cashier/operator submits an override request.</summary>
    Task<DiscountOverrideDto> CreateAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId);

    /// <summary>Manager approves or rejects the request.</summary>
    Task<DiscountOverrideDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request);

    /// <summary>Requester cancels their own pending request.</summary>
    Task<bool> CancelAsync(int id, Guid userId);
}
