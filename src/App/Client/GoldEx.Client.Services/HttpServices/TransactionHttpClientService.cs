using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Sdk.Client.Extensions;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Services.HttpServices;

[ScopedService]
public class TransactionHttpClientService(HttpClient client, JsonSerializerOptions jsonOptions) : ITransactionHttpService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetList(filter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetTransactionResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, Guid customerId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetList(filter, customerId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetTransactionResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetTransactionResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetTransactionResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetTransactionResponse?> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetByNumber(number), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetTransactionResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<bool> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.PostAsJsonAsync(ApiUrls.Transactions.Create(), request, jsonOptions, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            return true;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return false; // server is not available

            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.PutAsJsonAsync(ApiUrls.Transactions.Update(id), request, jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            return true;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return false; // server is not available

            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = bad)
    {
        try
        {
            using var response = await client.DeleteAsync(ApiUrls.Transactions.Delete(id), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            return true;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return false; // server is not available

            throw;
        }
    }

    public async Task<List<GetPendingTransactionResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.GetAsync(ApiUrls.Transactions.GetPendingItems(checkpointDate), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            var result = await response.Content.ReadFromJsonAsync<List<GetPendingTransactionResponse>>(jsonOptions, cancellationToken);

            return result ?? throw new UnexpectedHttpResponseException();
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return []; // server is not available

            throw;
        }
    }

    public async Task<GetTransactionNumberResponse> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetLatestTransactionNumber(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetTransactionNumberResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetCustomerRemainingCreditResponse?> GetCustomerRemainingCreditAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetCustomerRemainingCredit(customerId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetCustomerRemainingCreditResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}