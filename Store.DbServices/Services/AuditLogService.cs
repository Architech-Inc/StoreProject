using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _uow;

    public AuditLogService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AuditLogMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var logs = await _uow.Repository<AuditLog>().Query()
            .AsNoTracking()
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;

        var dtos = logs.Select(MapToDto).ToList();

        return new AuditLogMetricsDto
        {
            TotalEvents = dtos.Count,
            TodayEvents = dtos.Count(l => l.DateCreated.Date == today),
            SecurityIncidentsCount = dtos.Count(l => l.Severity == "Security" || l.Category == "Security" || l.Category == "Authentication" || l.Action.Contains("2FA", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Lock", StringComparison.OrdinalIgnoreCase)),
            PrivilegeChangesCount = dtos.Count(l => l.Category == "Privilege" || l.Action.Contains("Role", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Privilege", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Permission", StringComparison.OrdinalIgnoreCase)),
            CriticalRiskCount = dtos.Count(l => l.Severity == "Critical" || l.Action.Contains("Override", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Wastage", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Void", StringComparison.OrdinalIgnoreCase) || l.Action.Contains("Delete", StringComparison.OrdinalIgnoreCase))
        };
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<AuditLog>().Query()
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Role)
            .Include(a => a.User).ThenInclude(u => u.Employee)
            .AsQueryable();

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.DateCreated >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.DateCreated <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a => a.Action.Contains(term) ||
                                     (a.Details != null && a.Details.Contains(term)) ||
                                     (a.IpAddress != null && a.IpAddress.Contains(term)) ||
                                     a.User.Username.Contains(term) ||
                                     (a.User.Employee != null && (a.User.Employee.FirstName.Contains(term) || a.User.Employee.LastName.Contains(term))));
        }

        var allMatching = await query
            .OrderByDescending(a => a.DateCreated)
            .ToListAsync(ct);

        var mapped = allMatching.Select(MapToDto);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            mapped = mapped.Where(m => m.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            mapped = mapped.Where(m => m.Severity.Equals(request.Severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.TargetEntity))
        {
            mapped = mapped.Where(m => string.Equals(m.TargetEntity, request.TargetEntity, StringComparison.OrdinalIgnoreCase));
        }

        var list = mapped.ToList();
        var total = list.Count;
        var paged = list
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<AuditLogDto>
        {
            Items = paged,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var log = await _uow.Repository<AuditLog>().Query()
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Role)
            .Include(a => a.User).ThenInclude(u => u.Employee)
            .FirstOrDefaultAsync(a => a.AuditLogId == id, ct);

        return log is null ? null : MapToDto(log);
    }

    public async Task<AuditLogDto> LogAsync(CreateAuditLogEntryRequest request, CancellationToken ct = default)
    {
        var payload = new StructuredAuditPayload
        {
            Category = request.Category,
            Severity = request.Severity,
            Summary = request.Summary,
            TargetEntity = request.TargetEntity,
            TargetId = request.TargetId,
            OldValuesJson = request.OldValuesJson,
            NewValuesJson = request.NewValuesJson,
            MetadataJson = request.MetadataJson
        };

        var detailsJson = JsonSerializer.Serialize(payload);

        var log = new AuditLog
        {
            UserId = request.UserId,
            Action = request.Action.Trim(),
            Details = detailsJson,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        };

        await _uow.Repository<AuditLog>().AddAsync(log);
        await _uow.SaveChangesAsync();

        var loaded = await _uow.Repository<AuditLog>().Query()
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Role)
            .Include(a => a.User).ThenInclude(u => u.Employee)
            .FirstOrDefaultAsync(a => a.AuditLogId == log.AuditLogId, ct);

        return MapToDto(loaded ?? log);
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken ct = default)
    {
        var logs = await _uow.Repository<AuditLog>().Query()
            .AsNoTracking()
            .Include(a => a.User).ThenInclude(u => u.Role)
            .Include(a => a.User).ThenInclude(u => u.Employee)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.DateCreated)
            .Take(limit)
            .ToListAsync(ct);

        return logs.Select(MapToDto).ToList();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static AuditLogDto MapToDto(AuditLog a)
    {
        var dto = new AuditLogDto
        {
            Id = a.AuditLogId,
            Action = a.Action,
            ActorUserId = a.UserId,
            ActorUsername = a.User?.Username ?? "Unknown Actor",
            ActorRole = a.User?.Role?.Name ?? "User",
            ActorFullName = a.User?.Employee != null ? $"{a.User.Employee.FirstName} {a.User.Employee.LastName}".Trim() : null,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            DeviceType = InferDeviceType(a.UserAgent),
            RawDetails = a.Details,
            DateCreated = a.DateCreated
        };

        if (!string.IsNullOrWhiteSpace(a.Details))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<StructuredAuditPayload>(a.Details);
                if (parsed != null && (!string.IsNullOrEmpty(parsed.Summary) || !string.IsNullOrEmpty(parsed.Category)))
                {
                    dto.Category = parsed.Category ?? "System";
                    dto.Severity = parsed.Severity ?? "Info";
                    dto.Summary = !string.IsNullOrWhiteSpace(parsed.Summary) ? parsed.Summary : a.Action;
                    dto.TargetEntity = parsed.TargetEntity;
                    dto.TargetId = parsed.TargetId;
                    dto.OldValuesJson = parsed.OldValuesJson;
                    dto.NewValuesJson = parsed.NewValuesJson;
                    dto.MetadataJson = parsed.MetadataJson;
                    return dto;
                }
            }
            catch
            {
                // Fallback to unstructured text
            }

            dto.Summary = a.Details;
        }
        else
        {
            dto.Summary = a.Action;
        }

        // Heuristic categorization for legacy unstructured records
        if (a.Action.Contains("2FA", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Password", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Session", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Security";
            dto.Severity = a.Action.Contains("Disable", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Revoke", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info";
        }
        else if (a.Action.Contains("Role", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Privilege", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Administration";
            dto.Severity = "Warning";
        }
        else if (a.Action.Contains("Login", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Authentication";
            dto.Severity = a.Action.Contains("Fail", StringComparison.OrdinalIgnoreCase) ? "Security" : "Info";
        }
        else if (a.Action.Contains("Price", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Discount", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Pricing";
            dto.Severity = "Warning";
        }
        else if (a.Action.Contains("Stock", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Transfer", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("Wastage", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Inventory";
            dto.Severity = a.Action.Contains("Wastage", StringComparison.OrdinalIgnoreCase) ? "Critical" : "Info";
        }
        else if (a.Action.Contains("Purchase", StringComparison.OrdinalIgnoreCase) || a.Action.Contains("PO", StringComparison.OrdinalIgnoreCase))
        {
            dto.Category = "Procurement";
            dto.Severity = "Info";
        }

        return dto;
    }

    public async Task<int> PruneLogsOlderThanAsync(DateTime threshold, CancellationToken ct = default)
    {
        var oldLogs = await _uow.Repository<AuditLog>().Query()
            .Where(a => a.DateCreated < threshold)
            .ToListAsync(ct);

        if (oldLogs.Count == 0) return 0;

        _uow.Repository<AuditLog>().RemoveRange(oldLogs);
        await _uow.SaveChangesAsync(ct);
        return oldLogs.Count;
    }

    private static string InferDeviceType(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Server / Service";
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone")) return "Mobile";
        if (ua.Contains("ipad") || ua.Contains("tablet")) return "Tablet";
        if (ua.Contains("windows") || ua.Contains("macintosh") || ua.Contains("linux")) return "Desktop";
        if (ua.Contains("curl") || ua.Contains("postman") || ua.Contains("httpclient")) return "API Client";
        return "Browser / Client";
    }
}
