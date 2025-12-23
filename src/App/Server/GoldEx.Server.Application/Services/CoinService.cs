using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Coins;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CoinService(ICoinRepository repository,
    IPriceRepository priceRepository,
    IPriceUnitRepository priceUnitRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    IMapper mapper,
    ILogger<CoinService> logger,
    CoinRequestDtoValidator validator,
    DeleteCoinValidator deleteValidator) : ICoinService
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

    public async Task<GetExchangeRateResponse?> GetPriceAsync(
        Guid coinId,
        Guid? priceUnitId,
        CancellationToken cancellationToken = default)
    {
        var coin = await repository
                       .Get(new CoinsByIdSpecification(new CoinId(coinId)))
                       .AsNoTracking()
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException();

        if (coin.PriceId is null)
            return null;

        var baseItem = await priceRepository
            .Get(new PricesByIdSpecification(coin.PriceId.Value))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (baseItem?.PriceHistory == null)
            return null;

        var defaultUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (priceUnitId.HasValue)
        {
            var conversionUnit = await priceUnitRepository
                .Get(new PriceUnitsByIdSpecification(new PriceUnitId(priceUnitId.Value)))
                .AsNoTracking()
                .Include(pu => pu.Price)
                .FirstOrDefaultAsync(cancellationToken);

            if (conversionUnit?.Price?.PriceHistory != null &&
                conversionUnit.Price.PriceHistory.CurrentValue != 0)
            {
                var convertedValue = baseItem.PriceHistory.CurrentValue /
                                     conversionUnit.Price.PriceHistory.CurrentValue;

                return new GetExchangeRateResponse(convertedValue);
            }
        }

        var valueToReturn = ConvertFromRial(baseItem.PriceHistory.CurrentValue, defaultUnit?.UnitType);
        return new GetExchangeRateResponse(valueToReturn);
    }


    public async Task CreateAsync(CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var coinLedgerAccount = await ledgerAccountRepository
                                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CoinInventory))
                                            .FirstOrDefaultAsync(cancellationToken) ??
                                        throw new NotFoundException($"{nameof(SystemLedgerAccounts.CoinInventory)} account not found");

                var coin = Coin.Create(request.Title,
                    request.Weight,
                    request.Fineness,
                    request.StartMintYear,
                    request.EndMintYear,
                    coinLedgerAccount.Id,
                    request.PriceId.HasValue
                        ? new PriceId(request.PriceId.Value)
                        : null);

                await repository.CreateAsync(coin, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task UpdateAsync(Guid id, CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await validator.ValidateAndThrowAsync(request, cancellationToken);

                var coin = await repository
                    .Get(new CoinsByIdSpecification(new CoinId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                coin.SetTitle(request.Title);
                coin.SetPriceId(request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);
                coin.SetWeight(request.Weight);
                coin.SetFineness(request.Fineness);
                coin.SetMintYears(request.StartMintYear, request.EndMintYear);

                await repository.UpdateAsync(coin, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        coin.SetStatus(isActive);

        await repository.UpdateAsync(coin, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(id, cancellationToken);

        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        await repository.DeleteAsync(coin, cancellationToken);
    }

    private static decimal ConvertFromRial(decimal value, UnitType? defaultUnitType)
    {
        return defaultUnitType switch
        {
            UnitType.TMN => value / 10,
            _ => value // Rial or any other non-adjusted unit
        };
    }
}