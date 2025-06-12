using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services;

public interface IPriceService
{
    Task<List<GetPriceResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<GetPriceTitleResponse>> GetTitlesAsync(MarketType[] marketTypes, CancellationToken cancellationToken = default);
    Task<List<GetPriceResponse>> GetListAsync(MarketType marketType, CancellationToken cancellationToken = default);
    /// <summary>
    /// Gets the price for a specific unit type based on priceUnitId if provided. Default PriceUnitId is rial (IRR).
    /// </summary>
    /// <param name="unitType"></param>
    /// <param name="priceUnitId">Target price unit id that unit type should be converted to it.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetPriceResponse?> GetAsync(UnitType unitType, Guid? priceUnitId, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetAsync(Guid priceUnitId, CancellationToken cancellationToken = default);
    Task<GetExchangeRateResponse> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId,
        CancellationToken cancellationToken = default);
    Task<List<GetPriceSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, UpdatePriceStatusRequest request, CancellationToken cancellationToken = default);
}