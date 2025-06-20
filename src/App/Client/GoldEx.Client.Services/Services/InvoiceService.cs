using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class InvoiceService(HttpClient client, JsonSerializerOptions jsonOptions) : IInvoiceService
{
    public async Task CreateAsync(InvoiceRequestDto request, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Invoices.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<PagedList<GetInvoiceResponse>> GetListAsync(RequestFilter filter, Guid? customerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Invoices.GetList(filter, customerId), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInvoiceResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}