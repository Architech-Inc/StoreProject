using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;

    public EmployeeService(IUnitOfWork uow) => _uow = uow;

    public async Task<EmployeeDto?> GetByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().Query()
            .Include(e => e.Department)
            .Include(e => e.Salary)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

        return emp is null ? null : MapToDto(emp);
    }

    public async Task<PagedResult<EmployeeDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Employee>().Query()
            .Include(e => e.Department)
            .Include(e => e.Salary)
            .Where(e => e.Status != EmployeeStatus.Fired)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(e => e.FirstName.Contains(request.SearchTerm) ||
                                     e.LastName.Contains(request.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);

        return new PagedResult<EmployeeDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = new Employee
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            NidNumber = request.NidNumber?.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            DateEmployed = request.DateEmployed,
            DepartmentId = request.DepartmentId,
            SalaryId = request.SalaryId,
            Status = EmployeeStatus.Pending
        };

        await _uow.Repository<Employee>().AddAsync(employee, ct);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(employee.EmployeeId, ct))!;
    }

    public async Task<EmployeeDto?> UpdateAsync(Guid employeeId, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await _uow.Repository<Employee>().Query()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

        if (employee is null) return null;

        if (!string.IsNullOrWhiteSpace(request.FirstName)) employee.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) employee.LastName = request.LastName.Trim();
        if (request.MiddleName is not null) employee.MiddleName = request.MiddleName.Trim();
        if (request.Gender.HasValue) employee.Gender = request.Gender.Value;
        if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth;
        if (request.DepartmentId.HasValue) employee.DepartmentId = request.DepartmentId;
        if (request.SalaryId.HasValue) employee.SalaryId = request.SalaryId;
        if (request.Status.HasValue) employee.Status = request.Status.Value;

        _uow.Repository<Employee>().Update(employee);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(employeeId, ct);
    }

    public async Task<bool> DeleteAsync(Guid employeeId, CancellationToken ct = default)
    {
        var employee = await _uow.Repository<Employee>().Query()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

        if (employee is null) return false;

        employee.Status = EmployeeStatus.Fired;
        _uow.Repository<Employee>().Update(employee);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        FirstName = e.FirstName,
        MiddleName = e.MiddleName,
        LastName = e.LastName,
        NidNumber = e.NidNumber,
        Gender = e.Gender,
        DateOfBirth = e.DateOfBirth,
        DateEmployed = e.DateEmployed,
        Status = e.Status,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name,
        SalaryId = e.SalaryId,
        SalaryGrade = e.Salary?.Grade,
        ImagePath = e.ImagePath,
        DateCreated = e.DateCreated
    };
}
