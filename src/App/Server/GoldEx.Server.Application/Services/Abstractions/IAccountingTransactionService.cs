using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.MeltingBatches;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
    Task ClearTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
    Task ClearTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task SetForMeltingBatchRequestAsync(MeltingBatchId meltingBatchId, List<ProductId> productIds, CancellationToken cancellationToken = default);
    Task SetForMoltenGoldEntryAsync(MeltingBatch meltingBatch,
        CompleteMeltingRequestDto request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delta-based Replace: تضمین ترتیب: original (00:00) => reversal (+1 tick) => repost (+2 ticks)
    /// </summary>
    /// <param name="invoice"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ReplaceTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry, InventoryStock inventoryStock, Product product, CreateProductItemRequest productItemRequest, CancellationToken cancellationToken = default);
    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry, InventoryStock inventoryStock, CreateCoinItemRequest coinItem, CancellationToken cancellationToken = default);
    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry, InventoryStock inventoryStock, CreateCurrencyItemRequest currencyItem, CancellationToken cancellationToken = default);
    Task AddWeightChangeTransactionAsync(ProductId id, decimal oldWeight, decimal newWeight,
        InventoryStockId? outStockId, InventoryStockId? inStockId, CancellationToken cancellationToken = default);
}