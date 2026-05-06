using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface ITransactionService
{
    Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId, Guid? priceUnitId, CancellationToken cancellationToken = default);
    Task<PagedList<GetTransactionResponse>> GetListAsync(TransactionFilter transactionFilter, RequestFilter requestFilter, CancellationToken cancellationToken = default);
    Task<GetFinancialAccountBalanceResponse> GetFinancialAccountBalanceAsync(Guid financialAccountId, CancellationToken cancellationToken = default);
    Task<List<GetAccountBalanceResponse>> GetAccountBalanceAsync(CancellationToken cancellationToken = default);
    Task<List<GetPriceUnitTitleResponse>> GetAvailablePriceUnitsAsync(TransactionFilter transactionFilter, CancellationToken cancellationToken = default);
    Task<List<GetTopCustomerResponse>> GetTopCustomersAsync(TransactionType transactionType, Guid? priceUnitId, CancellationToken cancellationToken = default);
}