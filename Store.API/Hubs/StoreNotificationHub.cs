using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Store.Models.DTOs.Notifications;

namespace Store.API.Hubs;

[Authorize]
public class StoreNotificationHub : Hub<IStoreNotificationClient>
{
    private readonly ILogger<StoreNotificationHub> _logger;

    public StoreNotificationHub(ILogger<StoreNotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("uid")?.Value ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("SignalR client {ConnectionId} attached to user group user_{UserId}", Context.ConnectionId, userId);
        }

        if (!string.IsNullOrEmpty(role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role.ToLowerInvariant()}");
            _logger.LogInformation("SignalR client {ConnectionId} attached to role group role_{Role}", Context.ConnectionId, role);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("SignalR client {ConnectionId} disconnected.", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinBranchGroup(int branchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"branch_{branchId}");
        _logger.LogInformation("Client {ConnectionId} joined branch group branch_{BranchId}", Context.ConnectionId, branchId);
    }

    public async Task LeaveBranchGroup(int branchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch_{branchId}");
        _logger.LogInformation("Client {ConnectionId} left branch group branch_{BranchId}", Context.ConnectionId, branchId);
    }
}
