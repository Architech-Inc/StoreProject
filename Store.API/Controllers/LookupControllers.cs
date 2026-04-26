using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
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
    public async Task<IActionResult> Create([FromBody] CreateLookupRequest request, CancellationToken ct)
    {
        var category = await _categoryService.CreateAsync(request.Name, request.Description, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, ApiResponse<Category>.Ok(category));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLookupRequest request, CancellationToken ct)
    {
        var category = await _categoryService.UpdateAsync(id, request.Name, request.Description, ct);
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

// Shared request DTOs for lookup controllers
public record CreateLookupRequest(string Name, string? Description);
public record CreateUnitRequest(string Name, string Abbreviation, string? Description);
