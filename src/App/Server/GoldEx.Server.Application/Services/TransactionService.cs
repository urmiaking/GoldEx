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

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class TransactionService(
    ITransactionRepository repository,
    ICustomerService customerService,
    IMapper mapper,
    CreateTransactionRequestValidator createValidator,
    UpdateTransactionRequestValidator updateValidator,
    DeleteTransactionValidator deleteValidator) : ITransactionService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, Guid? customerId, CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        var data = await repository
            .Get(new TransactionsByFilterSpecification(filter,
                customerId.HasValue
                    ? new CustomerId(customerId.Value)
                    : null))
            .ToListAsync(cancellationToken);

        var totalCount = await repository.CountAsync(new TransactionsByFilterSpecification(filter,
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

    public async Task CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

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

        var transaction = Transaction.Create(request.DateTime,
            request.Number,
            request.Description,
            request.Credit,
            request.CreditPriceUnitId.HasValue ? new PriceUnitId(request.CreditPriceUnitId.Value) : null,
            request.CreditRate,
            request.Debit,
            request.DebitPriceUnitId.HasValue ? new PriceUnitId(request.DebitPriceUnitId.Value) : null,
            request.DebitRate,
            new CustomerId(customerId));

        await repository.CreateAsync(transaction, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

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

        var transaction = await repository
            .Get(new TransactionsByIdSpecification(new TransactionId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        transaction.SetCredit(request.Credit);
        transaction.SetCreditUnit(request.CreditPriceUnitId.HasValue ? new PriceUnitId(request.CreditPriceUnitId.Value) : null);
        transaction.SetCreditRate(request.CreditRate);
        transaction.SetDebit(request.Debit);
        transaction.SetDebitUnit(request.DebitPriceUnitId.HasValue ? new PriceUnitId(request.DebitPriceUnitId.Value) : null);
        transaction.SetDebitRate(request.DebitRate);
        transaction.SetDateTime(request.DateTime);
        transaction.SetNumber(request.Number);
        transaction.SetDescription(request.Description);
        transaction.SetCustomer(new CustomerId(customerId));

        await repository.UpdateAsync(transaction, cancellationToken);
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