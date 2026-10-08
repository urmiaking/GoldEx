using GoldEx.Shared.Contracts.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Hubs;

[Authorize]
public class SmartTrayHub : Hub<ISmartTrayHubClient>
{
    private readonly ILogger<SmartTrayHub> _logger;

    public SmartTrayHub(ILogger<SmartTrayHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogDebug("SmartTrayHub client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug(exception, "SmartTrayHub client disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
