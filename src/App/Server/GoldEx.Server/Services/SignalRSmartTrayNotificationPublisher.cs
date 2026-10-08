using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Hubs;
using GoldEx.Shared.Contracts.Hubs;
using GoldEx.Shared.DTOs.SmartTrays;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Services;

[ScopedService]
public class SignalRSmartTrayNotificationPublisher(
    IHubContext<SmartTrayHub, ISmartTrayHubClient> hubContext,
    ILogger<SignalRSmartTrayNotificationPublisher> logger) : ISmartTrayNotificationPublisher
{
    public async Task TrayUpdatedAsync(SmartTrayDto tray, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.TrayUpdated(tray);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast TrayUpdated via SignalR.");
        }
    }

    public async Task ItemAddedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.ItemAdded(trayId, item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast ItemAdded via SignalR.");
        }
    }

    public async Task ItemReturnedAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.ItemReturned(trayId, item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast ItemReturned via SignalR.");
        }
    }

    public async Task ItemSoldAsync(Guid trayId, SmartTrayItemDto item, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.ItemSold(trayId, item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast ItemSold via SignalR.");
        }
    }

    public async Task ItemRemovedAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.ItemRemoved(trayId, barcode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast ItemRemoved via SignalR.");
        }
    }

    public async Task DiscrepancyReportedAsync(Guid trayId, string notes, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.DiscrepancyReported(trayId, notes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast DiscrepancyReported via SignalR.");
        }
    }

    public async Task TrayClosedAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.All.TrayClosed(trayId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast TrayClosed via SignalR.");
        }
    }
}
