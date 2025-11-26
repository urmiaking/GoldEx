using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerInventoryStockService
{
    Task RemoveInventoryByInvoiceIdAsync(InvoiceId invoiceId, ItemType? itemType, CancellationToken cancellationToken = default);
    Task CreateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task MeltProductsAsync(MeltingBatchId meltingBatchId, List<ProductId> productIds,
        CancellationToken cancellationToken = default);
    Task CreateMoltenGoldAsync(MeltingBatch meltingBatch, string assayNumber, decimal fineness, decimal weight,
        CancellationToken cancellationToken = default);
    Task ReplaceInventoryForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task<(InventoryStock? OutStock, InventoryStock? InStock)> UpdateStockAsync(ProductId id, decimal weight,
        CancellationToken cancellationToken = default);
}