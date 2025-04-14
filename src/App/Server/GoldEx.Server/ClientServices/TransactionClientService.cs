using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class TransactionClientService(
    ITransactionService<Transaction, Customer> transactionService,
    IMapper mapper) : ITransactionClientService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var list = await transactionService.GetListAsync(filter, cancellationToken);

        return mapper.Map<PagedList<GetTransactionResponse>>(list);
    }

    public async Task<GetTransactionResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(new TransactionId(id), cancellationToken) ??
                   throw new NotFoundException("تراکنش یافت نشد");

        return mapper.Map<GetTransactionResponse>(item);
    }

    public async Task<GetTransactionResponse?> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        var item = await transactionService.GetAsync(number, cancellationToken) ??
                   throw new NotFoundException("تراکنش یافت نشد");

        return mapper.Map<GetTransactionResponse>(item);
    }

    public async Task<bool> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var item = new Transaction(new TransactionId(request.Id),
            request.DateTime,
            request.Number,
            request.Description,
            request.Credit,
            request.CreditUnit,
            request.CreditRate,
            request.Debit,
            request.DebitUnit,
            request.DebitRate,
            new CustomerId(request.CustomerId));

        await transactionService.CreateAsync(item, cancellationToken);
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionService.GetAsync(new TransactionId(id), cancellationToken) ?? throw new NotFoundException("تراکنش یافت نشد");

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

        await transactionService.UpdateAsync(transaction, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionService.GetAsync(new TransactionId(id), cancellationToken) ??
                          throw new NotFoundException("تراکنش يافت نشد");

        await transactionService.DeleteAsync(transaction, deletePermanently, cancellationToken);
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
}