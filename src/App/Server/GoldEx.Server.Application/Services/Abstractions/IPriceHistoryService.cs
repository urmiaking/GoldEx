using GoldEx.Server.Domain.PriceHistoryAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IPriceHistoryService
{
    Task CreateAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default);
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}