using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class ReportingService(HttpClient client, JsonSerializerOptions jsonOptions) : IReportingService
{
    public async Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Reporting.GetLedgerAccountStatements(request), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<LedgerAccountStatementRpResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}