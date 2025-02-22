using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceHistoryService(IPriceHistoryRepository repository) : IPriceHistoryService
{
    public Task CreateAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default)
        => repository.CreateAsync(priceHistory, cancellationToken);

    public Task<int> CleanupAsync(CancellationToken cancellationToken = default)
        => repository.CleanupAsync(cancellationToken);
    
}