using System.Text;
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
    private readonly IEmployeeManager _employeeManager;

    public IReadOnlyList<EmployeeDto> Employees { get; private set; } = Array.Empty<EmployeeDto>();
    public EmployeeMetricsDto Metrics { get; private set; } = new();
    public IReadOnlyList<Department> Departments { get; private set; } = Array.Empty<Department>();
    public int TotalEmployees { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 24;
    public int TotalPages => (int)Math.Ceiling((double)TotalEmployees / PageSize);

    // Query, Filter & View Controls
    [BindProperty(SupportsGet = true)] public string? SearchQuery { get; set; }
    [BindProperty(SupportsGet = true)] public int? DepartmentFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string SortBy { get; set; } = "name_asc";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "grid";
    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }

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
    [BindProperty] public IFormFile? ImageUpload { get; set; }

    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public EmployeesModel(IEmployeeService employeeService, IApiClientService apiClient, IEmployeeManager employeeManager)
    {
        _employeeService = employeeService;
        _apiClient = apiClient;
        _employeeManager = employeeManager;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PageNumber = Math.Max(1, page);

        var metricsTask = _employeeService.GetMetricsAsync(ct);
        var deptsTask = _apiClient.GetAsync<List<Department>>("/api/departments", ct);
        var employeesTask = _employeeService.GetAllAsync(new EmployeeFilterRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            IncludeInactive = true,
            SearchTerm = SearchQuery,
            DepartmentId = DepartmentFilter,
            Status = StatusFilter,
            SortBy = SortBy
        }, ct);

        await Task.WhenAll(metricsTask, deptsTask, employeesTask);

        Metrics = await metricsTask ?? new EmployeeMetricsDto();
        Departments = await deptsTask ?? new List<Department>();
        var result = await employeesTask ?? new PagedResult<EmployeeDto>();

        Employees = result.Items?.ToList() ?? new List<EmployeeDto>();
        TotalEmployees = result.TotalCount;

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            if (EditEmployeeId.HasValue && EditEmployeeId.Value != Guid.Empty)
            {
                var updated = await _employeeManager.UpdateEmployeeAsync(
                    EditEmployeeId.Value, EmpFirstName, EmpMiddleName, EmpLastName,
                    EmpGender, EmpDateOfBirth, EmpDepartmentId, EmpStatus,
                    ImageUpload, CropX, CropY, CropW, CropH, ct);
                    
                StatusMessage = updated is not null
                    ? $"Employee '{updated.FullName}' updated successfully."
                    : "Error: Employee not found.";
            }
            else
            {
                var created = await _employeeManager.CreateEmployeeAsync(
                    EmpFirstName, EmpMiddleName, EmpLastName,
                    EmpGender, EmpDateOfBirth, EmpDateEmployed, EmpDepartmentId,
                    ImageUpload, CropX, CropY, CropW, CropH, ct);
                    
                StatusMessage = created is not null 
                    ? $"Employee '{created.FullName}' added successfully."
                    : "Error: Could not create employee.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetEmployeeDrawerAsync(Guid id, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };

        _apiClient.SetToken(token);

        try
        {
            var employee360 = await _employeeManager.Get360ByIdAsync(id, ct);
            if (employee360 == null)
                return new JsonResult(new { success = false, message = "Employee not found." }) { StatusCode = 404 };

            return new JsonResult(new { success = true, employee360 });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message }) { StatusCode = 500 };
        }
    }

    public async Task<IActionResult> OnPostTerminateAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var success = await _employeeManager.TerminateOrDeleteEmployeeAsync(employeeId, ct);
            StatusMessage = success ? "Employee record updated successfully." : "Error: Could not terminate/delete employee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReinstateAsync(Guid employeeId, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            var success = await _employeeManager.ReinstateEmployeeAsync(employeeId, ct);
            StatusMessage = success ? "Employee has been reinstated to Active status." : "Error: Could not reinstate employee.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var result = await _employeeService.GetAllAsync(new EmployeeFilterRequest
        {
            Page = 1,
            PageSize = 5000,
            IncludeInactive = true,
            SearchTerm = SearchQuery,
            DepartmentId = DepartmentFilter,
            Status = StatusFilter,
            SortBy = SortBy
        }, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Employee Code,First Name,Middle Name,Last Name,Full Name,Gender,Department,Status,Date Employed,NID Number,Salary Grade");

        foreach (var emp in result.Items)
        {
            sb.AppendLine($"\"{emp.ShortEmployeeCode}\",\"{EscapeCsv(emp.FirstName)}\",\"{EscapeCsv(emp.MiddleName)}\",\"{EscapeCsv(emp.LastName)}\",\"{EscapeCsv(emp.FullName)}\",\"{emp.Gender}\",\"{EscapeCsv(emp.DepartmentName)}\",\"{emp.Status}\",\"{emp.DateEmployed:yyyy-MM-dd}\",\"{EscapeCsv(emp.NidNumber)}\",\"{EscapeCsv(emp.SalaryGrade)}\"");
        }

        var fileName = $"employees_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }
}
