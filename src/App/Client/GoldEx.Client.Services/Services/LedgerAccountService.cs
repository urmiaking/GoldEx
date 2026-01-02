using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class LedgerAccountService(HttpClient client, JsonSerializerOptions jsonOptions) : ILedgerAccountService
{
    public async Task<List<GetLedgerAccountResponse>> GetListAsync(Guid? customerId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.LedgerAccounts.GetList(customerId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetLedgerAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetLedgerAccountResponse>> GetTitlesAsync(FinancialAccountType? financialAccountType, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.LedgerAccounts.GetTitles(financialAccountType), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetLedgerAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetLedgerAccountResponse>> GetActiveLedgerAccountsAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.LedgerAccounts.GetActiveList(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetLedgerAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetLedgerAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.LedgerAccounts.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetLedgerAccountResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CreateAsync(LedgerAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.LedgerAccounts.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, LedgerAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.LedgerAccounts.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.LedgerAccounts.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}