using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface ICustomerService<TCustomer> where TCustomer : CustomerBase
{
    Task CreateAsync(TCustomer customer, CancellationToken cancellationToken = default);
    Task UpdateAsync(TCustomer customer, CancellationToken cancellationToken = default);
    Task DeleteAsync(TCustomer customer, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<TCustomer?> GetAsync(CustomerId id, CancellationToken cancellationToken = default);
    Task<TCustomer?> GetAsync(string nationalId, CancellationToken cancellationToken = default);
    Task<TCustomer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<PagedList<TCustomer>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<List<TCustomer>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}