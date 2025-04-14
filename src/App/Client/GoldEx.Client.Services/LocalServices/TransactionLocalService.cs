using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.CustomerAggregate;
using GoldEx.Client.Offline.Domain.TransactionAggregate;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.DTOs.Transactions;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class TransactionLocalService(ITransactionService<Transaction, Customer> transactionService, ICustomerService<Customer> customerService, IMapper mapper) : ITransactionLocalClientService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await transactionService.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetTransactionResponse>>(list);
    }

    public async Task<GetTransactionResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(new TransactionId(id), cancellationToken);

        return item is null ? null : mapper.Map<GetTransactionResponse>(item);
    }

    public async Task<GetTransactionResponse?> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(number, cancellationToken);

        return item is null ? null : mapper.Map<GetTransactionResponse>(item);
    }   

    public async Task<bool> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = new Transaction(request.DateTime, request.Number, request.Description, request.Credit,
            request.CreditUnit, request.CreditRate, request.Debit, request.DebitUnit, request.DebitRate, new CustomerId(request.CustomerId));

        await transactionService.CreateAsync(transaction, cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionService.GetAsync(new TransactionId(id), cancellationToken);

        if (transaction is null)
            return false;

        transaction.SetNumber(request.Number);
        transaction.SetDescription(request.Description);
        transaction.SetDateTime(request.DateTime);
        transaction.SetCredit(request.Credit);
        transaction.SetCreditUnit(request.CreditUnit);
        transaction.SetCreditRate(request.CreditRate);
        transaction.SetDebit(request.Debit);
        transaction.SetDebitUnit(request.DebitUnit);
        transaction.SetDebitRate(request.DebitRate);
        transaction.SetCustomer(new CustomerId(request.CustomerId));

        // In case the item is synced, status changes to updated otherwise the previous status remains. e,g. Created
        if (transaction.Status == ModifyStatus.Synced)
            transaction.SetStatus(ModifyStatus.Updated);

        await transactionService.UpdateAsync(transaction, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(new TransactionId(id), cancellationToken);

        if (item is null)
            return false;

        // In case the item is created locally and is not synced to server, it will be deleted permanently
        if (item.Status == ModifyStatus.Created)
        {
            await transactionService.DeleteAsync(item, true, cancellationToken);
            return true;
        }

        if (deletePermanently)
        {
            await transactionService.DeleteAsync(item, deletePermanently, cancellationToken);
        }
        else
        {
            item.SetStatus(ModifyStatus.Deleted);
            await transactionService.UpdateAsync(item, cancellationToken);
        }

        return true;
    }

    public async Task<List<GetPendingTransactionResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var items = await transactionService.GetPendingItemsAsync(checkpointDate, cancellationToken);

        return mapper.Map<List<GetPendingTransactionResponse>>(items);
    }

    public async Task<GetTransactionNumberResponse> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
    {
        var number = await transactionService.GetLatestTransactionNumberAsync(cancellationToken);

        return new GetTransactionNumberResponse(number);
    }

    public async Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(new TransactionId(id), cancellationToken)
                   ?? throw new NotFoundException("تراکنش مورد نظر یافت نشد");

        item.SetStatus(ModifyStatus.Synced);

        await transactionService.UpdateAsync(item, cancellationToken);
    }

    public async Task CreateAsSyncedAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = new Transaction(request.DateTime, request.Number, request.Description, request.Credit,
            request.CreditUnit, request.CreditRate, request.Debit, request.DebitUnit, request.DebitRate, new CustomerId(request.CustomerId));

        transaction.SetStatus(ModifyStatus.Synced);

        await transactionService.CreateAsync(transaction, cancellationToken);
    }

    public async Task UpdateAsSyncAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionService.GetAsync(new TransactionId(id), cancellationToken);

        if (transaction is null)
            return;

        transaction.SetNumber(request.Number);
        transaction.SetDescription(request.Description);
        transaction.SetDateTime(request.DateTime);
        transaction.SetCredit(request.Credit);
        transaction.SetCreditUnit(request.CreditUnit);
        transaction.SetCreditRate(request.CreditRate);
        transaction.SetDebit(request.Debit);
        transaction.SetDebitUnit(request.DebitUnit);
        transaction.SetDebitRate(request.DebitRate);

        transaction.SetStatus(ModifyStatus.Synced);

        await transactionService.UpdateAsync(transaction, cancellationToken);
    }
}