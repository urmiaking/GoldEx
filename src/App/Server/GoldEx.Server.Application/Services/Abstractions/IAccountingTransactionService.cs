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

using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
#line 19
    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
#line 21
    Task ClearTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
#line 23
    Task ClearTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task SetForMeltingBatchRequestAsync(MeltingBatchId meltingBatchId,
        List<ProductId> productIds,
        CancellationToken cancellationToken = default);
#line 28
    Task SetForMoltenGoldEntryAsync(MeltingBatch meltingBatch,
        CompleteMeltingRequestDto request,
        CancellationToken cancellationToken = default);
#line 32
    Task ReplaceTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
#line 34
    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        Product product,
        CreateProductItemEntryRequest productItemEntryRequest,
        CancellationToken cancellationToken = default);
#line 40
    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCoinItemEntryRequest coinItemEntry,
        CancellationToken cancellationToken = default);
#line 45
    Task CreateForInventoryEntryAsync(InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCurrencyItemEntryRequest currencyItemEntry,
        CancellationToken cancellationToken = default);
#line 50
    Task AddWeightChangeTransactionAsync(ProductId id,
        decimal oldWeight,
        decimal newWeight,
        InventoryStockId? outStockId,
        InventoryStockId? inStockId,
        CancellationToken cancellationToken = default);
#line 57
    Task CreateForInventoryExitAsync(InventoryExitId inventoryExitId,
        CreateInventoryExitRequest request,
        List<InventoryStock> inventoryStocks,
        CancellationToken cancellationToken = default);
#line 62
    Task CreateTransactionsForCheckAcceptAsync(CheckPayment checkPayment, Guid? targetFinancialAccountId, string? description, decimal? cashingExchangeRate = null, CheckCashingAdjustmentMode? adjustmentMode = null, bool settleDifference = false, Guid? settlementFinancialAccountId = null, CancellationToken cancellationToken = default);
    Task CreateTransactionsForCheckReturnAsync(CheckPayment checkPayment, string? description, CancellationToken cancellationToken = default);
}