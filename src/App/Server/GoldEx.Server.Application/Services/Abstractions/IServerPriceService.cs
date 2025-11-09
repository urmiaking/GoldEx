using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerPriceService
{
    Task AddOrUpdateAsync(List<PriceResponse> incomingPriceList, CancellationToken cancellationToken = default);
}