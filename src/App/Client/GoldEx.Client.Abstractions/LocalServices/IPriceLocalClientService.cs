using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface IPriceLocalClientService : IPriceClientService
{
    Task<GetPriceResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(CreatePriceRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdatePriceRequest request, CancellationToken cancellationToken = default);
}