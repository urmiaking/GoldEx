using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IMoltenGoldTradeRepository : IRepository<MoltenGoldTrade>, ICreateRepository<MoltenGoldTrade>
{
    IQueryable<MoltenGoldTrade> Query { get; }
    IQueryable<MoltenGoldTradeEvent> Events { get; }
    Task LockAsync(CancellationToken cancellationToken);
    Task AddEventAsync(MoltenGoldTradeEvent entry, CancellationToken cancellationToken);
}
