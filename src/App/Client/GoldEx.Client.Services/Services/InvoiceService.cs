using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Shared.Enums;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class InvoiceService(HttpClient client, JsonSerializerOptions jsonOptions) : IInvoiceService
{
    public async Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Invoices.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, InvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Invoices.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<PagedList<GetInvoiceListResponse>> GetListAsync(RequestFilter filter, InvoiceFilter invoiceFilter,
        Guid? customerId, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.GetList(filter, invoiceFilter, customerId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInvoiceListResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetInvoiceResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetInvoiceResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetInvoiceResponse> GetAsync(long invoiceNumber, InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.Get(invoiceNumber, invoiceType), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetInvoiceResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.Invoices.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<GetInvoiceNumberResponse> GetLastNumberAsync(InvoiceType invoiceType,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.GetLastNumber(invoiceType), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetInvoiceNumberResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}