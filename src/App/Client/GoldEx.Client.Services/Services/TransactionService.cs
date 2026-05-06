using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class TransactionService(HttpClient client, JsonSerializerOptions jsonOptions) : ITransactionService
{
    public async Task<List<GetCustomerRemainingResponse>> GetCustomerRemainingListAsync(Guid customerId,
        Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetRemainingList(customerId, priceUnitId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetCustomerRemainingResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<PagedList<GetTransactionResponse>> GetListAsync(TransactionFilter transactionFilter, RequestFilter requestFilter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetList(transactionFilter, requestFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetTransactionResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetFinancialAccountBalanceResponse> GetFinancialAccountBalanceAsync(Guid financialAccountId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetFinancialAccountBalance(financialAccountId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetFinancialAccountBalanceResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetAccountBalanceResponse>> GetAccountBalanceAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetAccountBalance(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetAccountBalanceResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetPriceUnitTitleResponse>> GetAvailablePriceUnitsAsync(TransactionFilter transactionFilter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetAvailablePriceUnits(transactionFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetPriceUnitTitleResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetTopCustomerResponse>> GetTopCustomersAsync(TransactionType transactionType, Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Transactions.GetTopCustomers(transactionType, priceUnitId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetTopCustomerResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}