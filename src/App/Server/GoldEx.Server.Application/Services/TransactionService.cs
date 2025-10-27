using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class TransactionService(IMapper mapper,
    ITransactionRepository repository) : ITransactionService
{
    public async Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var balances = await repository.GetCustomerRemainingListAsync(new CustomerId(customerId), cancellationToken: cancellationToken);

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
}