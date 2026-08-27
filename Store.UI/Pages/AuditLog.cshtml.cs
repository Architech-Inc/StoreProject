using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class AuditLogModel : SecurePageModel
{
    private readonly IAuditLogManager _auditLogManager;
    private readonly IApiClientService _apiClient;

    public AuditLogMetricsDto Metrics { get; private set; } = new();
    public PagedResult<AuditLogDto> LogsPaged { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SeverityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TargetEntityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public string[] Categories { get; } = new[]
    {
        "Authentication",
        "Security",
        "Inventory",
        "Pricing",
        "Procurement",
        "Administration",
        "Finance",
        "System"
    };

    public string[] Severities { get; } = new[]
    {
        "Info",
        "Warning",
        "Critical",
        "Security"
    };

    public AuditLogModel(IAuditLogManager auditLogManager, IApiClientService apiClient)
    {
        _auditLogManager = auditLogManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.AdminRoleMatrix) &&
            !HasPermission(permissions, PermissionKeys.ReportsRead) &&
            !HasPermission(permissions, PermissionKeys.InventoryRead))
        {
            return AccessDenied();
        }

        _apiClient.SetToken(token);

        Metrics = await _auditLogManager.GetMetricsAsync(ct);

        var filter = new AuditLogFilterRequest
        {
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = 25,
            SearchTerm = Search,
            Category = CategoryFilter,
            Severity = SeverityFilter,
            TargetEntity = TargetEntityFilter
        };

        LogsPaged = await _auditLogManager.GetAuditLogsPagedAsync(filter, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetDetailsJsonAsync(long id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var log = await _auditLogManager.GetAuditLogByIdAsync(id, ct);
        if (log is null) return NotFound();

        return new JsonResult(log);
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new AuditLogFilterRequest
        {
            Page = 1,
            PageSize = 2000,
            SearchTerm = Search,
            Category = CategoryFilter,
            Severity = SeverityFilter,
            TargetEntity = TargetEntityFilter
        };

        var paged = await _auditLogManager.GetAuditLogsPagedAsync(filter, ct);
        var bytes = _auditLogManager.ExportCsv(paged.Items);
        var filename = $"audit_compliance_log_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnGetExportJsonAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new AuditLogFilterRequest
        {
            Page = 1,
            PageSize = 2000,
            SearchTerm = Search,
            Category = CategoryFilter,
            Severity = SeverityFilter,
            TargetEntity = TargetEntityFilter
        };

        var paged = await _auditLogManager.GetAuditLogsPagedAsync(filter, ct);
        var bytes = _auditLogManager.ExportJson(paged.Items);
        var filename = $"audit_compliance_log_{DateTime.UtcNow:yyyyMMdd_HHmm}.json";
        return File(bytes, "application/json", filename);
    }
}
