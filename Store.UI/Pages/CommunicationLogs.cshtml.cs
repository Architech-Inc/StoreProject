using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CommunicationLogsModel : SecurePageModel
{
    private readonly IApiCommunicationLogService _logService;
    private readonly IApiClientService _apiClient;

    public List<CommunicationLog> Logs { get; private set; } = new();
    
    public int TotalLogs { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public int TotalPages => (int)Math.Ceiling((double)TotalLogs / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? Channel { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public CommunicationLogsModel(IApiCommunicationLogService logService, IApiClientService apiClient)
    {
        _logService = logService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, string? channel = null, string? status = null, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PageNumber = Math.Max(1, page);
        Channel = channel;
        Status = status;

        var result = await _logService.GetLogsAsync(PageNumber, PageSize, Channel, Status, ct);
        
        Logs = result.Logs;
        TotalLogs = (int)result.TotalCount;

        return Page();
    }
}
