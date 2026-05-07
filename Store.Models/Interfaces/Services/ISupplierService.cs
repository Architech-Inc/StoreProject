using Store.Models.DTOs.Procurement;

namespace Store.Models.Interfaces.Services;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(Guid id);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, Guid createdByUserId);
    Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(Guid id);
}