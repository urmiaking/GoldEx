using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.BarcodeReservations;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class BarcodeGeneratorService(
    IProductRepository productRepository,
    IProductCategoryRepository categoryRepository,
    IBarcodeReservationRepository reservationRepository
) : IBarcodeGeneratorService
{
    public async Task<string> BuildFullPrefixAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default)
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

    public async Task<string> GenerateNextAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default)
    {
        var fullPrefix = await BuildFullPrefixAsync(productType, categoryId, cancellationToken);

        // last product with prefix
        var lastProductBarcode = await productRepository.GetLastBarcodeWithPrefixAsync(fullPrefix, cancellationToken);

        // last active or committed reservation with prefix
        var lastReservedOrCommitted = await reservationRepository
            .Get(new BarcodeLatestActiveOrCommittedByPrefixSpecification(fullPrefix))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var baseline = MaxBarcode(lastProductBarcode, lastReservedOrCommitted?.Barcode);

        var next = ComputeNextBarcode(fullPrefix, baseline);

        // ensure uniqueness across products and reservations
        while (await productRepository.ExistsAsync(new ProductsByBarcodeSpecification(next), cancellationToken)
            || await reservationRepository.ExistsAsync(new BarcodeReservationsByBarcodeSpecification(next), cancellationToken))
        {
            next = ComputeNextBarcode(fullPrefix, next);
        }

        return next;
    }

    public async Task ValidateUniquenessAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var existsInProducts = await productRepository.ExistsAsync(new ProductsByBarcodeSpecification(barcode), cancellationToken);

        if (existsInProducts)
            throw new InvalidOperationException($"کد {barcode} از قبل موجود می‌باشد");
    }

    private static string MaxBarcode(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a)) return b ?? "";
        if (string.IsNullOrWhiteSpace(b)) return a;
        return string.CompareOrdinal(a, b) >= 0 ? a : b;
    }

    private static string ComputeNextBarcode(string fullPrefix, string? lastOrNull)
    {
        if (string.IsNullOrEmpty(lastOrNull) || !lastOrNull.StartsWith(fullPrefix) || lastOrNull.Length < 4)
            return $"{fullPrefix}{1:D5}";

        var numericPart = lastOrNull.Substring(fullPrefix.Length);
        if (!int.TryParse(numericPart, out var n))
            n = 0;

        var next = (n + 1).ToString("D5");
        return $"{fullPrefix}{next}";
    }
}