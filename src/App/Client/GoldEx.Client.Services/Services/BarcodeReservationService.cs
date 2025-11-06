using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class BarcodeReservationService(HttpClient client, JsonSerializerOptions jsonOptions) : IBarcodeReservationService
{
    public async Task<IssueNextBarcodeResponse> IssueNextAsync(IssueNextBarcodeRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.BarcodeReservations.IssueNext(), request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<IssueNextBarcodeResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task ReleaseAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(ApiUrls.BarcodeReservations.Release(barcode), null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}