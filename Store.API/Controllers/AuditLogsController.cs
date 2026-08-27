using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/audit-logs")]
[ApiController]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _auditLogService.GetMetricsAsync(ct);
        return Ok(ApiResponse<AuditLogMetricsDto>.Ok(metrics));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
    public async Task<IActionResult> GetPaged([FromQuery] AuditLogFilterRequest request, CancellationToken ct)
    {
        var result = await _auditLogService.GetAuditLogsPagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var log = await _auditLogService.GetByIdAsync(id, ct);
        if (log is null)
        {
            return NotFound(ApiErrorResponse.From("not_found", "Audit log entry not found",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<AuditLogDto>.Ok(log));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogEntryRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId) && request.UserId == Guid.Empty)
        {
            request.UserId = userId;
        }

        if (string.IsNullOrWhiteSpace(request.IpAddress))
        {
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        if (string.IsNullOrWhiteSpace(request.UserAgent))
        {
            request.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        }

        var created = await _auditLogService.LogAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<AuditLogDto>.Ok(created));
    }

    [HttpGet("export/csv")]
    [Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
    public async Task<IActionResult> ExportCsv([FromQuery] AuditLogFilterRequest request, CancellationToken ct)
    {
        request.Page = 1;
        request.PageSize = 5000;
        var paged = await _auditLogService.GetAuditLogsPagedAsync(request, ct);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Timestamp,Actor Username,Actor Role,Category,Severity,Action,Summary,Target Entity,Target ID,IP Address,Device,User Agent");

        foreach (var l in paged.Items)
        {
            sb.AppendLine(string.Join(",",
                l.Id,
                EscapeCsv(l.DateCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsv(l.ActorUsername),
                EscapeCsv(l.ActorRole ?? "User"),
                EscapeCsv(l.Category),
                EscapeCsv(l.Severity),
                EscapeCsv(l.Action),
                EscapeCsv(l.Summary),
                EscapeCsv(l.TargetEntity ?? ""),
                EscapeCsv(l.TargetId ?? ""),
                EscapeCsv(l.IpAddress ?? ""),
                EscapeCsv(l.DeviceType ?? ""),
                EscapeCsv(l.UserAgent ?? "")
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"audit_log_export_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    [HttpGet("export/json")]
    [Authorize(Policy = PermissionKeys.AdminRoleMatrix)]
    public async Task<IActionResult> ExportJson([FromQuery] AuditLogFilterRequest request, CancellationToken ct)
    {
        request.Page = 1;
        request.PageSize = 5000;
        var paged = await _auditLogService.GetAuditLogsPagedAsync(request, ct);

        var json = JsonSerializer.Serialize(paged.Items, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"audit_log_export_{DateTime.UtcNow:yyyyMMdd_HHmm}.json");
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return $"\"{field}\"";
    }
}
