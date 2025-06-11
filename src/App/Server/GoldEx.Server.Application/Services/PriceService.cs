using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceService(
    IPriceRepository repository,
    IPriceUnitRepository priceUnitRepository,
    ISettingRepository settingRepository,
    IMapper mapper,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment) : IServerPriceService,
    IPriceService
{
    #region ServerPriceService

    public async Task AddOrUpdateAsync(List<PriceResponse> incomingPriceList, CancellationToken cancellationToken = default)
    {
        if (!incomingPriceList.Any())
            return;

        var localPrices = await repository.Get(new PricesWithoutSpecification()).ToListAsync(cancellationToken);

        var pricesToCreate = new List<Price>();
        var pricesToUpdate = new List<Price>();
        var downloadTasks = new List<Task<(Price price, byte[]? imageData, string? imageFormat)>>();

        foreach (var incomingPrice in incomingPriceList)
        {
            var existingPrice = localPrices.FirstOrDefault(p => p.Title == incomingPrice.Title);
            if (existingPrice is not null)
            {
                if (existingPrice.PriceHistory is null)
                {
                    existingPrice.CreatePriceHistory(PriceHistory.Create(incomingPrice.CurrentValue, incomingPrice.LastUpdate, incomingPrice.Change, incomingPrice.Unit));
                }
                else
                {
                    if (existingPrice.PriceHistory.CurrentValue == incomingPrice.CurrentValue && existingPrice.PriceHistory.LastUpdate == incomingPrice.LastUpdate)
                        continue;

                    existingPrice.SetPriceHistory(incomingPrice.CurrentValue, incomingPrice.LastUpdate, incomingPrice.Change, incomingPrice.Unit);
                }

                pricesToUpdate.Add(existingPrice);
            }
            else
            {
                var price = Price.Create(incomingPrice.Title,
                    incomingPrice.MarketType,
                    PriceHistory.Create(incomingPrice.CurrentValue,
                        incomingPrice.LastUpdate,
                        incomingPrice.Change,
                        incomingPrice.Unit),
                    UnitTypeMapper.GetUnitType(incomingPrice));

                pricesToCreate.Add(price);
                
                if (!string.IsNullOrEmpty(incomingPrice.IconUrl))
                {
                    downloadTasks.Add(Task.Run(async () => {
                        var result = await ImageConverter.ToByteArrayAsync(incomingPrice.IconUrl);
                        return (price, result.ByteArray, result.ImageFormat);
                    }, cancellationToken));
                }
            }
        }

        if (pricesToCreate.Any())
        {
            await repository.CreateRangeAsync(pricesToCreate, cancellationToken);

            var emptyPriceUnits = await priceUnitRepository.Get(new PriceUnitsWithoutPriceIdSpecification())
                .ToListAsync(cancellationToken);

            var priceTitleToIdMap = pricesToCreate
                .GroupBy(p => p.Title)
                .ToDictionary(g => g.Key, g => g.First().Id);

            var updatedPriceUnits = new List<PriceUnit>();

            foreach (var priceUnit in emptyPriceUnits)
            {
                if (priceTitleToIdMap.TryGetValue(priceUnit.Title, out var matchingPriceId) &&
                    priceUnit.PriceId != matchingPriceId)
                {
                    priceUnit.SetPriceId(matchingPriceId);
                    updatedPriceUnits.Add(priceUnit);
                }
            }

            if (updatedPriceUnits.Any())
                await priceUnitRepository.UpdateRangeAsync(updatedPriceUnits, cancellationToken);
        }

        if (pricesToUpdate.Any()) await repository.UpdateRangeAsync(pricesToUpdate, cancellationToken);

        await Task.WhenAll(downloadTasks);

        foreach (var taskResult in downloadTasks)
        {
            var downloaded = await taskResult;
            if (downloaded.imageData is not null)
            {
                await fileService.SaveLocalFileAsync(webHostEnvironment.GetPriceHistoryIconPath(
                        downloaded.price.Id.Value,
                        downloaded.imageFormat),
                    downloaded.imageData,
                    cancellationToken);

                var relatedPriceUnits = await priceUnitRepository.Get(
                        new PriceUnitsByPriceIdSpecification(downloaded.price.Id))
                    .ToListAsync(cancellationToken);

                foreach (var priceUnit in relatedPriceUnits)
                {
                    await fileService.SaveLocalFileAsync(
                        webHostEnvironment.GetPriceUnitIconPath(
                            priceUnit.Id.Value,
                            downloaded.imageFormat),
                        downloaded.imageData,
                        cancellationToken);
                }
            }
        }
    }

    #endregion

    #region PriceService

    public async Task<List<GetPriceResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesDefaultSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPriceResponse>>(item);
    }

    public async Task<List<GetPriceTitleResponse>> GetTitlesAsync(MarketType[] marketTypes,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.Get(new PricesByMarketTypesSpecification(marketTypes))
            .ToListAsync(cancellationToken);
        return mapper.Map<List<GetPriceTitleResponse>>(items);
    }

    public async Task<List<GetPriceResponse>> GetListAsync(MarketType marketType, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesByMarketTypeSpecification(marketType)).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPriceResponse>>(item);
    }

    public async Task<GetPriceResponse?> GetAsync(UnitType unitType, Guid? priceUnitId, CancellationToken cancellationToken = default)
    {
        var baseItem = await repository.Get(new PricesByUnitTypeSpecification(unitType))
            .FirstOrDefaultAsync(cancellationToken);

        if (baseItem?.PriceHistory is null)
            return null;

        var setting = await settingRepository.Get(new SettingsDefaultSpecification()).FirstOrDefaultAsync(cancellationToken);
        if (setting is not null && setting.GoldSafetyMarginPercent != 0)
        {
            if (baseItem is { UnitType: UnitType.Gold18K })
            {
                var adjustedValue = baseItem.PriceHistory.CurrentValue * (1 + setting.GoldSafetyMarginPercent / 100);
                baseItem.PriceHistory.SetCurrentValue(adjustedValue);
            }
        }

        if (priceUnitId.HasValue)
        {
            var conversionUnit = await priceUnitRepository
                .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)))
                .Include(pu => pu.Price)
                .FirstOrDefaultAsync(cancellationToken);

            if (conversionUnit?.Price?.PriceHistory != null && conversionUnit.Price.PriceHistory.CurrentValue != 0)
            {
                var baseValueInRial = baseItem.PriceHistory.CurrentValue;
                var conversionUnitValueInRial = conversionUnit.Price.PriceHistory.CurrentValue;

                var convertedValue = baseValueInRial / conversionUnitValueInRial;

                return new GetPriceResponse(
                    Id: baseItem.Id.Value,
                    Title: baseItem.Title,
                    Value: convertedValue.ToString("G29"),
                    Unit: conversionUnit.Title,
                    Change: baseItem.PriceHistory.DailyChangeRate,
                    LastUpdate: baseItem.PriceHistory.LastUpdate,
                    HasIcon: webHostEnvironment.PriceUnitIconExists(baseItem.Id.Value),
                    Type: baseItem.MarketType,
                    UnitType: baseItem.UnitType
                );
            }
        }

        return new GetPriceResponse(
            Id: baseItem.Id.Value,
            Title: baseItem.Title,
            Value: baseItem.PriceHistory.CurrentValue.ToString("G29"),
            Unit: baseItem.PriceHistory.Unit,
            Change: baseItem.PriceHistory.DailyChangeRate,
            LastUpdate: baseItem.PriceHistory.LastUpdate,
            HasIcon: webHostEnvironment.PriceUnitIconExists(baseItem.Id.Value),
            Type: baseItem.MarketType,
            UnitType: baseItem.UnitType
        );
    }

    public async Task<GetPriceResponse?> GetAsync(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        var item = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)))
            .Include(pu => pu.Price)
            .FirstOrDefaultAsync(cancellationToken);

        return item?.Price is null ? null : mapper.Map<GetPriceResponse>(item.Price);
    }

    public async Task<List<GetPriceSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.Get(new PricesWithoutSpecification()).ToListAsync(cancellationToken);

        var priceSettings = items
            .GroupBy(p => p.MarketType)
            .Select(group => new GetPriceSettingResponse(
                group.Key,
                group.Select(price => new PriceSettingDto(
                    price.Id.Value,
                    price.Title,
                    price.IsActive
                )).ToList()
            ))
            .ToList();

        return priceSettings;
    }

    public async Task SetStatusAsync(Guid id, UpdatePriceStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesByIdSpecification(new PriceId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetStatus(request.IsActive);
        
        await repository.UpdateAsync(item, cancellationToken);
    }

    #endregion
}