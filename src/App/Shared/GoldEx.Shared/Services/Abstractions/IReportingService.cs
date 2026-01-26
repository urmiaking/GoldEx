using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Shared.Services.Abstractions;

public interface IReportingService
{
    /// <summary>
    /// Get ledger account transactions report for given ledger account
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(LedgerAccountStatementRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trial balance for ledger accounts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(LedgerAccountTrialBalanceRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer payable and receivable amounts report and their remaining balance
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<CustomerRemainingBalanceRpResponse>> GetCustomerRemainingBalanceAsync(CustomerRemainingBalanceRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer payable and receivable transactions report for given customer
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<CustomerTransactionRpResponse>> GetCustomerTransactionsAsync(CustomerTransactionRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sell invoice report
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<SellInvoiceRpResponse>> GetSellInvoicesAsync(SellInvoiceRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get purchase invoices report
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<PurchaseInvoiceRpResponse>> GetPurchaseInvoicesAsync(PurchaseInvoiceRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments report
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<PaymentRpResponse>> GetPaymentsAsync(PaymentRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments report of an invoice
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<InvoicePaymentRpResponse>> GetInvoicePaymentsAsync(InvoicePaymentRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory kardex
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<InventoryKardexRpResponse>> GetInventoryKardexAsync(InventoryKardexRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product (gold, jewelry and molten gold) report
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<ProductInventoryRpResponse>> GetProductInventoryAsync(ProductInventoryRpRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get coin instance report in inventory
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<CoinInventoryRpResponse>> GetCoinInventoryAsync(CoinInventoryRpRequest request, CancellationToken cancellationToken = default);

    Task<List<CurrencyInventoryRpResponse>> GetCurrencyInventoryAsync(CurrencyInventoryRpRequest request, CancellationToken cancellationToken = default);
}