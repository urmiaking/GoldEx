using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

//TODO: add validators
[ScopedService]
public class PriceService(IPriceRepository priceRepository) : IPriceService
{
    public Task CreateAsync(Price price, CancellationToken cancellationToken = default)
        => priceRepository.CreateAsync(price, cancellationToken);

    public Task UpdateAsync(Price price, CancellationToken cancellationToken = default)
        => priceRepository.UpdateAsync(price, cancellationToken);

    public Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
        => priceRepository.GetLatestPricesAsync(cancellationToken);

    public Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default)
        => priceRepository.GetListAsync(cancellationToken);
}