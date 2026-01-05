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

        // Product total length must be 8 (prefix + numeric tail)
        var next = ComputeNext(prefixed: true, prefix, baseline, totalDigits: 8, start: 1);

        while (await productRepository.ExistsAsync(
                   new ProductsByBarcodeSpecification(next), cancellationToken)
               || await reservationRepository.ExistsAsync(
                   new BarcodeReservationsByBarcodeSpecification(next), cancellationToken))
        {
            next = ComputeNext(prefixed: true, prefix, next, totalDigits: 8, start: 1);
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

        var next = ComputeNext(prefixed: false, null, baseline, totalDigits: CoinDigits, start: CoinStart);

        while (await coinRepository.ExistsAsync(
                   new CoinInstancesByBarcodeSpecification(next), cancellationToken)
               || await reservationRepository.ExistsAsync(
                   new BarcodeReservationsByBarcodeSpecification(next), cancellationToken))
        {
            next = ComputeNext(prefixed: false, null, next, totalDigits: CoinDigits, start: CoinStart);
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

    /// <summary>
    /// Computes the next barcode.
    /// - If prefixed=false (coins): returns a numeric string with totalDigits length.
    /// - If prefixed=true (products): returns prefix + numeric tail, where (prefix + tail).Length == totalDigits.
    /// </summary>
    private static string ComputeNext(
        bool prefixed,
        string? prefix,
        string? last,
        int totalDigits,
        int start)
    {
        if (!prefixed)
        {
            // Coins: whole barcode is numeric with fixed length
            if (string.IsNullOrWhiteSpace(last))
                return start.ToString($"D{totalDigits}");

            if (!int.TryParse(last, out var result) || result < start)
                result = start - 1;

            return (result + 1).ToString($"D{totalDigits}");
        }

        // Products: barcode total length is fixed, prefix is part of it
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentException("Prefix is required for prefixed barcode.", nameof(prefix));

        if (prefix.Length >= totalDigits)
            throw new InvalidOperationException($"Prefix length ({prefix.Length}) cannot be >= total digits ({totalDigits}).");

        var numericDigits = totalDigits - prefix.Length;

        if (string.IsNullOrWhiteSpace(last))
        {
            // first barcode for this prefix
            return prefix + start.ToString($"D{numericDigits}");
        }

        var normalizedLast = last.Trim();

        // last may already include prefix; if not, treat it as numeric tail
        var numericPart = normalizedLast.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? normalizedLast.Substring(prefix.Length)
            : normalizedLast;

        if (!int.TryParse(numericPart, out var n) || n < start)
            n = start - 1;

        return prefix + (n + 1).ToString($"D{numericDigits}");
    }

    #endregion
}