using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface IPriceRepository<TPrice, TPriceHistory> : IRepository,
    ICreateRepository<TPrice>,
    IUpdateRepository<TPrice>
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<TPrice?> GetAsync(PriceId id, CancellationToken cancellationToken = default);
    Task<List<TPrice>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
    Task<TPrice?> GetGram18PriceAsync(CancellationToken cancellationToken = default);
    Task<TPrice?> GetUsDollarPriceAsync(CancellationToken cancellationToken = default);
}