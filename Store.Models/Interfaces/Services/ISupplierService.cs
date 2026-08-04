using Store.Models.DTOs.Procurement;

namespace Store.Models.Interfaces.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync(string? search = null, string? city = null, string? country = null, string? sortBy = null);
    Task<SupplierDto?> GetByIdAsync(Guid id);
    Task<SupplierProfileDto?> GetProfileAsync(Guid id);
    Task<SupplierMetricsDto> GetMetricsAsync();
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, Guid createdByUserId);
    Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(Guid id);
}