using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceHistoryAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPriceHistoryRepository : IRepository,
    ICreateRepository<PriceHistory>,
    IDeleteRepository<PriceHistory>
{
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}