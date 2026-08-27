using Microsoft.AspNetCore.Http;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class EmployeeManager : IEmployeeManager
{
    private readonly IEmployeeService _employeeService;
    private readonly IFileService _fileService;

    public EmployeeManager(IEmployeeService employeeService, IFileService fileService)
    {
        _employeeService = employeeService;
        _fileService = fileService;
    }

    public async Task<EmployeeDto?> CreateEmployeeAsync(
        string firstName, string? middleName, string lastName, 
        string gender, DateTime? dob, DateTime dateEmployed, 
        int? departmentId, IFormFile? imageUpload, 
        int? cropX, int? cropY, int? cropW, int? cropH, 
        CancellationToken ct = default)
    {
        Enum.TryParse<Gender>(gender, out var parsedGender);

        string? thumbUrl = null;
        string? fullUrl = null;

        if (imageUpload != null && imageUpload.Length > 0)
        {
            using var stream = imageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, imageUpload.FileName, imageUpload.ContentType, "employees", cropX, cropY, cropW, cropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var request = new CreateEmployeeRequest
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Gender = parsedGender,
            DateOfBirth = dob,
            DateEmployed = dateEmployed,
            DepartmentId = departmentId,
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl
        };

        return await _employeeService.CreateAsync(request, ct);
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(
        Guid employeeId, string firstName, string? middleName, string lastName, 
        string gender, DateTime? dob, int? departmentId, string status, 
        IFormFile? imageUpload, int? cropX, int? cropY, int? cropW, int? cropH, 
        CancellationToken ct = default)
    {
        Enum.TryParse<Gender>(gender, out var parsedGender);
        Enum.TryParse<EmployeeStatus>(status, out var parsedStatus);

        string? thumbUrl = null;
        string? fullUrl = null;

        if (imageUpload != null && imageUpload.Length > 0)
        {
            var existingEmployee = await _employeeService.GetByIdAsync(employeeId, ct);
            if (existingEmployee != null)
            {
                if (!string.IsNullOrWhiteSpace(existingEmployee.ThumbnailUrl))
                    await _fileService.DeleteFileAsync(existingEmployee.ThumbnailUrl, ct);
                if (!string.IsNullOrWhiteSpace(existingEmployee.FullImageUrl))
                    await _fileService.DeleteFileAsync(existingEmployee.FullImageUrl, ct);
            }
            using var stream = imageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, imageUpload.FileName, imageUpload.ContentType, "employees", cropX, cropY, cropW, cropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

        var update = new UpdateEmployeeRequest
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Gender = parsedGender,
            DateOfBirth = dob,
            DepartmentId = departmentId,
            Status = parsedStatus,
            ThumbnailUrl = thumbUrl,
            FullImageUrl = fullUrl
        };

        return await _employeeService.UpdateAsync(employeeId, update, ct);
    }

    public async Task<Employee360Dto?> Get360ByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        return await _employeeService.Get360ByIdAsync(employeeId, ct);
    }

    public async Task<bool> TerminateOrDeleteEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var existing = await _employeeService.GetByIdAsync(employeeId, ct);
        if (existing == null) return false;

        if (existing.Status == EmployeeStatus.Pending)
        {
            // Hard delete is safe for Pending (e.g. mistaken entry)
            return await _employeeService.DeleteAsync(employeeId, ct);
        }
        
        // Soft delete / terminate for active staff
        var update = new UpdateEmployeeRequest
        {
            FirstName = existing.FirstName,
            MiddleName = existing.MiddleName,
            LastName = existing.LastName,
            Gender = existing.Gender,
            DateOfBirth = existing.DateOfBirth,
            DepartmentId = existing.DepartmentId,
            Status = EmployeeStatus.Fired,
            ThumbnailUrl = existing.ThumbnailUrl,
            FullImageUrl = existing.FullImageUrl
        };
        var updated = await _employeeService.UpdateAsync(employeeId, update, ct);
        return updated != null;
    }

    public async Task<bool> ReinstateEmployeeAsync(Guid employeeId, CancellationToken ct = default)
    {
        var existing = await _employeeService.GetByIdAsync(employeeId, ct);
        if (existing == null) return false;
        
        var update = new UpdateEmployeeRequest
        {
            FirstName = existing.FirstName,
            MiddleName = existing.MiddleName,
            LastName = existing.LastName,
            Gender = existing.Gender,
            DateOfBirth = existing.DateOfBirth,
            DepartmentId = existing.DepartmentId,
            Status = EmployeeStatus.Active,
            ThumbnailUrl = existing.ThumbnailUrl,
            FullImageUrl = existing.FullImageUrl
        };
        var updated = await _employeeService.UpdateAsync(employeeId, update, ct);
        return updated != null;
    }
}
