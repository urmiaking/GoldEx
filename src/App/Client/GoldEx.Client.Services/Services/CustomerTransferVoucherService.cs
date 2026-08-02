using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class CustomerTransferVoucherService(HttpClient client, JsonSerializerOptions jsonOptions) : ICustomerTransferVoucherService
{
    public async Task<PagedList<GetCustomerTransferVoucherListResponse>> GetListAsync(
        RequestFilter filter,
        CustomerTransferVoucherFilter voucherFilter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CustomerTransfers.GetList(filter, voucherFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetCustomerTransferVoucherListResponse>>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetCustomerTransferVoucherResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CustomerTransfers.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetCustomerTransferVoucherResponse>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetCustomerTransferVoucherResponse> GetAsync(long voucherNumber, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CustomerTransfers.GetByNumber(voucherNumber), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetCustomerTransferVoucherResponse>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CreateAsync(CreateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.CustomerTransfers.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerTransferVoucherRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.CustomerTransfers.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.CustomerTransfers.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<GetVoucherNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CustomerTransfers.GetLastNumber(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetVoucherNumberResponse>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }
}
