using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface IPriceService<TPrice, TPriceHistory> 
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<List<TPrice>> GetListAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(TPrice price, CancellationToken cancellationToken = default);
    Task UpdateAsync(TPrice price, CancellationToken cancellationToken = default);
}