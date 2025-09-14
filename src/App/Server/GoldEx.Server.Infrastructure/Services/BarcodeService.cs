using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Services;

[ScopedService]
internal class BarcodeService(IProductRepository productRepository, IProductCategoryRepository categoryRepository) : IBarcodeService
{
    public async Task<string> GenerateNextProductBarcodeAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default)
    {
        var typePrefix = GetTypePrefix(productType);
        var categoryPrefix = "00";

        if (categoryId.HasValue)
        {
            var category = await categoryRepository.Get(new ProductCategoriesByIdSpecification(categoryId.Value))
                .FirstOrDefaultAsync(cancellationToken);

            if (category?.PrefixCode is not null)
            {
                categoryPrefix = category.PrefixCode;
            }
        }

        var fullPrefix = $"{typePrefix}{categoryPrefix}";

        // --- قدم دوم: پیدا کردن آخرین بارکد با این پیشوند ---
        var lastBarcode = await productRepository.GetLastBarcodeWithPrefixAsync(fullPrefix, cancellationToken);

        // --- قدم سوم: استخراج شماره سریال، افزایش و فرمت‌بندی ---
        var nextSequence = 1;
        if (!string.IsNullOrEmpty(lastBarcode))
        {
            var lastSequenceStr = lastBarcode.Substring(3); // از کاراکتر سوم به بعد
            if (int.TryParse(lastSequenceStr, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        var sequenceNumber = nextSequence.ToString("D5"); // فرمت ۵ رقمی

        // --- قدم چهارم: ترکیب نهایی ---
        return $"{fullPrefix}{sequenceNumber}";
    }

    private string GetTypePrefix(ProductType productType)
    {
        return productType switch
        {
            ProductType.Jewelry => "J",
            ProductType.Gold => "G",
            ProductType.MoltenGold => "M",
            _ => "U" // Unknown
        };
    }

    public async Task<string> GenerateProductBarcodeAsync(Product product,
        CancellationToken cancellationToken = default)
    {
        var identicalProducts = await productRepository
            .Get(new ProductsByNameSpecification(product.Name))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // No products with this name, generate completely new barcode
        if (!identicalProducts.Any())
        {
            while (true)
            {
                var barcode = StringExtensions.GenerateRandomBarcode();

                if (!await productRepository.ExistsAsync(new ProductsByBarcodeSpecification(barcode), cancellationToken))
                    return barcode;
            }
        }

        // Try to find identical product based on attributes
        var identicalProduct = identicalProducts.FirstOrDefault(x =>
            x.WageType == product.WageType &&
            x.Wage == product.Wage &&
            x.Fineness == product.Fineness &&
            x.ProductCategoryId == product.ProductCategoryId &&
            x.Weight == product.Weight);

        if (identicalProduct is not null)
            return identicalProduct.Barcode;

        // Calculate next numeric barcode
        var maxNumericPart = identicalProducts
            .Select(p => p.Barcode)
            .Where(b => !string.IsNullOrWhiteSpace(b) && b.Length >= 6 && long.TryParse(b[^8..], out _))
            .Select(b => long.Parse(b[^8..]))
            .DefaultIfEmpty(0)
            .Max();

        var nextBarcode = (maxNumericPart + 1).ToString(); //supports 6-8 digits

        while (true)
        {
            nextBarcode = nextBarcode.PadLeft(8, '0');

            if (!await productRepository.ExistsAsync(new ProductsByBarcodeSpecification(nextBarcode), cancellationToken))
                break;

            nextBarcode = (long.Parse(nextBarcode) + 1).ToString();
        }

        return nextBarcode;
    }

    public async Task ValidateBarcodeUniquenessAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (await productRepository.ExistsAsync(new ProductsByBarcodeSpecification(barcode), cancellationToken))
            throw new ValidationException(new List<ValidationFailure>
            {
                new("barcode", $"کد {barcode} از قبل موجود می باشد")
            });
    }
}
