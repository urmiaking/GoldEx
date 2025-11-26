using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class TransactionService(IMapper mapper,
    ITransactionRepository repository,
    IFinancialAccountRepository financialAccountRepository) : ITransactionService
{
    public async Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId,
        Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var balances = await repository.GetCustomerRemainingListAsync(new CustomerId(customerId),
            priceUnitId: priceUnitId.HasValue
                ? new PriceUnitId(priceUnitId.Value)
                : null,
            cancellationToken: cancellationToken);

        return balances
            .Select(x => new GetCustomerRemainingResponse(mapper.Map<GetPriceUnitTitleResponse>(x.Key), x.Value))
            .ToList();
    }

    public async Task<PagedList<GetTransactionResponse>> GetListAsync(TransactionFilter transactionFilter, RequestFilter requestFilter,
        CancellationToken cancellationToken = default)
    {
        var spec = new TransactionsByFilterSpecification(transactionFilter, requestFilter);

        var list = await repository
            .Get(spec)
            .ToListAsync(cancellationToken);

        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetTransactionResponse>
        {
            Data = mapper.Map<List<GetTransactionResponse>>(list),
            Total = total,
            Skip = requestFilter.Skip ?? 0,
            Take = requestFilter.Take ?? 15
        };
    }

    public async Task<GetFinancialAccountBalanceResponse> GetFinancialAccountBalanceAsync(Guid financialAccountId, CancellationToken cancellationToken = default)
    {
        var financialAccount = await financialAccountRepository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(financialAccountId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
                              ?? throw new NotFoundException("Financial account not found.");

        if (!financialAccount.LedgerAccountId.HasValue)
            return new GetFinancialAccountBalanceResponse(0);

        var amount = await repository
                    .Get(new TransactionsByLedgerAccountIdSpecification(financialAccount.LedgerAccountId.Value, skipReversed: true))
                    .AsNoTracking()
                    .Select(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
                    .SumAsync(cancellationToken);

        return new GetFinancialAccountBalanceResponse(amount);
    }
}