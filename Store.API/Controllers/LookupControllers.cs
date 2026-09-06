using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.HR;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Category>>.Ok(await _categoryService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category is null) return NotFound(ApiResponse<object>.Fail("Category not found."));
        return Ok(ApiResponse<Category>.Ok(category));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(request.Name, request.Description, request.ThumbnailUrl, request.FullImageUrl, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, ApiResponse<Category>.Ok(category));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var category = await _categoryService.UpdateAsync(id, request.Name, request.Description, request.ThumbnailUrl, request.FullImageUrl, ct);
        if (category is null) return NotFound(ApiResponse<object>.Fail("Category not found."));
        return Ok(ApiResponse<Category>.Ok(category));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _categoryService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Category not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Category deleted."));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService) => _unitService = unitService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Unit>>.Ok(await _unitService.GetAllAsync(ct)));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        var unit = await _unitService.CreateAsync(request.Name, request.Abbreviation, request.Description, ct);
        return Ok(ApiResponse<Unit>.Ok(unit));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        var unit = await _unitService.UpdateAsync(id, request.Name, request.Abbreviation, request.Description, ct);
        if (unit is null) return NotFound(ApiResponse<object>.Fail("Unit not found."));
        return Ok(ApiResponse<Unit>.Ok(unit));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _unitService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Unit not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Unit deleted."));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _deptService;

    public DepartmentsController(IDepartmentService deptService) => _deptService = deptService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Department>>.Ok(await _deptService.GetAllAsync(ct)));

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateLookupRequest request, CancellationToken ct)
    {
        var dept = await _deptService.CreateAsync(request.Name, request.Description, ct);
        return Ok(ApiResponse<Department>.Ok(dept));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLookupRequest request, CancellationToken ct)
    {
        var dept = await _deptService.UpdateAsync(id, request.Name, request.Description, ct);
        if (dept is null) return NotFound(ApiResponse<object>.Fail("Department not found."));
        return Ok(ApiResponse<Department>.Ok(dept));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _deptService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Department not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Department deleted."));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalariesController : ControllerBase
{
    private readonly ISalaryService _salaryService;

    public SalariesController(ISalaryService salaryService) => _salaryService = salaryService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Salary>>.Ok(await _salaryService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var salary = await _salaryService.GetByIdAsync(id, ct);
        if (salary is null) return NotFound(ApiResponse<object>.Fail("Salary grade not found."));
        return Ok(ApiResponse<Salary>.Ok(salary));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateSalaryRequest request, CancellationToken ct)
    {
        try
        {
            var salary = await _salaryService.CreateAsync(request.Grade, request.BasicAmount, request.AllowanceAmount, request.Description, ct);
            return CreatedAtAction(nameof(GetById), new { id = salary.SalaryId }, ApiResponse<Salary>.Ok(salary));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalaryRequest request, CancellationToken ct)
    {
        try
        {
            var salary = await _salaryService.UpdateAsync(id, request.Grade, request.BasicAmount, request.AllowanceAmount, request.Description, ct);
            if (salary is null) return NotFound(ApiResponse<object>.Fail("Salary grade not found."));
            return Ok(ApiResponse<Salary>.Ok(salary));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _salaryService.DeleteAsync(id, ct);
            if (!deleted) return NotFound(ApiResponse<object>.Fail("Salary grade not found."));
            return Ok(ApiResponse<object>.Ok(null!, "Salary grade deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

// Shared request DTOs for lookup controllers
public record CreateLookupRequest(string Name, string? Description);
public record CreateCategoryRequest(string Name, string? Description, string? ThumbnailUrl, string? FullImageUrl);
public record CreateUnitRequest(string Name, string Abbreviation, string? Description);

[ApiController]
[Route("api/[controller]")]
[Route("api/lookup/[controller]")]
[AllowAnonymous]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;

    public CountriesController(ICountryService countryService) => _countryService = countryService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<CountryDto>>.Ok(await _countryService.GetCountriesAsync(ct)));
}

