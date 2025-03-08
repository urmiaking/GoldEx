using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.DTOs.Prices;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class PriceLocalClientService(IPriceService<Price, PriceHistory> priceService, IMapper mapper) : IPriceLocalClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        var prices = await priceService.GetLatestPricesAsync(cancellationToken);

        return mapper.Map<List<GetPriceResponse>>(prices);
    }

    public async Task<GetPriceResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(id, cancellationToken);

        return price is null ? null : mapper.Map<GetPriceResponse>(price);
    }

    public async Task CreateAsync(CreatePriceRequest request, CancellationToken cancellationToken = default)
    {
        var price = new Price(new PriceId(request.Id), request.Title, request.Type, request.IconFileBase64);

        price.SetPriceHistory(new PriceHistory(double.Parse(request.Value), request.LastUpdate, request.Change, request.Unit));

        await priceService.CreateAsync(price, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdatePriceRequest request, CancellationToken cancellationToken = default)
    {
        var price = await priceService.GetAsync(id, cancellationToken) ?? throw new NotFoundException();

        price.SetTitle(request.Title);
        price.SetMarketType(request.Type);
        if (!string.IsNullOrEmpty(request.IconFileBase64))
            price.SetIconFile(request.IconFileBase64);

        price.SetPriceHistory(new PriceHistory(double.Parse(request.Value), request.LastUpdate, request.Change, request.Unit));

        await priceService.UpdateAsync(price, cancellationToken);
    }

    public Task<List<GetPriceResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}