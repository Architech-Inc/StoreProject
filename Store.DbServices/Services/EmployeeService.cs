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

    public async Task<Employee360Dto?> Get360ByIdAsync(Guid employeeId, CancellationToken ct = default)
    {
        var emp = await _uow.Repository<Employee>().Query()
            .Include(e => e.Department)
            .Include(e => e.Salary)
            .Include(e => e.Emails).ThenInclude(ee => ee.Email)
            .Include(e => e.Phones).ThenInclude(ep => ep.Phone)
            .Include(e => e.Users).ThenInclude(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

        if (emp is null) return null;

        var dto = new Employee360Dto
        {
            Profile = MapToDto(emp),
            Emails = emp.Emails.Select(ee => ee.Email.Address).ToList(),
            Phones = emp.Phones.Select(ep => ep.Phone.Number).ToList(),
            Users = emp.Users.Select(u => new Store.Models.DTOs.Users.UserDto 
            {
                UserId = u.UserId,
                Username = u.Username,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name,
                Status = u.Status,
                DateCreated = u.DateCreated
            }).ToList()
        };

        return dto;
    }

    public async Task<PagedResult<EmployeeDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Employee>().Query()
            .Include(e => e.Department)
            .Include(e => e.Salary)
            .AsNoTracking();

        if (request is EmployeeFilterRequest filterReq)
        {
            if (filterReq.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == filterReq.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(filterReq.Status) && Enum.TryParse<EmployeeStatus>(filterReq.Status, true, out var statusVal))
                query = query.Where(e => e.Status == statusVal);
        }

        if (!request.IncludeInactive)
            query = query.Where(e => e.Status != EmployeeStatus.Fired);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(e => e.FirstName.Contains(term) ||
                                     e.LastName.Contains(term) ||
                                     (e.MiddleName != null && e.MiddleName.Contains(term)) ||
                                     (e.NidNumber != null && e.NidNumber.Contains(term)) ||
                                     (e.Department != null && e.Department.Name.Contains(term)));
        }

        var total = await query.CountAsync(ct);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name_desc" => query.OrderByDescending(e => e.LastName).ThenByDescending(e => e.FirstName),
            "date_desc" or "newest" => query.OrderByDescending(e => e.DateEmployed),
            "date_asc" or "oldest" => query.OrderBy(e => e.DateEmployed),
            "dept" => query.OrderBy(e => e.Department != null ? e.Department.Name : "").ThenBy(e => e.LastName),
            _ => query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => MapToDto(e))
            .ToListAsync(ct);

        return new PagedResult<EmployeeDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<EmployeeMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var query = _uow.Repository<Employee>().Query().AsNoTracking();
        var total = await query.CountAsync(ct);
        var active = await query.CountAsync(e => e.Status == EmployeeStatus.Active, ct);
        var pending = await query.CountAsync(e => e.Status == EmployeeStatus.Pending, ct);
        var terminated = await query.CountAsync(e => e.Status == EmployeeStatus.Fired || e.Status == EmployeeStatus.Suspended || e.Status == EmployeeStatus.Sanctioned, ct);
        var deptCount = await _uow.Repository<Department>().Query().CountAsync(ct);

        return new EmployeeMetricsDto
        {
            TotalEmployees = total,
            ActiveEmployees = active,
            PendingEmployees = pending,
            TerminatedEmployees = terminated,
            DepartmentCount = deptCount
        };
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
            Status = EmployeeStatus.Pending,
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            FullImageUrl = request.FullImageUrl?.Trim()
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
        if (request.ThumbnailUrl != null) employee.ThumbnailUrl = request.ThumbnailUrl.Trim();
        if (request.FullImageUrl != null) employee.FullImageUrl = request.FullImageUrl.Trim();

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
        ThumbnailUrl = e.ThumbnailUrl,
            FullImageUrl = e.FullImageUrl,
        DateCreated = e.DateCreated
    };
}
