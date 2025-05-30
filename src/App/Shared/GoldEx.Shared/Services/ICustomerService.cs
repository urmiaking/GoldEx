using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.Services;

public interface ICustomerService
{
    Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetAsync(string nationalId, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}