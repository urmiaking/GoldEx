using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Reports;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class ReportService(HttpClient client, JsonSerializerOptions jsonOptions) : IReportService
{
    public async Task<List<GetReportResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Reports.GetList(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetReportResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}