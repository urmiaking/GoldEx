using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IPriceRepository<TPrice, TPriceHistory> : IRepository,
    ICreateRepository<TPrice>,
    IUpdateRepository<TPrice>
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<List<TPrice>> GetListAsync(CancellationToken cancellationToken = default);
}