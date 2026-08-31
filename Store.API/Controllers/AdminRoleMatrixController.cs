using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/admin/role-matrix")]
[Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
public class AdminRoleMatrixController : ControllerBase
{
    private readonly IStoreOperationsService _ops;

    public AdminRoleMatrixController(IStoreOperationsService ops)
    {
        _ops = ops;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoleMatrix(CancellationToken ct)
    {
        var result = await _ops.GetRoleMatrixAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<RoleMatrixDto>>.Ok(result));
    }

    [HttpPost("permission")]
    public async Task<IActionResult> UpdatePermission([FromBody] UpdateRolePermissionRequest request, CancellationToken ct)
    {
        if (request.RoleId <= 0 || string.IsNullOrWhiteSpace(request.PermissionKey))
        {
            return BadRequest(ApiErrorResponse.From("invalid_request", "RoleId and PermissionKey are required.", traceId: HttpContext.TraceIdentifier));
        }

        var result = await _ops.UpdateRolePermissionAsync(request, ct);
        return Ok(ApiResponse<RolePermissionDto>.Ok(result, "Role permission updated successfully."));
    }
}
