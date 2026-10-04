using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class DashboardService(IDashboardRepository dashboardRepository) : IDashboardService
{
    public Task<List<TodaySalesSummaryDto>> GetTodaySalesAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTodaySalesSummaryAsync(cancellationToken);

    public Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetCustomerBalancesSummaryAsync(cancellationToken);

    public Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTradeTrend30DaysAsync(cancellationToken);

    public Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTopUnpaidInvoicesAsync(count, cancellationToken);
}
