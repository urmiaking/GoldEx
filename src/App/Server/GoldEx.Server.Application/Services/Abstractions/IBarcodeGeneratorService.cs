using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IBarcodeGeneratorService
{
    Task<string> BuildProductPrefixAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default);
    Task<string> GenerateNextAsync(BarcodeType barcodeType, ProductType? productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default);
    Task ValidateUniquenessAsync(BarcodeType barcodeType, string barcode, CancellationToken cancellationToken = default);
}