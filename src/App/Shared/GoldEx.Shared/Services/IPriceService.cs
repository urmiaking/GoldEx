using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services;

public interface IPriceService
{
    Task<List<GetPriceResponse>> GetAsync(CancellationToken cancellationToken = default);
    Task<List<GetPriceResponse>> GetAsync(MarketType marketType, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetAsync(UnitType unitType, CancellationToken cancellationToken = default);
}