using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;

namespace Store.Models.Interfaces.Services;

public interface IEmployeeService
{
    Task<EmployeeDto?> GetByIdAsync(Guid employeeId, CancellationToken ct = default);
    Task<PagedResult<EmployeeDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeDto?> UpdateAsync(Guid employeeId, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid employeeId, CancellationToken ct = default);
}
