using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Shared.Services.Abstractions;

public interface ITransactionService
{
    Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default);
}