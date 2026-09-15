using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class DashboardService(HttpClient client, JsonSerializerOptions jsonOptions) : IDashboardService
{
    public async Task<List<TodaySalesSummaryDto>> GetTodaySalesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Dashboard.GetTodaySales(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<TodaySalesSummaryDto>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Dashboard.GetCustomerBalances(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<CustomerBalancesSummaryDto>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Dashboard.GetTradeTrend30Days(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<TradeTrendPointDto>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Dashboard.GetTopUnpaidInvoices(count), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<TopUnpaidInvoiceDto>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}
