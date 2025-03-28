using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.Services;

public interface ICustomerClientService
{
    Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<List<GetPendingCustomerResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}