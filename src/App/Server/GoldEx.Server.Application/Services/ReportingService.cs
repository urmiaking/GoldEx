using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(ITransactionRepository transactionRepository, IMapper mapper) : IReportingService
{
    public async Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var models = await transactionRepository.GetLedgerAccountStatementsAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountStatementRpResponse>>(models);
    }

    public async Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountTrialBalanceRpResponse>>(list.Nodes);
    }

    public async Task<List<CustomerRemainingBalanceRpResponse>> GetCustomerRemainingBalanceAsync(CustomerRemainingBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerRemainingBalanceAsync(request, cancellationToken);
        return mapper.Map<List<CustomerRemainingBalanceRpResponse>>(list);
    }

    public async Task<List<CustomerTransactionRpResponse>> GetCustomerTransactionsAsync(CustomerTransactionRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetCustomerTransactionsAsync(request, cancellationToken);
        return mapper.Map<List<CustomerTransactionRpResponse>>(list);
    }
}