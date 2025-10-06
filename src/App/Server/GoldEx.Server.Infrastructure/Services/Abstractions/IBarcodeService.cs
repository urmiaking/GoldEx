using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Services.Abstractions;

public interface IBarcodeService
{
    Task<string> GenerateNextProductBarcodeAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default);
    Task<string> GenerateProductBarcodeAsync(Product product, CancellationToken cancellationToken = default);
    Task ValidateBarcodeUniquenessAsync(string barcode, CancellationToken cancellationToken = default);
}