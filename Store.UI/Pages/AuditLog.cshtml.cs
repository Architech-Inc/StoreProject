using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class AuditLogModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<StockMovementDto> Movements { get; private set; } = Array.Empty<StockMovementDto>();
    public int CurrentPage { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public StockMovementType? TypeFilter { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public AuditLogModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, StockMovementType? type = null, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.InventoryRead))
            return AccessDenied();

        _apiClient.SetToken(token);
        CurrentPage = Math.Max(1, page);
        TypeFilter = type;

        var typeParam = type.HasValue ? $"&type={(int)type.Value}" : string.Empty;
        var url = $"/api/inventory/movements?page={CurrentPage}&pageSize={PageSize}{typeParam}";

        var movements = await _apiClient.GetAsync<List<StockMovementDto>>(url, ct);
        Movements = (IReadOnlyList<StockMovementDto>?)movements?.AsReadOnly() ?? Array.Empty<StockMovementDto>();

        return Page();
    }
}
