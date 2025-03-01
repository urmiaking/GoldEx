using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class PriceHistoryService<T>(IPriceHistoryRepository<T> repository)
    : IPriceHistoryService<T> where T : PriceHistoryBase
{
    public Task CreateAsync(T priceHistory, CancellationToken cancellationToken = default)
        => repository.CreateAsync(priceHistory, cancellationToken);

    public Task<int> CleanupAsync(CancellationToken cancellationToken = default)
        => repository.CleanupAsync(cancellationToken);
    
}