using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerLedgerAccountService
{
    Task<LedgerAccount> GetOrCreateCustomerSubLedgerAsync(
        CustomerId customerId,
        PriceUnitId priceUnitId,
        LedgerAccountRole role,
        CancellationToken cancellationToken = default);
}