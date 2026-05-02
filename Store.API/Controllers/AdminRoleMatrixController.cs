using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        return Ok(await _ops.GetRoleMatrixAsync(ct));
    }

    [HttpPost("permission")]
    public async Task<IActionResult> UpdatePermission([FromBody] UpdateRolePermissionRequest request, CancellationToken ct)
    {
        return Ok(await _ops.UpdateRolePermissionAsync(request, ct));
    }
}
