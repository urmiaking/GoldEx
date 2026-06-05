using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Services.PriceProviders;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceProviderMappings;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceService(
    IPriceRepository repository,
    IPriceUnitRepository priceUnitRepository,
    ISettingRepository settingRepository,
    IPriceProviderMappingRepository providerMappingRepository,
    IMapper mapper,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    PriceProviderRegistry providerRegistry) : IServerPriceService,
    IPriceService 
{
    #region ServerPriceService

    public async Task AddOrUpdateAsync(List<PriceResponse> incomingPriceList, CancellationToken cancellationToken = default)
    {
        if (!incomingPriceList.Any())
            return;

        var localPricesLookup = (await repository.Get(new PricesWithoutSpecification()).ToListAsync(cancellationToken))
            .ToDictionary(p => p.Title);

        var pricesToUpdate = GetPricesToUpdate(incomingPriceList, localPricesLookup);

        if (pricesToUpdate.Any())
            await repository.UpdateRangeAsync(pricesToUpdate, cancellationToken);
    }

    #region Helper methods

    private List<Price> GetPricesToUpdate(
            List<PriceResponse> incomingPriceList,
            Dictionary<string, Price> localPricesLookup)
    {
        var pricesToUpdate = new List<Price>();

        foreach (var incomingPrice in incomingPriceList)
        {
            if (localPricesLookup.TryGetValue(incomingPrice.Title, out var existingPrice))
            {
                if (UpdateExistingPriceHistory(existingPrice, incomingPrice))
                {
                    pricesToUpdate.Add(existingPrice);
                }
            }
        }

        return pricesToUpdate;
    }

    /// <summary>
    /// Updates an existing price's history. Returns true if an update was made, false otherwise.
    /// </summary>
    private bool UpdateExistingPriceHistory(Price existingPrice, PriceResponse incomingPrice)
    {
        if (existingPrice.PriceHistory is null)
        {
            existingPrice.CreatePriceHistory(PriceHistory.Create(incomingPrice.CurrentValue, incomingPrice.LastUpdate, incomingPrice.Change, incomingPrice.Unit));
            return true; // Price was updated
        }

        // Continue if data is identical
        if (existingPrice.PriceHistory.CurrentValue == incomingPrice.CurrentValue &&
            existingPrice.PriceHistory.LastUpdate == incomingPrice.LastUpdate)
        {
            return false; // No change
        }

        existingPrice.SetPriceHistory(incomingPrice.CurrentValue, incomingPrice.LastUpdate, incomingPrice.Change, incomingPrice.Unit);
        return true; // Price was updated
    }

    /// <summary>
    /// Creates and starts a task to download a price icon.
    /// </summary>
    private Task<(Price price, byte[]? imageData, string? imageFormat)> CreateIconDownloadTask(
        Price price, string iconUrl, CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            var result = await ImageConverter.ToByteArrayAsync(iconUrl);
            return (price, result.ByteArray, result.ImageFormat);
        }, cancellationToken);
    }

    // --- 2. Image Download Processing Helper ---

    /// <summary>
    /// Awaits all pending image downloads and saves the results to the file system
    /// for both the Price and its related PriceUnits.
    /// </summary>
    private async Task ProcessImageDownloadsAsync(
        List<Task<(Price price, byte[]? imageData, string? imageFormat)>> downloadTasks,
        CancellationToken cancellationToken)
    {
        if (!downloadTasks.Any()) return;

        await Task.WhenAll(downloadTasks);

        foreach (var task in downloadTasks)
        {
            var (price, imageData, imageFormat) = await task; // Get completed task result

            if (imageData is not null && imageFormat is not null)
            {
                // Save icon for the Price (History)
                await fileService.SaveLocalFileAsync(
                    webHostEnvironment.GetPriceHistoryIconPath(price.Id.Value, imageFormat),
                    imageData,
                    cancellationToken);

                // Save icon for all related PriceUnits
                var relatedPriceUnits = await priceUnitRepository.Get(
                        new PriceUnitsByPriceIdSpecification(price.Id))
                    .ToListAsync(cancellationToken);

                foreach (var priceUnit in relatedPriceUnits)
                {
                    await fileService.SaveLocalFileAsync(
                        webHostEnvironment.GetPriceUnitIconPath(priceUnit.Id.Value, imageFormat),
                        imageData,
                        cancellationToken);
                }
            }
        }
    }

    #endregion

    #endregion

    #region PriceService

    public async Task<List<GetPriceResponse>> GetListAsync(bool? isPinned = null, CancellationToken cancellationToken = default)
    {
        var list = await repository
            .Get(new PricesDefaultSpecification(isPinned))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var defaultPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        foreach (var price in list.Where(price => price.MarketType is not MarketType.Ounce))
        {
            price.PriceHistory?.SetCurrentValue(ConvertFromRial(price.PriceHistory.CurrentValue, defaultPriceUnit?.UnitType));
            price.PriceHistory?.SetUnit(defaultPriceUnit?.Title ?? price.PriceHistory.Unit);
            price.PriceHistory?.SetDailyChangeRate(ConvertFormattedPrice(
                price.PriceHistory.DailyChangeRate,
                defaultPriceUnit?.UnitType
            ));
        }

        return mapper.Map<List<GetPriceResponse>>(list);
    }

    public async Task<List<GetPriceTitleResponse>> GetTitlesAsync(MarketType[] marketTypes,
        CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PricesByMarketTypesSpecification(marketTypes))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceTitleResponse>>(items);
    }

    public async Task<List<GetPriceResponse>> GetListAsync(MarketType marketType, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PricesByMarketTypeSpecification(marketType))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceResponse>>(item);
    }

    public async Task<GetPriceResponse?> GetAsync(GoldUnitType unitType, Guid? priceUnitId, bool applySafetyMargin,
        CancellationToken cancellationToken = default)
    {
        var unit = unitType switch
        {
            GoldUnitType.Gram => UnitType.Gold18K,
            GoldUnitType.Mesghal => UnitType.Mesghal,
            _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
        };

        var baseItem = await repository
            .Get(new PricesByUnitTypeSpecification(unit))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (baseItem?.PriceHistory is null)
            return null;

        var setting = await settingRepository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken);

        var defaultUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (setting is not null && setting.GoldSafetyMarginPercent != 0)
        {
            if (baseItem.PriceUnit?.UnitType is UnitType.Gold18K or UnitType.Mesghal)
            {
                var marginFactor = setting.GoldSafetyMarginPercent / 100m;
                var adjustedValue = applySafetyMargin
                    ? baseItem.PriceHistory.CurrentValue * (1 + marginFactor)
                    : baseItem.PriceHistory.CurrentValue * (1 - marginFactor);

                baseItem.PriceHistory.SetCurrentValue(adjustedValue);
            }
        }

        if (priceUnitId.HasValue)
        {
            var conversionUnit = await priceUnitRepository
                .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)))
                .AsNoTracking()
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
                    UnitType: baseItem.PriceUnit?.UnitType
                );
            }
        }

        return new GetPriceResponse(
            Id: baseItem.Id.Value,
            Title: baseItem.Title,
            Value: ConvertFromRial(baseItem.PriceHistory.CurrentValue, defaultUnit?.UnitType).ToString("G29"),
            Unit: baseItem.PriceHistory.Unit,
            Change: baseItem.PriceHistory.DailyChangeRate,
            LastUpdate: baseItem.PriceHistory.LastUpdate,
            HasIcon: webHostEnvironment.PriceUnitIconExists(baseItem.Id.Value),
            Type: baseItem.MarketType,
            UnitType: baseItem.PriceUnit?.UnitType
        );
    }


    public async Task<GetPriceResponse?> GetAsync(Guid priceUnitId, CancellationToken cancellationToken = default)
    {
        var item = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId)))
            .AsNoTracking()
            .Include(pu => pu.Price)
            .FirstOrDefaultAsync(cancellationToken);

        return item?.Price is null ? null : mapper.Map<GetPriceResponse>(item.Price);
    }

    public async Task<GetPriceResponse?> GetAsync(PriceCatalog priceCatalog, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PricesByPriceCatalogSpecification(priceCatalog))
            .AsNoTracking()
            .Include(x => x.PriceUnit)
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return null;

        var defaultUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (item.MarketType is not MarketType.Ounce && item.PriceHistory is not null)
        {
            item.PriceHistory.SetCurrentValue(ConvertFromRial(item.PriceHistory.CurrentValue, defaultUnit?.UnitType));
            item.PriceHistory.SetUnit(defaultUnit?.Title ?? item.PriceHistory.Unit);
            item.PriceHistory.SetDailyChangeRate(ConvertFormattedPrice(item.PriceHistory.DailyChangeRate, defaultUnit?.UnitType));
        }

        return mapper.Map<GetPriceResponse>(item);
    }

    public async Task<GetExchangeRateResponse> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId,
        CancellationToken cancellationToken = default)
    {
        var primaryPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(primaryPriceUnitId)))
            .AsNoTracking()
            .Include(pu => pu.Price)
            .FirstOrDefaultAsync(cancellationToken);

        var secondaryPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(secondaryPriceUnitId)))
            .AsNoTracking()
            .Include(pu => pu.Price)
            .FirstOrDefaultAsync(cancellationToken);

        var defaultPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (primaryPriceUnit?.Price is null && primaryPriceUnit?.UnitType == defaultPriceUnit?.UnitType)
        {
            if (secondaryPriceUnit?.Price?.PriceHistory is not null && secondaryPriceUnit.Price?.PriceHistory?.CurrentValue != 0)
            {
                var rate = 1 / ConvertFromRial(secondaryPriceUnit.Price?.PriceHistory?.CurrentValue ?? 1, defaultPriceUnit?.UnitType);
                return new GetExchangeRateResponse(rate);
            }
        }

        if (primaryPriceUnit?.Price is null)
            return new GetExchangeRateResponse(0);

        if (secondaryPriceUnit?.Price is null)
            return new GetExchangeRateResponse(ConvertFromRial(primaryPriceUnit.Price?.PriceHistory?.CurrentValue ?? 0, defaultPriceUnit?.UnitType));

        if (Equals(primaryPriceUnit, secondaryPriceUnit))
            throw new InvalidOperationException("Secondary unit cannot be as same as Primary unit");

        if ((primaryPriceUnit.Price?.PriceHistory is null && primaryPriceUnit.Price?.PriceHistory?.CurrentValue == 0) ||
            (secondaryPriceUnit.Price?.PriceHistory is null && secondaryPriceUnit.Price?.PriceHistory?.CurrentValue == 0))
            return new GetExchangeRateResponse(0);

        var primaryValue = primaryPriceUnit.Price?.PriceHistory?.CurrentValue;
        var secondaryValue = secondaryPriceUnit.Price?.PriceHistory?.CurrentValue;

        if (primaryValue is null or 0 || secondaryValue is null or 0)
            return new GetExchangeRateResponse(0);

        var exchangeRate = primaryValue / secondaryValue;

        return new GetExchangeRateResponse(
            exchangeRate
        );
    }

    public async Task<List<GetPriceSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PricesWithoutSpecification())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Collect price ids
        var priceIds = items.Select(p => p.Id).ToList();

        // Load mappings for these prices
        var mappings = await providerMappingRepository
            .Get(new PriceProviderMappingsByPriceIdsSpecification(priceIds))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapByPriceId = mappings.ToDictionary(m => m.PriceId, m => m);

        var priceSettings = items
            .GroupBy(p => p.MarketType)
            .Select(group => new GetPriceSettingResponse(
                group.Key,
                group.Select(price =>
                {
                    mapByPriceId.TryGetValue(price.Id, out var mapping);
                    return new PriceSettingDto(
                        price.Id.Value,
                        price.Title,
                        price.MarketType,
                        price.IsActive,
                        price.IsPinned,
                        mapping?.ProviderType,
                        mapping?.ProviderSymbol,
                        mapping?.IsEnabled
                    );
                }).ToList()
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

    public async Task SetPinnedAsync(Guid id, bool isPinned, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new PricesByIdSpecification(new PriceId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var pinnedItemsCount = await repository.CountAsync(new PricesByPinStatusSpecification(), cancellationToken);

        if (isPinned && !item.IsPinned && pinnedItemsCount >= 12)
            throw new ValidationException(new List<ValidationFailure>
            {
                new("IsPinned", "تعداد ارزهای سنجاق شده نمی تواند بیشتر از 12 عدد باشد")
            });

        item.SetPinned(isPinned);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task<GetPriceProviderCatalogResponse> GetProviderCatalogAsync(PriceProviderType providerType, MarketType? marketType, CancellationToken cancellationToken = default)
    {
        if (providerType == PriceProviderType.Manual)
            return new GetPriceProviderCatalogResponse([]);

        var provider = providerRegistry.Resolve(providerType);
        if (provider is null)
            return new GetPriceProviderCatalogResponse([]);

        // Fetch all (empty symbols list means "provider returns its full known list" in GenericBatch)
        var all = await provider.FetchAsync(Array.Empty<string>(), cancellationToken);

        var filtered = all
            .Where(p => marketType is null || p.MarketType == marketType)
            .Select(p => new PriceProviderSymbolDto(p.Title, p.Title, p.MarketType))
            .GroupBy(x => x.Symbol)
            .Select(g => g.First())
            .OrderBy(x => x.Title)
            .ToList();

        return new GetPriceProviderCatalogResponse(filtered);
    }

    // Validation
    public async Task<ValidatePriceProviderResponse> ValidateProviderSymbolAsync(Guid priceId, PriceProviderType providerType, string providerSymbol, CancellationToken cancellationToken = default)
    {
        var price = await repository
            .Get(new PricesByIdSpecification(new PriceId(priceId)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (price is null)
            return new ValidatePriceProviderResponse(false, false, "قیمت یافت نشد.", null, null);

        if (providerType == PriceProviderType.Manual)
            return new ValidatePriceProviderResponse(true, true, "حالت دستی انتخاب شده است.", price.MarketType, null);

        var provider = providerRegistry.Resolve(providerType);
        if (provider is null)
            return new ValidatePriceProviderResponse(false, false, "سرویس‌دهنده معتبر نیست.", null, null);

        if (string.IsNullOrWhiteSpace(providerSymbol))
            return new ValidatePriceProviderResponse(false, false, "نماد وارد نشده است.", null, null);

        var list = await provider.FetchAsync(new[] { providerSymbol }, cancellationToken);
        var sample = list.FirstOrDefault(x => x.Title == providerSymbol);
        if (sample is null)
            return new ValidatePriceProviderResponse(false, false, "نماد در سرویس‌دهنده یافت نشد.", null, null);

        var marketMatch = sample.MarketType == price.MarketType;
        var msg = marketMatch
            ? "سرویس دهنده قیمت و نماد معتبر است."
            : "هشدار: بازار نماد با بازار نرخ متفاوت است.";

        return new ValidatePriceProviderResponse(true, marketMatch, msg, sample.MarketType, sample);
    }

    public async Task UpdateAsync(Guid id, UpdatePriceSettingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PricesByIdSpecification(new PriceId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            throw new NotFoundException();

        // Replace icon if provided
        if (request.IconContent.Length > 0)
        {
            await fileService.ReplaceLocalFileAsync(
                webHostEnvironment.GetPriceHistoryIconPath(id, null),
                request.IconContent,
                cancellationToken);
        }

        // Upsert provider mapping logic
        var mapping = await providerMappingRepository
            .Get(new PriceProviderMappingsByPriceIdSpecification(new PriceId(id)))
            .FirstOrDefaultAsync(cancellationToken);

        if (!request.IsProviderEnabled || request.ProviderType == PriceProviderType.Manual)
        {
            // Disable mapping if exists
            if (mapping is not null && mapping.IsEnabled)
            {
                mapping.SetEnabled(false);
                await providerMappingRepository.UpdateAsync(mapping, cancellationToken);
            }
            return;
        }

        if (mapping is null)
        {
            var newMapping = PriceProviderMapping.Create(new PriceId(id),
                request.ProviderType,
                request.ProviderSymbol ?? string.Empty);
            await providerMappingRepository.CreateAsync(newMapping, cancellationToken);
        }
        else
        {
            mapping.SetProvider(request.ProviderType, request.ProviderSymbol ?? string.Empty);
            mapping.SetEnabled(true);
            await providerMappingRepository.UpdateAsync(mapping, cancellationToken);
        }
    }


    #endregion

    private static decimal ConvertFromRial(decimal value, UnitType? defaultUnitType)
    {
        return defaultUnitType switch
        {
            UnitType.TMN => value / 10,
            _ => value // Rial or any other non-adjusted unit
        };
    }

    private static string ConvertFormattedPrice(string formattedPrice, UnitType? defaultUnitType)
    {
        if (string.IsNullOrWhiteSpace(formattedPrice))
            return formattedPrice;

        // Split the numeric and percentage parts (e.g. "4٬542٬268 (0.94%)")
        var parts = formattedPrice.Split('(', StringSplitOptions.TrimEntries);
        var numberPart = parts[0].Trim();
        var percentPart = parts.Length > 1 ? "(" + parts[1] : string.Empty;

        // Remove Persian/Arabic thousand separators and commas
        var cleaned = numberPart
            .Replace("٬", string.Empty) // Arabic comma
            .Replace(",", string.Empty) // Normal comma
            .Trim();

        if (!decimal.TryParse(cleaned, out var value))
            return formattedPrice; // if parse fails, return original

        // Convert if default unit is Toman
        if (defaultUnitType == UnitType.TMN)
            value /= 10;

        // Format back with a thousand separators (using current culture)
        var formattedValue = $"{value:N0}";

        return $"{formattedValue} {percentPart}".TrimEnd();
    }
}