using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Shared.Contracts.Hubs;

public interface IPriceHubClient
{
    Task ReceivePriceUpdates(List<PriceChangedNotificationDto> prices);
}
