using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class MoltenGoldTradeService(HttpClient client, JsonSerializerOptions options) : IMoltenGoldTradeService
{
    private const string Url = "/" + ApiRoutes.MoltenGoldDesk;

    public Task<MoltenGoldDeskSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
        => ReadAsync<MoltenGoldDeskSettings>($"{Url}/settings", cancellationToken);

    public Task<List<MoltenGoldStockChoice>> SearchStockAsync(string query, CancellationToken cancellationToken = default)
        => ReadAsync<List<MoltenGoldStockChoice>>($"{Url}/stock?query={Uri.EscapeDataString(query)}", cancellationToken);

    public Task<List<MoltenGoldSettlementChoice>> SearchSettlementsAsync(Guid id, string query, CancellationToken cancellationToken = default)
        => ReadAsync<List<MoltenGoldSettlementChoice>>($"{Url}/{id}/settlements?query={Uri.EscapeDataString(query)}", cancellationToken);

    public Task SettleGoldAsync(Guid id, SettleMoltenGold request, CancellationToken cancellationToken = default)
        => PostAsync($"{Url}/{id}/gold-settlement", request, cancellationToken);

    public Task<List<MoltenGoldPaymentChoice>> SearchPaymentTradesAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default)
        => ReadAsync<List<MoltenGoldPaymentChoice>>($"{Url}/{id}/payment-trades?customerId={customerId}", cancellationToken);

    public Task<List<MoltenGoldTradeResponse>> GetListAsync(MoltenGoldTradeFilter filter, int skip, CancellationToken cancellationToken = default)
        => ReadAsync<List<MoltenGoldTradeResponse>>($"{Url}?filter={(int)filter}&skip={skip}", cancellationToken);

    public Task CreateAsync(CreateMoltenGoldTrade request, CancellationToken cancellationToken = default)
        => PostAsync(Url, request, cancellationToken);

    public Task DeliverAsync(Guid id, DeliverMoltenGold request, CancellationToken cancellationToken = default)
        => PostAsync($"{Url}/{id}/delivery", request, cancellationToken);

    public Task PayAsync(Guid id, PayMoltenGold request, CancellationToken cancellationToken = default)
        => PostAsync($"{Url}/{id}/payment", request, cancellationToken);

    public Task TransferAsync(Guid id, TransferMoltenGold request, CancellationToken cancellationToken = default)
        => PostAsync($"{Url}/{id}/transfer", request, cancellationToken);

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync($"{Url}/{id}", cancellationToken);
        Check(response);
    }

    private async Task<T> ReadAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(url, cancellationToken);
        Check(response);
        return await response.Content.ReadFromJsonAsync<T>(options, cancellationToken) ?? throw new UnexpectedHttpResponseException();
    }

    private async Task PostAsync<T>(string url, T request, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(url, request, options, cancellationToken);
        Check(response);
    }

    private static void Check(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
