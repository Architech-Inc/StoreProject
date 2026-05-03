using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class EmployeesModel : SecurePageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<EmployeeDto> Employees { get; private set; } = Array.Empty<EmployeeDto>();
    public IReadOnlyList<Department> Departments { get; private set; } = Array.Empty<Department>();
    public int TotalEmployees { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalEmployees / PageSize);

    // Create / Edit form
    [BindProperty] public Guid? EditEmployeeId { get; set; }
    [BindProperty] public string EmpFirstName { get; set; } = string.Empty;
    [BindProperty] public string? EmpMiddleName { get; set; }
    [BindProperty] public string EmpLastName { get; set; } = string.Empty;
    [BindProperty] public string EmpGender { get; set; } = "NotSpecified";
    [BindProperty] public DateTime? EmpDateOfBirth { get; set; }
    [BindProperty] public DateTime EmpDateEmployed { get; set; } = DateTime.Today;
    [BindProperty] public int? EmpDepartmentId { get; set; }
    [BindProperty] public string EmpStatus { get; set; } = "Active";

    [TempData] public string? StatusMessage { get; set; }

    public EmployeesModel(IEmployeeService employeeService, IApiClientService apiClient)
    {
        _employeeService = employeeService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        PageNumber = Math.Max(1, page);
        var result = await _employeeService.GetAllAsync(new PagedRequest { Page = PageNumber, PageSize = PageSize }, ct);
        Employees = result.Items?.ToList() ?? new List<EmployeeDto>();
        TotalEmployees = result.TotalCount;

        Departments = (await _apiClient.GetAsync<List<Department>>("/api/departments", ct)) ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        try
        {
            Enum.TryParse<Gender>(EmpGender, out var gender);

            if (EditEmployeeId.HasValue && EditEmployeeId.Value != Guid.Empty)
            {
                Enum.TryParse<EmployeeStatus>(EmpStatus, out var status);
                var update = new UpdateEmployeeRequest
                {
                    FirstName = EmpFirstName,
                    MiddleName = EmpMiddleName,
                    LastName = EmpLastName,
                    Gender = gender,
                    DateOfBirth = EmpDateOfBirth,
                    DepartmentId = EmpDepartmentId,
                    Status = status
                };
                var updated = await _employeeService.UpdateAsync(EditEmployeeId.Value, update, ct);
                StatusMessage = updated is not null
                    ? $"Employee '{updated.FullName}' updated."
                    : "Error: Employee not found.";
            }
            else
            {
                var create = new CreateEmployeeRequest
                {
                    FirstName = EmpFirstName,
                    MiddleName = EmpMiddleName,
                    LastName = EmpLastName,
                    Gender = gender,
                    DateOfBirth = EmpDateOfBirth,
                    DateEmployed = EmpDateEmployed,
                    DepartmentId = EmpDepartmentId
                };
                var created = await _employeeService.CreateAsync(create, ct);
                StatusMessage = $"Employee '{created.FullName}' added.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out _, out _)) return GoToLogin();

        try
        {
            var ok = await _employeeService.DeleteAsync(employeeId, ct);
            StatusMessage = ok ? "Employee removed." : "Error: Employee not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }
}
