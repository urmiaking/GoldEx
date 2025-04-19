using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface ITransactionRepository<TTransaction, TCustomer> : IRepository,
    ICreateRepository<TTransaction>,
    IUpdateRepository<TTransaction>,
    IDeleteRepository<TTransaction>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    Task<TTransaction?> GetAsync(TransactionId id, CancellationToken cancellationToken = default);
    Task<TTransaction?> GetAsync(int number, CancellationToken cancellationToken = default);
    Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, Guid customerId, CancellationToken cancellationToken = default);
    Task<List<TTransaction>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
    Task<int> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default);
}