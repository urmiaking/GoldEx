using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.Data;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class FinancialAccountService(HttpClient client, JsonSerializerOptions jsonOptions) : IFinancialAccountService
{
    public async Task<List<GetFinancialAccountResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.FinancialAccounts.GetAll(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetFinancialAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<PagedList<GetFinancialAccountResponse>> GetListAsync(RequestFilter filer,
        FinancialAccountFilter financialAccountFilter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.FinancialAccounts.GetList(filer, financialAccountFilter),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetFinancialAccountResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetFinancialAccountTitleResponse>> GetTitlesAsync(Guid? customerId, Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.FinancialAccounts.GetTitles(customerId, priceUnitId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetFinancialAccountTitleResponse>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetFinancialAccountResponse>> GetCustomerAccountsAsync(Guid customerId, Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.FinancialAccounts.GetCustomerAccounts(customerId, priceUnitId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetFinancialAccountResponse>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetFinancialAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.FinancialAccounts.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetFinancialAccountResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.FinancialAccounts.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.FinancialAccounts.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.FinancialAccounts.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}