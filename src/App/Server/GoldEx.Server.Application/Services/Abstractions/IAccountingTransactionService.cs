using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.DTOs.MeltingBatches;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);

    Task ClearTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);

    Task ClearTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task SetForMeltingBatchRequestAsync(MeltingBatchId meltingBatchId,
        List<ProductId> productIds,
        CancellationToken cancellationToken = default);

    Task SetForMoltenGoldEntryAsync(MeltingBatch meltingBatch,
        CompleteMeltingRequestDto request,
        CancellationToken cancellationToken = default);

    Task ReplaceTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        Product product,
        CreateProductItemEntryRequest productItemEntryRequest,
        CancellationToken cancellationToken = default);

    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCoinItemEntryRequest coinItemEntry,
        CancellationToken cancellationToken = default);

    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCurrencyItemEntryRequest currencyItemEntry,
        CancellationToken cancellationToken = default);

    Task AddWeightChangeTransactionAsync(ProductId id,
        decimal oldWeight,
        decimal newWeight,
        InventoryStockId? outStockId,
        InventoryStockId? inStockId,
        CancellationToken cancellationToken = default);

    Task CreateForInventoryExitAsync(InventoryExitId inventoryExitId,
        CreateInventoryExitRequest request,
        List<InventoryStock> inventoryStocks,
        CancellationToken cancellationToken = default);

    Task CreateTransactionsForCheckAcceptAsync(CheckPayment checkPayment, Guid? targetFinancialAccountId, string? description, CancellationToken cancellationToken = default);
    Task CreateTransactionsForCheckReturnAsync(CheckPayment checkPayment, string? description, CancellationToken cancellationToken = default);
}