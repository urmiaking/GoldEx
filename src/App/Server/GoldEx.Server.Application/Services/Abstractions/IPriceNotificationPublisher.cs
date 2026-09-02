using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IPriceNotificationPublisher
{
    Task PublishPriceChangesAsync(List<PriceChangedNotificationDto> changes, CancellationToken cancellationToken = default);
}
