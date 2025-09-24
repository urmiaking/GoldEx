using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class TransactionService(IMapper mapper,
    ITransactionRepository repository) : ITransactionService
{
    public async Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var balances = await repository.GetCustomerRemainingListAsync(new CustomerId(customerId), cancellationToken);

        return balances
            .Select(x => new GetCustomerRemainingResponse(mapper.Map<GetPriceUnitTitleResponse>(x.Key), x.Value))
            .ToList();
    }

    //public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter,
    //    TransactionFilter transactionFilter, Guid? customerId, CancellationToken cancellationToken = default)
    //{
    //    var skip = filter.Skip ?? 0;
    //    var take = filter.Take ?? 100;

    //    var data = await repository
    //        .Get(new TransactionsByFilterSpecification(filter,
    //            transactionFilter,
    //            customerId.HasValue
    //                ? new CustomerId(customerId.Value)
    //                : null))
    //        .ToListAsync(cancellationToken);

    //    var totalCount = await repository.CountAsync(new TransactionsByFilterSpecification(filter,
    //            transactionFilter,
    //            customerId.HasValue
    //                ? new CustomerId(customerId.Value)
    //                : null),
    //        cancellationToken);

    //    return new PagedList<GetTransactionResponse>
    //    {
    //        Data = mapper.Map<List<GetTransactionResponse>>(data),
    //        Skip = skip,
    //        Take = take,
    //        Total = totalCount
    //    };
    //}

}