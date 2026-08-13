using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // In a real scenario, probably require specific Admin policy
public class CommunicationLogsController : ControllerBase
{
    private readonly ICommunicationLogService _logService;

    public CommunicationLogsController(ICommunicationLogService logService)
    {
        _logService = logService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? channel = null, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        var logs = await _logService.GetLogsAsync(page, pageSize, channel, status, ct);
        var total = await _logService.GetLogsCountAsync(channel, status, ct);

        return Ok(new
        {
            Data = logs,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }
}
