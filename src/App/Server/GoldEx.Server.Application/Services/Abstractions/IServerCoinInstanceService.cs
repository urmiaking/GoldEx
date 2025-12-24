using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Invoices;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerCoinInstanceService
{
    Task SyncCoinItemsAsync(Invoice invoice, IEnumerable<InvoiceCoinItemDto> requestedItems, CancellationToken cancellationToken = default);
    Task<CoinInstance> CreateCoinAsync(CoinInstanceRequestDto request, CancellationToken cancellationToken = default);
}