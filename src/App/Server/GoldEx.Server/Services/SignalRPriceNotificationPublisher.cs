using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Hubs;
using GoldEx.Shared.Contracts.Hubs;
using GoldEx.Shared.DTOs.Prices;
using Microsoft.AspNetCore.SignalR;

namespace GoldEx.Server.Services;

[ScopedService]
public class SignalRPriceNotificationPublisher(
    IHubContext<PriceHub, IPriceHubClient> hubContext,
    ILogger<SignalRPriceNotificationPublisher> logger) : IPriceNotificationPublisher
{
    public async Task PublishPriceChangesAsync(List<PriceChangedNotificationDto> changes, CancellationToken cancellationToken = default)
    {
        if (changes.Count == 0) return;

        try
        {
            await hubContext.Clients.All.ReceivePriceUpdates(changes);
            //logger.LogInformation("Successfully broadcast {Count} price change updates via SignalR.", changes.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast {Count} price updates via SignalR.", changes.Count);
        }
    }
}
