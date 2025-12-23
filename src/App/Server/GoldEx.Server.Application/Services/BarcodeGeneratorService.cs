using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.BarcodeReservations;
using GoldEx.Server.Infrastructure.Specifications.CoinInstances;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class BarcodeGeneratorService(
    IProductRepository productRepository,
    ICoinInstanceRepository coinRepository,
    IProductCategoryRepository categoryRepository,
    IBarcodeReservationRepository reservationRepository
) : IBarcodeGeneratorService
{
    private const int CoinDigits = 6;
    private const int CoinStart = 100001;

    #region Public API

    public async Task<string> GenerateNextAsync(
        BarcodeType barcodeType,
        ProductType? productType,
        ProductCategoryId? categoryId,
        CancellationToken cancellationToken = default)
    {
        return barcodeType switch
        {
            BarcodeType.Product => await GenerateNextProductAsync(productType!.Value, categoryId, cancellationToken),
            BarcodeType.Coin => await GenerateNextCoinAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(barcodeType))
        };
    }

    public async Task ValidateUniquenessAsync(
        BarcodeType barcodeType,
        string barcode,
        CancellationToken cancellationToken = default)
    {
        var exists = barcodeType switch
        {
            BarcodeType.Product =>
                await productRepository.ExistsAsync(
                    new ProductsByBarcodeSpecification(barcode), cancellationToken),

            BarcodeType.Coin =>
                await coinRepository.ExistsAsync(
                    new CoinInstancesByBarcodeSpecification(barcode), cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(barcodeType))
        };

        if (exists)
            throw new InvalidOperationException($"کد {barcode} از قبل موجود می‌باشد");
    }

    #endregion

    #region Product

    private async Task<string> GenerateNextProductAsync(
        ProductType productType,
        ProductCategoryId? categoryId,
        CancellationToken cancellationToken)
    {
        var prefix = await BuildProductPrefixAsync(productType, categoryId, cancellationToken);

        var lastProduct = await productRepository
            .GetLastBarcodeWithPrefixAsync(prefix, cancellationToken);

        var lastReserved = await reservationRepository
            .Get(new BarcodeLatestActiveOrCommittedByPrefixSpecification(prefix))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var baseline = MaxBarcode(lastProduct, lastReserved?.Barcode);

        var next = ComputeNext(prefixed: true, prefix, baseline, 8, 1);

        while (await productRepository.ExistsAsync(
                   new ProductsByBarcodeSpecification(next), cancellationToken)
               || await reservationRepository.ExistsAsync(
                   new BarcodeReservationsByBarcodeSpecification(next), cancellationToken))
        {
            next = ComputeNext(prefixed: true, prefix, next, 8, 1);
        }

        return next;
    }

    public async Task<string> BuildProductPrefixAsync(
        ProductType productType,
        ProductCategoryId? categoryId,
        CancellationToken cancellationToken)
    {
        var typePrefix = productType switch
        {
            ProductType.Jewelry => "1",
            ProductType.Gold => "2",
            ProductType.MoltenGold => "3",
            _ => "9"
        };

        var categoryPrefix = "00";

        if (categoryId.HasValue)
        {
            var category = await categoryRepository
                .Get(new ProductCategoriesByIdSpecification(categoryId.Value))
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(category?.PrefixCode))
                categoryPrefix = category.PrefixCode;
        }

        return $"{typePrefix}{categoryPrefix}";
    }

    #endregion

    #region Coin

    private async Task<string> GenerateNextCoinAsync(CancellationToken cancellationToken)
    {
        var lastCoinBarcode = await coinRepository.GetLastBarcodeAsync(cancellationToken);

        var lastReserved = await reservationRepository
            .Get(new BarcodeLatestActiveOrCommittedByTypeSpecification(BarcodeType.Coin))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var baseline = MaxBarcode(lastCoinBarcode, lastReserved?.Barcode);

        var next = ComputeNext(prefixed: false, null, baseline, CoinDigits, CoinStart);

        while (await coinRepository.ExistsAsync(
                   new CoinInstancesByBarcodeSpecification(next), cancellationToken)
               || await reservationRepository.ExistsAsync(
                   new BarcodeReservationsByBarcodeSpecification(next), cancellationToken))
        {
            next = ComputeNext(prefixed: false, null, next, CoinDigits, CoinStart);
        }

        return next;
    }

    #endregion

    #region Helpers

    private static string MaxBarcode(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a)) return b ?? string.Empty;
        if (string.IsNullOrWhiteSpace(b)) return a;
        return string.CompareOrdinal(a, b) >= 0 ? a : b;
    }

    private static string ComputeNext(
        bool prefixed,
        string? prefix,
        string? last,
        int digits,
        int start)
    {
        if (string.IsNullOrWhiteSpace(last))
            return start.ToString($"D{digits}");

        var numericPart = prefixed
            ? last.Substring(prefix!.Length)
            : last;

        if (!int.TryParse(numericPart, out var n) || n < start)
            n = start - 1;

        return (n + 1).ToString($"D{digits}");
    }

    #endregion
}
