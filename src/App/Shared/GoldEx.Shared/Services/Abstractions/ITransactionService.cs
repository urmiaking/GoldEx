using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Shared.Services.Abstractions;

public interface ITransactionService
{
    Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<PagedList<GetTransactionResponse>> GetListAsync(TransactionFilter transactionFilter, RequestFilter requestFilter, CancellationToken cancellationToken = default);
    Task<GetFinancialAccountBalanceResponse> GetFinancialAccountBalanceAsync(Guid financialAccountId, CancellationToken cancellationToken = default);
}