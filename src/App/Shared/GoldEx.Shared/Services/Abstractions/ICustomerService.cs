using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.Services.Abstractions;

public interface ICustomerService
{
    Task<PagedList<GetCustomerResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse?> GetAsync(string nationalId, CancellationToken cancellationToken = default);
    Task<GetCustomerResponse> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CustomerRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CustomerRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}