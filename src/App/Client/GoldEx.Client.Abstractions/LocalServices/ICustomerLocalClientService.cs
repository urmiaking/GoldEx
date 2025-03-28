using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface ICustomerLocalClientService : ICustomerClientService
{
    Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsSyncedAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsSyncAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
}