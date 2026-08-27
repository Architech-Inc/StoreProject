using System.Text;
using System.Text.Json;
using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class AuditLogManager : IAuditLogManager
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogManager(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<AuditLogMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _auditLogService.GetMetricsAsync(ct);

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default)
        => await _auditLogService.GetAuditLogsPagedAsync(request, ct);

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken ct = default)
        => await _auditLogService.GetByIdAsync(id, ct);

    public byte[] ExportCsv(IEnumerable<AuditLogDto> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Log ID,Timestamp,Actor,Role,Category,Severity,Action,Summary,Target Entity,Target ID,IP Address,Device,User Agent");

        foreach (var l in logs)
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
                EscapeCsv(l.TargetEntity ?? "—"),
                EscapeCsv(l.TargetId ?? "—"),
                EscapeCsv(l.IpAddress ?? "—"),
                EscapeCsv(l.DeviceType ?? "—"),
                EscapeCsv(l.UserAgent ?? "—")
            ));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportJson(IEnumerable<AuditLogDto> logs)
    {
        var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
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
