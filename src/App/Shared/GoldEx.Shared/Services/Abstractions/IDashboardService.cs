using GoldEx.Shared.DTOs.Dashboard;

namespace GoldEx.Shared.Services.Abstractions;

public interface IDashboardService
{
    Task<List<TodaySalesSummaryDto>> GetTodaySalesAsync(CancellationToken cancellationToken = default);
    Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default);
    Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default);
}
