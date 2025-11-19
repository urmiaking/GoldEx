using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class InventoryEntryService(HttpClient client, JsonSerializerOptions jsonOptions) : IInventoryEntryService
{
    public async Task CreateAsync(CreateInventoryEntryRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.InventoryEntries.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<ProcessExcelResponse> ProcessExcelAsync(ProcessExcelRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.InventoryEntries.ProcessExcel(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<ProcessExcelResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}