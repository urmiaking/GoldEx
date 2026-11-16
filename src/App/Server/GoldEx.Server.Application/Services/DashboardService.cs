using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class DashboardService(IDashboardRepository dashboardRepository, StoreContext storeContext,
    IHttpContextAccessor httpContextAccessor) : IDashboardService
{
    private static readonly TimeZoneInfo Tehran = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");

    public Task<DashboardOverviewResponse> GetOverviewAsync(int days, CancellationToken cancellationToken = default)
    {
        if (days is not (7 or 30 or 90))
            throw new ArgumentOutOfRangeException(nameof(days));

        // Server rendering can call the application service from a component's child scope.
        if (!storeContext.StoreId.HasValue &&
            httpContextAccessor.HttpContext?.RequestServices.GetService<IStoreContext>() is { StoreId: Guid id, StoreSlug: string slug })
            storeContext.SetStore(id, slug);

        if (!storeContext.StoreId.HasValue)
            throw new InvalidOperationException("فروشگاه برای نمایش آمار مشخص نشده است.");

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tehran).DateTime);
        return dashboardRepository.GetOverviewAsync(today, days, cancellationToken);
    }

    public Task<List<TodaySalesSummaryDto>> GetTodaySalesAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTodaySalesSummaryAsync(cancellationToken);

    public Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetCustomerBalancesSummaryAsync(cancellationToken);

    public Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTradeTrend30DaysAsync(cancellationToken);

    public Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default) =>
        dashboardRepository.GetTopUnpaidInvoicesAsync(count, cancellationToken);
}
