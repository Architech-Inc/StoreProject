using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CommunicationLogsModel : SecurePageModel
{
    private readonly ICommunicationManager _commManager;
    private readonly IApiClientService _apiClient;

    public List<CommunicationLog> Logs { get; private set; } = new();
    
    public int TotalLogs { get; private set; }
    public int EmailCount { get; private set; }
    public int SmsCount { get; private set; }
    public int WhatsAppCount { get; private set; }
    public int FailedCount { get; private set; }

    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public int TotalPages => (int)Math.Ceiling((double)TotalLogs / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? Channel { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public CommunicationLogsModel(ICommunicationManager commManager, IApiClientService apiClient)
    {
        _commManager = commManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, string? channel = null, string? status = null, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PageNumber = Math.Max(1, page);
        Channel = channel;
        Status = status;

        var result = await _commManager.GetLogsPagedAsync(PageNumber, PageSize, Channel, Status, ct);
        
        Logs = result.Logs;
        TotalLogs = (int)result.TotalCount;

        EmailCount = Logs.Count(l => l.Channel == CommunicationChannel.Email);
        SmsCount = Logs.Count(l => l.Channel == CommunicationChannel.Sms);
        WhatsAppCount = Logs.Count(l => l.Channel == CommunicationChannel.WhatsApp);
        FailedCount = Logs.Count(l => l.Status == CommunicationStatus.Failed);

        return Page();
    }
}
