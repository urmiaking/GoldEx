using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IPriceStateService
{
    Task<List<GetPriceResponse>> GetListAsync(bool? isPinned = null, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetAsync(GoldUnitType unitType, Guid? priceUnitId, bool applySafetyMargin, CancellationToken cancellationToken = default);
    Task<GetExchangeRateResponse> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId, CancellationToken cancellationToken = default);
    
    Task RefreshAsync(CancellationToken cancellationToken = default);
    
    event Action? OnPricesUpdated;
}
