using GoldEx.Shared.DTOs;

namespace GoldEx.Shared.Services;

public interface IPriceClientService
{
    Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
}