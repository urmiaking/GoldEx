using GoldEx.Shared.DTOs.Vitrine;

namespace GoldEx.Shared.Services.Abstractions;

public interface IVitrineService
{
    Task<VitrineStoreInfoDto?> GetStoreInfoAsync(string storeSlug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VitrineProductSummaryDto>> GetVitrineProductsAsync(
        string storeSlug,
        Guid? categoryId = null,
        bool? onlyFeatured = null,
        CancellationToken cancellationToken = default);

    Task<VitrineProductDetailDto?> GetProductDetailAsync(
        string storeSlug,
        string barcode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VitrineCategoryDto>> GetCategoriesAsync(
        string storeSlug,
        CancellationToken cancellationToken = default);

    Task UpdateProductVitrineAsync(
        Guid productId,
        UpdateProductVitrineRequest request,
        CancellationToken cancellationToken = default);
}
