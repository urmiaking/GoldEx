using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Transactions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class TransactionService<TTransaction, TCustomer>(ITransactionRepository<TTransaction, TCustomer> repository,
    CreateTransactionValidator<TTransaction, TCustomer> createValidator,
    UpdateTransactionValidator<TTransaction, TCustomer> updateValidator,
    DeleteTransactionValidator<TTransaction, TCustomer> deleteValidator) : ITransactionService<TTransaction, TCustomer>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    public async Task CreateAsync(TTransaction transaction, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(transaction, cancellationToken);
        await repository.CreateAsync(transaction, cancellationToken);
    }

    public async Task UpdateAsync(TTransaction transaction, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(transaction, cancellationToken);
        await repository.UpdateAsync(transaction, cancellationToken);
    }

    public async Task DeleteAsync(TTransaction transaction, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(transaction, cancellationToken);
        await repository.DeleteAsync(transaction, deletePermanently, cancellationToken);
    }

    public Task<TTransaction?> GetAsync(TransactionId id, CancellationToken cancellationToken = default)
        => repository.GetAsync(id, cancellationToken);

    public Task<TTransaction?> GetAsync(int number, CancellationToken cancellationToken = default)
        => repository.GetAsync(number, cancellationToken);

    public Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, cancellationToken);

    public Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, Guid customerId, CancellationToken cancellationToken = default)
        => repository.GetListAsync(filter, customerId, cancellationToken);

    public Task<List<TTransaction>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
        => repository.GetPendingItemsAsync(checkpointDate, cancellationToken);

    public Task<int> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
        => repository.GetLatestTransactionNumberAsync(cancellationToken);
}