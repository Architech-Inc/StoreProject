using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;

namespace Store.Models.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct = default);
    Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid customerId, CancellationToken ct = default);
}
