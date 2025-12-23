using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.CoinInstances;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.CoinInstances;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CoinInstanceService(ICoinInstanceRepository repository,
    IBarcodeGeneratorService barcodeGenerator,
    IServerBarcodeReservationService barcodeReservationService,
    IMapper mapper,
    CoinInstanceDtoValidator validator) : IServerCoinInstanceService, ICoinInstanceService
{
    #region ServerSide

    public async Task SyncCoinItemsAsync(Invoice invoice, IEnumerable<InvoiceCoinItemDto> requestedItems,
        CancellationToken cancellationToken = default)
    {
        var existingItems = invoice.CoinItems.ToList();
        var requestedDtos = requestedItems.ToList();

        // 1. Remove deleted items
        var itemsToDelete = existingItems
            .Where(e => requestedDtos.All(r => r.Id != e.Id.Value))
            .ToList();

        foreach (var item in itemsToDelete)
        {
            invoice.RemoveCoinItem(item);
        }

        // 2. Process requested items
        foreach (var dto in requestedDtos)
        {
            CoinInstance coinInstance;

            // --- Resolve or Create CoinInstance ---
            if (dto.CoinInstance.Id.HasValue)
            {
                coinInstance = await repository
                    .Get(new CoinInstancesByIdSpecification(new CoinInstanceId(dto.CoinInstance.Id.Value)))
                    .FirstOrDefaultAsync(cancellationToken)
                               ?? throw new NotFoundException("Coin instance not found");

                // Update existing coin instance details
                coinInstance.SetMintYear(dto.CoinInstance.MintYear);
                coinInstance.SetWeight(dto.CoinInstance.Weight);
                coinInstance.SetFineness(dto.CoinInstance.Fineness);
                coinInstance.SetMintType(dto.CoinInstance.MintType);
                coinInstance.SetPackage(dto.CoinInstance.PackageType,
                    dto.CoinInstance.CoinPackage != null
                        ? CoinInstancePackage.Create(dto.CoinInstance.CoinPackage.VacuumedWeight,
                            dto.CoinInstance.CoinPackage.CardColor,
                            dto.CoinInstance.CoinPackage.StandardCode,
                            dto.CoinInstance.CoinPackage.IssuerId.HasValue
                                ? new CustomerId(dto.CoinInstance.CoinPackage.IssuerId.Value)
                                : null)
                        : null);

                await repository.UpdateAsync(coinInstance, cancellationToken);
            }
            else
            {
                // Package
                CoinInstancePackage? package = null;
                if (dto.CoinInstance is { PackageType: CoinPackageType.VacuumSealed, CoinPackage: not null })
                {
                    package = CoinInstancePackage.Create(
                        dto.CoinInstance.CoinPackage.VacuumedWeight,
                        dto.CoinInstance.CoinPackage.CardColor,
                        dto.CoinInstance.CoinPackage.StandardCode,
                        dto.CoinInstance.CoinPackage.IssuerId.HasValue
                            ? new CustomerId(dto.CoinInstance.CoinPackage.IssuerId.Value)
                            : null);
                }

                // Barcode
                var barcode = dto.CoinInstance.Barcode;
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    barcode = await barcodeGenerator.GenerateNextAsync(BarcodeType.Coin, null, null, cancellationToken);
                }
                else
                {
                    await barcodeReservationService.CommitAsync(
                        BarcodeType.Coin,
                        barcode,
                        null,
                        cancellationToken);
                }

                coinInstance = dto.CoinInstance.PackageType == CoinPackageType.Open
                    ? CoinInstance.CreateOpen(
                        barcode,
                        dto.CoinInstance.MintYear,
                        dto.CoinInstance.Weight,
                        dto.CoinInstance.Fineness,
                        new CoinId(dto.CoinInstance.CoinId),
                        dto.CoinInstance.MintType)
                    : CoinInstance.CreateVacuumed(
                        barcode,
                        dto.CoinInstance.MintYear,
                        dto.CoinInstance.Weight,
                        dto.CoinInstance.Fineness,
                        new CoinId(dto.CoinInstance.CoinId),
                        dto.CoinInstance.MintType,
                        package!);

                await repository.CreateAsync(coinInstance, cancellationToken);
            }

            // --- Add / Update Invoice Item ---
            if (dto.Id.HasValue)
            {
                var existingItem = existingItems.Single(x => x.Id.Value == dto.Id.Value);

                existingItem.Update(
                    coinInstance.Id,
                    dto.UnitPrice,
                    dto.Quantity,
                    dto.ProfitPercent);
            }
            else
            {
                invoice.AddCoinItem(
                    null,
                    coinInstance.Id,
                    dto.UnitPrice,
                    dto.Quantity,
                    dto.ProfitPercent,
                    dto.IsInstant);
            }
        }
    }

    public async Task<CoinInstance> CreateCoinAsync(CoinInstanceRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CoinInstance coinInstance;
        var barcode = request.Barcode;

        if (string.IsNullOrEmpty(barcode))
        {
            barcode = await barcodeGenerator.GenerateNextAsync(BarcodeType.Coin, null, null, cancellationToken);
        }
        else
        {
            await barcodeReservationService.CommitAsync(BarcodeType.Coin, barcode, null, cancellationToken);
            await barcodeGenerator.ValidateUniquenessAsync(BarcodeType.Coin, barcode, cancellationToken);
        }

        switch (request.PackageType)
        {
            case CoinPackageType.VacuumSealed:
                if (request.CoinPackage is null)
                    throw new ValidationException("Coin package details must be provided for vacuum-sealed coins.");

                coinInstance = CoinInstance.CreateVacuumed(barcode, request.MintYear, request.Weight, request.Fineness,
                    new CoinId(request.CoinId), request.MintType, CoinInstancePackage.Create(
                        request.CoinPackage.VacuumedWeight,
                        request.CoinPackage.CardColor,
                        request.CoinPackage.StandardCode,
                        request.CoinPackage.IssuerId.HasValue
                            ? new CustomerId(request.CoinPackage.IssuerId.Value)
                            : null));
                break;
            case CoinPackageType.Open:
                coinInstance = CoinInstance.CreateOpen(barcode,
                    request.MintYear,
                    request.Weight,
                    request.Fineness,
                    new CoinId(request.CoinId),
                    request.MintType);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await repository.CreateAsync(coinInstance, cancellationToken);
        return coinInstance;
    }

    #endregion

    #region Shared

    public async Task<GetCoinInstanceResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var coinInstance = await repository
            .Get(new CoinInstancesByBarcodeSpecification(barcode))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return coinInstance is null ? null : mapper.Map<GetCoinInstanceResponse>(coinInstance);
    }

    #endregion
}