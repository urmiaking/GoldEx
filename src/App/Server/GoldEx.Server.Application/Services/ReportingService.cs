using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ReportingService(ITransactionRepository transactionRepository, IMapper mapper) : IReportingService
{
    public async Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository
            .Get(new TransactionDefaultSpecification())
            .Include(x => x.PriceUnit)
            .Where(x => x.LedgerAccountId == new LedgerAccountId(request.LedgerAccountId))
            .Where(x => request.PriceUnitId == null || x.PriceUnitId == new PriceUnitId(request.PriceUnitId.Value))
            .Where(x => request.FromDate == null || x.PostingDate >= request.FromDate)
            .Where(x => request.ToDate == null || x.PostingDate <= request.ToDate)
            .OrderBy(x => x.PostingDate)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<LedgerAccountStatementRpResponse>>(list);
    }

    public async Task<List<LedgerAccountTrialBalanceRpResponse>> GetLedgerAccountTrialBalanceAsync(LedgerAccountTrialBalanceRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var list = await transactionRepository.GetLedgerAccountTrialBalanceAsync(request, cancellationToken);
        return mapper.Map<List<LedgerAccountTrialBalanceRpResponse>>(list.Nodes);
    }
}