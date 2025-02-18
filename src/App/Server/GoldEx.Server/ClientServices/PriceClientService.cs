using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class PriceClientService(IPriceService priceService, IMapper mapper) : IPriceClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        var prices = await priceService.GetLatestPricesAsync(cancellationToken);

        return mapper.Map<List<GetPriceResponse>>(prices);
    }
}