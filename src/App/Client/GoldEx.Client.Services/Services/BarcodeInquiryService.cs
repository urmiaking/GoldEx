using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.BarcodeInquiries;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class BarcodeInquiryService(HttpClient client, JsonSerializerOptions jsonOptions) : IBarcodeInquiryService
{
    public async Task<List<GetBarcodeInquiryResponse>> GetListAsync(string? barcode,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.BarcodeInquiries.GetList(barcode), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetBarcodeInquiryResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task InquiryAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.BarcodeInquiries.Inquiry(barcode), jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}