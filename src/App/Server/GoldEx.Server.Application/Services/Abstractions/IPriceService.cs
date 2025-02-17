using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IPriceService
{
    Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Price price, CancellationToken cancellationToken = default);
    Task UpdateAsync(Price price, CancellationToken cancellationToken = default);
}