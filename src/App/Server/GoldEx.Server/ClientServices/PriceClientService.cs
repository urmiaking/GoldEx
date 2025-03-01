using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class PriceClientService(IPriceService<Price, PriceHistory> priceService, IMapper mapper) : IPriceClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        var prices = await priceService.GetLatestPricesAsync(cancellationToken);

        return mapper.Map<List<GetPriceResponse>>(prices);
    }
}