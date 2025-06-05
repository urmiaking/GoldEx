using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services;

public interface IPriceService
{
    Task<List<GetPriceResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<GetPriceTitleResponse>> GetTitlesAsync(MarketType[] marketTypes, CancellationToken cancellationToken = default);
    Task<List<GetPriceResponse>> GetListAsync(MarketType marketType, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetAsync(UnitType unitType, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetAsync(Guid priceUnitId, CancellationToken cancellationToken = default);
    Task<List<GetPriceSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, UpdatePriceStatusRequest request, CancellationToken cancellationToken = default);
}