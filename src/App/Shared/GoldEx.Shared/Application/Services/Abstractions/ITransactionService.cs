using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface ITransactionService<TTransaction, TCustomer> 
    where TTransaction : TransactionBase<TCustomer> 
    where TCustomer : CustomerBase
{
    Task CreateAsync(TTransaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(TTransaction transaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(TTransaction transaction, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<TTransaction?> GetAsync(TransactionId id, CancellationToken cancellationToken = default);
    Task<TTransaction?> GetAsync(int number, CancellationToken cancellationToken = default);
    Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, Guid customerId, CancellationToken cancellationToken = default);
    Task<List<TTransaction>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
    Task<int> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default);
    Task<(double value, UnitType unit)> GetCustomerRemainingCreditAsync(CustomerId customerId, CancellationToken cancellationToken = default);
}