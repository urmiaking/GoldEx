using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface ICustomerRepository<TCustomer> : IRepository,
    ICreateRepository<TCustomer>,
    IUpdateRepository<TCustomer>,
    IDeleteRepository<TCustomer>
    where TCustomer : CustomerBase
{
    Task<TCustomer?> GetAsync(CustomerId id, CancellationToken cancellationToken = default);
    Task<TCustomer?> GetAsync(string nationalId, CancellationToken cancellationToken = default);
    Task<TCustomer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<PagedList<TCustomer>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<List<TCustomer>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}