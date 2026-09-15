using GoldEx.Shared.DTOs.Dashboard;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IDashboardRepository
{
    Task<List<TodaySalesSummaryDto>> GetTodaySalesSummaryAsync(CancellationToken cancellationToken = default);
    Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default);
    Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default);
}
