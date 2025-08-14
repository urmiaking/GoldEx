using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Shared.Services.Abstractions;

public interface ICoinService
{
    Task<List<GetCoinResponse>> GetListAsync(bool? isActive, CancellationToken cancellationToken = default);
    Task<GetCoinResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetPriceResponse?> GetPriceAsync(Guid coinId, Guid? priceUnitId, CancellationToken cancellationToken = default);
    Task CreateAsync(CoinRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CoinRequestDto request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}