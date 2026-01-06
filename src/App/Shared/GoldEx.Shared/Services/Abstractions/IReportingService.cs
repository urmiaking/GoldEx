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
    Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trial balance for ledger accounts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(
        LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer payable and receivable amounts report and their remaining balance
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<CustomerRemainingBalanceRpResponse>> GetCustomerRemainingBalanceAsync(
        CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer payable and receivable transactions report for given customer
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<CustomerTransactionRpResponse>> GetCustomerTransactionsAsync(
        CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default);
}