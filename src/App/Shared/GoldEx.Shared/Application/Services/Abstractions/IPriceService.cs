using GoldEx.Shared.Domain.Aggregates.PriceAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IPriceService<TPrice, TPriceHistory> 
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<TPrice?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(TPrice price, CancellationToken cancellationToken = default);
    Task UpdateAsync(TPrice price, CancellationToken cancellationToken = default);
    Task<List<TPrice>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}