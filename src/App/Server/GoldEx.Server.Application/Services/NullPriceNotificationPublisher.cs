using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Server.Application.Services;

public class NullPriceNotificationPublisher : IPriceNotificationPublisher
{
    public Task PublishPriceChangesAsync(List<PriceChangedNotificationDto> changes, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
