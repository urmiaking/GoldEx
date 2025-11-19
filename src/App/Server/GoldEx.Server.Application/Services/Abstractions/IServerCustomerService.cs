using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerCustomerService
{
    Task<GetCustomerResponse> GetOrCreateAsync(string customerName, CustomerType type, CancellationToken cancellationToken = default);
}