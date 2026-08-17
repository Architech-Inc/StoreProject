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
    private readonly IFileService _fileService;

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
    [BindProperty] public IFormFile? ImageUpload { get; set; }

    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public EmployeesModel(IEmployeeService employeeService, IApiClientService apiClient, IFileService fileService)
    {
        _employeeService = employeeService;
        _apiClient = apiClient;
        _fileService = fileService;
    }

    [BindProperty(SupportsGet = true)] public string? SearchQuery { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PageNumber = Math.Max(1, page);
        var result = await _employeeService.GetAllAsync(new PagedRequest { Page = PageNumber, PageSize = PageSize, IncludeInactive = true, SearchTerm = SearchQuery }, ct);
        Employees = result.Items?.ToList() ?? new List<EmployeeDto>();
        TotalEmployees = result.TotalCount;

        Departments = (await _apiClient.GetAsync<List<Department>>("/api/departments", ct)) ?? new();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        try
        {
            Enum.TryParse<Gender>(EmpGender, out var gender);

            string? thumbUrl = null;
            string? fullUrl = null;
            if (ImageUpload != null && ImageUpload.Length > 0)
            {
                if (EditEmployeeId.HasValue && EditEmployeeId.Value != Guid.Empty)
                {
                    var existingEmployee = await _employeeService.GetByIdAsync(EditEmployeeId.Value, ct);
                    if (existingEmployee != null)
                    {
                        if (!string.IsNullOrWhiteSpace(existingEmployee.ThumbnailUrl))
                            await _fileService.DeleteFileAsync(existingEmployee.ThumbnailUrl, ct);
                        if (!string.IsNullOrWhiteSpace(existingEmployee.FullImageUrl))
                            await _fileService.DeleteFileAsync(existingEmployee.FullImageUrl, ct);
                    }
                }
                using var stream = ImageUpload.OpenReadStream();
                var uploadResult = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "employees", CropX, CropY, CropW, CropH, ct);
                thumbUrl = uploadResult.ThumbnailUrl;
                fullUrl = uploadResult.FullImageUrl;
            }

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
                    Status = status,
                    ThumbnailUrl = thumbUrl,
                    FullImageUrl = fullUrl
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
                    DepartmentId = EmpDepartmentId,
                    ThumbnailUrl = thumbUrl,
                    FullImageUrl = fullUrl
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

    public async Task<IActionResult> OnGetEmployeeDrawerAsync(Guid id, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return new JsonResult(new { success = false, message = "Unauthorized" }) { StatusCode = 401 };

        _apiClient.SetToken(token);

        try
        {
            var employee360 = await _apiClient.GetAsync<Employee360Dto>($"/api/employees/{id}/360", ct);
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
            var existing = await _employeeService.GetByIdAsync(employeeId, ct);
            if (existing == null)
            {
                StatusMessage = "Error: Employee not found.";
                return RedirectToPage();
            }

            if (existing.Status == EmployeeStatus.Pending)
            {
                // Hard delete is safe for Pending (e.g. mistaken entry)
                var ok = await _employeeService.DeleteAsync(employeeId, ct);
                StatusMessage = ok ? "Pending employee removed completely." : "Error removing pending employee.";
            }
            else
            {
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
                StatusMessage = updated != null ? $"Employee '{existing.FullName}' has been terminated." : "Error terminating employee.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }
}
