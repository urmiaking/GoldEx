using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class CheckPaymentService(HttpClient client, JsonSerializerOptions jsonOptions) : ICheckPaymentService
{
    public async Task<PagedList<GetCheckPaymentListResponse>> GetListAsync(
        RequestFilter filter,
        CheckPaymentFilter checkPaymentFilter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CheckPayments.GetList(filter, checkPaymentFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetCheckPaymentListResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task AcceptAsync(Guid checkPaymentId, AcceptCheckPaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.CheckPayments.Accept(checkPaymentId), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task ReturnAsync(Guid checkPaymentId, ReturnCheckPaymentRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.CheckPayments.Return(checkPaymentId), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
