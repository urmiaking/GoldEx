using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerInventoryStockService
{
    Task SetForProductAsync(ProductId productId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default);
    Task SetForCoinAsync(CoinId coinId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default);
    Task SetForCurrencyAsync(PriceUnitId currencyId, decimal amount, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default);
    Task RemoveInventoryByInvoiceIdAsync(InvoiceId invoiceId, ItemType? itemType, CancellationToken cancellationToken = default);
    Task UpdateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task CreateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task MeltProductsAsync(MeltingBatchId meltingBatchId, List<ProductId> productIds,
        CancellationToken cancellationToken = default);
    Task CreateMoltenGoldAsync(MeltingBatch meltingBatch, string assayNumber, decimal fineness, decimal weight,
        CancellationToken cancellationToken = default);
}