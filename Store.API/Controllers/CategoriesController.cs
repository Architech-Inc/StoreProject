using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Store.API.Attributes;
using Store.Models.Common;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Category>>.Ok(await _categoryService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var category = await _categoryService.GetByIdAsync(id, ct);
        if (category is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Category not found."));

        return Ok(ApiResponse<Category>.Ok(category));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    [Audit("Create Category", Category = "Inventory")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request.Name, request.Description, request.ThumbnailUrl, request.FullImageUrl, ct);
            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, ApiResponse<Category>.Ok(category));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "CreateCategory")));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    [Audit("Update Category", Category = "Inventory")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(id, request.Name, request.Description, request.ThumbnailUrl, request.FullImageUrl, ct);
            if (category is null)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Category not found."));

            return Ok(ApiResponse<Category>.Ok(category));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "UpdateCategory")));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    [Audit("Delete Category", Category = "Inventory")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Category not found."));

            return Ok(ApiResponse<object>.Ok(null!, "Category deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "DeleteCategory")));
        }
    }
}
