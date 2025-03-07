using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Shared.Services;

public interface IPriceClientService
{
    Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default);

    Task<List<GetPriceResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}