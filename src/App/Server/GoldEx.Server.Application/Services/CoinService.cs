using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Coins;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CoinService(ICoinRepository repository,
    IPriceRepository priceRepository,
    IPriceUnitRepository priceUnitRepository,
    IMapper mapper,
    CoinRequestDtoValidator validator) : ICoinService
{
    public async Task<List<GetCoinResponse>> GetListAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new CoinsByStatusSpecification(isActive))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetCoinResponse>>(items);
    }

    public async Task<GetCoinResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetCoinResponse>(item);
    }

    public async Task<GetPriceResponse?> GetPriceAsync(Guid coinId, Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(coinId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (coin.PriceId is null)
            return null;

        var basePriceInRial = await priceRepository
            .Get(new PricesByIdSpecification(coin.PriceId.Value))
            .FirstOrDefaultAsync(cancellationToken);

        if (!priceUnitId.HasValue)
            return basePriceInRial?.PriceHistory is null ? null : mapper.Map<GetPriceResponse>(basePriceInRial);

        if (basePriceInRial?.PriceHistory == null)
            return null;

        var conversionUnit = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)))
            .AsNoTracking()
            .Include(pu => pu.Price)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversionUnit?.Price?.PriceHistory != null && conversionUnit.Price.PriceHistory.CurrentValue != 0)
        {
            var baseValueInRial = basePriceInRial.PriceHistory.CurrentValue;
            var conversionUnitValueInRial = conversionUnit.Price.PriceHistory.CurrentValue;

            var convertedValue = baseValueInRial / conversionUnitValueInRial;

            return new GetPriceResponse(
                Id: basePriceInRial.Id.Value,
                Title: basePriceInRial.Title,
                Value: convertedValue.ToString("G29"),
                Unit: conversionUnit.Title,
                Change: basePriceInRial.PriceHistory.DailyChangeRate,
                LastUpdate: basePriceInRial.PriceHistory.LastUpdate,
                HasIcon: false,
                Type: basePriceInRial.MarketType,
                UnitType: basePriceInRial.UnitType
            );
        }

        return null;
    }

    public async Task CreateAsync(CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var coin = Coin.Create(request.Title, request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.CreateAsync(coin, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        coin.SetTitle(request.Title);
        coin.SetPriceId(request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.UpdateAsync(coin, cancellationToken);
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        coin.SetStatus(isActive);

        await repository.UpdateAsync(coin, cancellationToken);
    }
}