using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IBarcodeGeneratorService
{
    /// <summary>
    /// Builds the full prefix from product type and category (e.g., G + AA = GAA).
    /// </summary>
    Task<string> BuildFullPrefixAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates the next unique barcode for the given product type and optional category.
    /// It considers both existing Products and active/committed Reservations to preserve ordering.
    /// </summary>
    Task<string> GenerateNextAsync(ProductType productType, ProductCategoryId? categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates uniqueness of a barcode across Products and Reservations.
    /// Throws if not unique.
    /// </summary>
    Task ValidateUniquenessAsync(string barcode, CancellationToken cancellationToken = default);
}