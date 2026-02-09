using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Tjgu;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Server.Infrastructure.Services.Price;

public class TjguPriceFetcher(HttpClient client, JsonSerializerOptions jsonOptions) : IPriceFetcher
{
    public async Task<List<PriceResponse>> GetPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{ExternalRoutes.Tjgu}?rev={Guid.CreateVersion7().ToString()}";
            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("Bad response from Tjgu API");

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<TjguResponse>(json, jsonOptions);

            return TjguResponseMapper.MapPrices(data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return [];
        }
    }
}