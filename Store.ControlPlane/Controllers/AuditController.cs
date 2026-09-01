using Microsoft.AspNetCore.Mvc;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;

namespace Store.ControlPlane.Controllers;

[ApiController]
[Route("api/control/tenants/{id:guid}/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditTrail(Guid id, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var logs = await _auditService.GetAuditTrailAsync(id, limit, ct);
        return Ok(ApiResponse<IReadOnlyList<TenantAuditDto>>.Ok(logs));
    }
}
