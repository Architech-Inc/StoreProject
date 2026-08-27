using Microsoft.AspNetCore.Http;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;

namespace StoreUI.Services;

public interface IEmployeeManager
{
    Task<EmployeeDto?> CreateEmployeeAsync(
        string firstName, string? middleName, string lastName, 
        string gender, DateTime? dob, DateTime dateEmployed, 
        int? departmentId, IFormFile? imageUpload, 
        int? cropX, int? cropY, int? cropW, int? cropH, 
        CancellationToken ct = default);

    Task<EmployeeDto?> UpdateEmployeeAsync(
        Guid employeeId, string firstName, string? middleName, string lastName, 
        string gender, DateTime? dob, int? departmentId, string status, 
        IFormFile? imageUpload, int? cropX, int? cropY, int? cropW, int? cropH, 
        CancellationToken ct = default);

    Task<Employee360Dto?> Get360ByIdAsync(Guid employeeId, CancellationToken ct = default);
    Task<bool> TerminateOrDeleteEmployeeAsync(Guid employeeId, CancellationToken ct = default);
    Task<bool> ReinstateEmployeeAsync(Guid employeeId, CancellationToken ct = default);
}
