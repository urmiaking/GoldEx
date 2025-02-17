using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPriceRepository : IRepository,
    ICreateRepository<Price>,
    IUpdateRepository<Price>
{
    Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default);
    Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default);
}