using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Prices;
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

        var localPrices = await repository.Get(new PricesDefaultSpecification()).ToListAsync(cancellationToken);

        var pricesToCreate = new List<Price>();
        var pricesToUpdate = new List<Price>();
        var downloadTasks = new List<Task<(Price price, byte[]? imageData, string? contentType)>>();

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
                        return (price, result.ByteArray, result.ContentType);
                    }, cancellationToken));
                }
            }
        }

        if (pricesToCreate.Any()) await repository.CreateRangeAsync(pricesToCreate, cancellationToken);
        if (pricesToUpdate.Any()) await repository.UpdateRangeAsync(pricesToUpdate, cancellationToken);

        await Task.WhenAll(downloadTasks);

        foreach (var taskResult in downloadTasks)
        {
            var downloaded = await taskResult;
            if (downloaded.imageData is not null)
            {
                await fileService.SaveLocalFileAsync(webHostEnvironment.GetPriceHistoryIconPath(
                        downloaded.price.Id.Value,
                        downloaded.contentType),
                    downloaded.imageData,
                    cancellationToken);
            }
        }
    }

    #endregion

    #region PriceService

    public async Task<List<GetPriceResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesDefaultSpecification()).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPriceResponse>>(item);
    }

    public async Task<List<GetPriceResponse>> GetAsync(MarketType marketType, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesByMarketTypeSpecification(marketType)).ToListAsync(cancellationToken);
        return mapper.Map<List<GetPriceResponse>>(item);
    }

    public async Task<GetPriceResponse?> GetAsync(UnitType unitType, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesByUnitTypeSpecification(unitType)).FirstOrDefaultAsync(cancellationToken);
        return item is null ? null : mapper.Map<GetPriceResponse?>(item);
    }

    #endregion
}