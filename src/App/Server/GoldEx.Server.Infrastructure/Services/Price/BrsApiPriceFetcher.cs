using GoldEx.Shared.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.BrsApi;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Http;

namespace GoldEx.Server.Infrastructure.Services.Price;

public class BrsApiPriceFetcher(HttpClient client, JsonSerializerOptions jsonOptions, IOptions<PriceProviderSetting> settings) : IPriceFetcher
{
    private readonly string _apiKey = settings.Value.BrsApiKey;

    public async Task<List<PriceResponse>> GetPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{ExternalRoutes.BrsApi}?key={_apiKey}";
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("Bad response from Brs API");

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<BrsApiResponse>(json, jsonOptions);

            return BrsApiResponseMapper.MapPrices(data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }
}