using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Coins;
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
    ICoinService coinService,
    IFinancialAccountRepository financialAccountRepository,
    ILedgerAccountRepository ledgerAccountRepository,
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
                        incomingPrice.Unit));

                pricesToCreate.Add(price);

                if (!string.IsNullOrEmpty(incomingPrice.IconUrl))
                {
                    downloadTasks.Add(Task.Run(async () =>
                    {
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

            var coins = await coinService.GetListAsync(null, cancellationToken);

            if (!coins.Any())
            {
                var coinPrices = pricesToCreate.Where(x => x.MarketType is MarketType.Coin).ToList();

                foreach (var coinPrice in coinPrices)
                    await coinService.CreateAsync(new CoinRequestDto(null, coinPrice.Title, coinPrice.Id.Value), cancellationToken);
            }

            var goldAccountExists = await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Gold), cancellationToken);

            if (!goldAccountExists)
            {
                var goldPriceUnit = await priceUnitRepository.Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Gold price unit is not initialized");

                var inventoryLedgerAccount = await ledgerAccountRepository.Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Molten Gold Inventory ledger account is not initialized");

                var goldAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Gold, goldPriceUnit.Id, inventoryLedgerAccount.Id);
                await financialAccountRepository.CreateAsync(goldAccount, cancellationToken);
            }

            var cashAccountExists = await financialAccountRepository.ExistsAsync(new FinancialAccountsByTypeSpecification(FinancialAccountType.Cash), cancellationToken);

            if (!cashAccountExists)
            {
                var irrPriceUnit = await priceUnitRepository.Get(new PriceUnitsByTitleSpecification(UnitType.IRR.GetDisplayName()))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("IRR price unit is not initialized");

                var internalCashLedgerAccount = await ledgerAccountRepository.Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.InternalCashAccounts))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Internal Cash Accounts ledger account is not initialized");

                var cashAccount = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Cash, irrPriceUnit.Id, internalCashLedgerAccount.Id,
                    cashAccount: CashAccount.Create(null, CashAccountType.Internal));

                await financialAccountRepository.CreateAsync(cashAccount, cancellationToken);
            }
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
            price.PriceHistory!.SetCurrentValue(ConvertFromRial(price.PriceHistory.CurrentValue, defaultPriceUnit?.UnitType));
            price.PriceHistory!.SetUnit(defaultPriceUnit?.Title ?? price.PriceHistory.Unit);
            price.PriceHistory!.SetDailyChangeRate(ConvertFormattedPrice(
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

        if (primaryPriceUnit == secondaryPriceUnit)
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
            ConvertFromRial(exchangeRate ?? 0, defaultPriceUnit?.UnitType)
        );
    }

    public async Task<List<GetPriceSettingResponse>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PricesWithoutSpecification())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var priceSettings = items
            .GroupBy(p => p.MarketType)
            .Select(group => new GetPriceSettingResponse(
                group.Key,
                group.Select(price => new PriceSettingDto(
                    price.Id.Value,
                    price.Title,
                    price.IsActive,
                    price.IsPinned
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

    #endregion

    private static decimal ConvertFromRial(decimal value, UnitType? defaultUnitType)
    {
        return defaultUnitType switch
        {
            UnitType.Toman => value / 10,
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
        if (defaultUnitType == UnitType.Toman)
            value /= 10;

        // Format back with a thousand separators (using current culture)
        var formattedValue = $"{value:N0}";

        return $"{formattedValue} {percentPart}".TrimEnd();
    }
}