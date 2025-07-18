using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Transactions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class TransactionService(
    ITransactionRepository repository,
    ICustomerService customerService,
    IMapper mapper,
    ILogger<TransactionService> logger,
    TransactionRequestDtoValidator validator,
    DeleteTransactionValidator deleteValidator) : ITransactionService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter,
        TransactionFilter transactionFilter, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await repository
            .Get(new TransactionsByFilterSpecification(filter,
                transactionFilter,
                customerId.HasValue
                    ? new CustomerId(customerId.Value)
                    : null))
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(new TransactionsByFilterSpecification(filter,
                transactionFilter,
                customerId.HasValue
                    ? new CustomerId(customerId.Value)
                    : null),
            cancellationToken);

        return new PagedList<GetTransactionResponse>
        {
            Data = mapper.Map<List<GetTransactionResponse>>(data),
            Skip = skip,
            Take = take,
            Total = totalCount
        };
    }

    public async Task<GetTransactionResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await repository
            .Get(new TransactionsByIdSpecification(new TransactionId(id)))
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetTransactionResponse>(transaction);
    }

    public async Task<GetTransactionResponse> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        var transaction = await repository
            .Get(new TransactionsByNumberSpecification(number))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetTransactionResponse>(transaction);
    }

    public async Task SetAsync(TransactionRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                Guid customerId;

                if (request.Customer.Id.HasValue)
                {
                    await customerService.UpdateAsync(request.Customer.Id.Value, request.Customer, cancellationToken);
                    customerId = request.Customer.Id.Value;
                }
                else
                {
                    customerId = await customerService.CreateAsync(request.Customer, cancellationToken);
                }

                if (request.Id.HasValue)
                {
                    var existingTransaction = await repository
                        .Get(new TransactionsByIdSpecification(new TransactionId(request.Id.Value)))
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                    existingTransaction.SetCredit(request.Credit);
                    existingTransaction.SetCreditUnit(request.CreditPriceUnitId.HasValue ? new PriceUnitId(request.CreditPriceUnitId.Value) : null);
                    existingTransaction.SetCreditRate(request.CreditRate);
                    existingTransaction.SetDebit(request.Debit);
                    existingTransaction.SetDebitUnit(request.DebitPriceUnitId.HasValue ? new PriceUnitId(request.DebitPriceUnitId.Value) : null);
                    existingTransaction.SetDebitRate(request.DebitRate);
                    existingTransaction.SetDateTime(request.DateTime);
                    existingTransaction.SetPriceUnitId(new PriceUnitId(request.PriceUnitId));
                    existingTransaction.SetNumber(request.Number);
                    existingTransaction.SetDescription(request.Description);
                    existingTransaction.SetCustomer(new CustomerId(customerId));

                    await repository.UpdateAsync(existingTransaction, cancellationToken);
                }
                else
                {
                    var newTransaction = Transaction.Create(request.DateTime,
                        request.Number,
                        request.Description,
                        new PriceUnitId(request.PriceUnitId),
                        request.Credit,
                        request.CreditPriceUnitId.HasValue ? new PriceUnitId(request.CreditPriceUnitId.Value) : null,
                        request.CreditRate,
                        request.Debit,
                        request.DebitPriceUnitId.HasValue ? new PriceUnitId(request.DebitPriceUnitId.Value) : null,
                        request.DebitRate,
                        new CustomerId(customerId));

                    await repository.CreateAsync(newTransaction, cancellationToken);
                }

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while create or update transaction");
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await repository
            .Get(new TransactionsByIdSpecification(new TransactionId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await deleteValidator.ValidateAndThrowAsync(transaction, cancellationToken);

        await repository.DeleteAsync(transaction, cancellationToken);
    }

    public async Task<GetTransactionNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastNumber = await repository.GetLastTransactionNumberAsync(cancellationToken);
        return new GetTransactionNumberResponse(lastNumber);
    }
}