using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Vitrine;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class VitrineService(
    IProductRepository productRepository,
    IStoreRepository storeRepository,
    ISettingRepository settingRepository,
    IProductCategoryRepository categoryRepository,
    IPriceRepository priceRepository,
    IInventoryStockRepository inventoryStockRepository,
    ILogger<VitrineService> logger) : IVitrineService
{
    public async Task<VitrineStoreInfoDto?> GetStoreInfoAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return null;

        var normalizedSlug = storeSlug.ToLowerInvariant().Trim();

        var store = await storeRepository.Get(new StoreBySlugSpecification(normalizedSlug))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (store == null)
            return null;

        var setting = await settingRepository.Get(new SettingsByStoreIdSpecification(store.Id))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cancellationToken);

        var liveGoldPrice18K = await GetLive18KGoldPriceAsync(cancellationToken);

        return new VitrineStoreInfoDto(
            Name: store.Name,
            Slug: store.Slug,
            LogoUrl: store.LogoUrl,
            BackgroundImageUrl: store.BackgroundImageUrl,
            Address: setting?.Address,
            PhoneNumber: setting?.PhoneNumber,
            InstagramUrl: setting?.InstagramUrl,
            TelegramUrl: setting?.TelegramUrl,
            BaleUrl: setting?.BaleUrl,
            WhatsAppNumber: setting?.WhatsAppNumber,
            AboutText: setting?.AboutText,
            LiveGoldPrice18K: liveGoldPrice18K);
    }

    public async Task<IReadOnlyList<VitrineCategoryDto>> GetCategoriesAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return [];

        var normalizedSlug = storeSlug.ToLowerInvariant().Trim();

        var store = await storeRepository.Get(new StoreBySlugSpecification(normalizedSlug))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (store == null)
            return [];

        var categories = await categoryRepository.Get(new ProductCategoriesByStoreIdSpecification(store.Id))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);

        var vitrineProducts = await productRepository.Get(new ProductsForVitrineSpecification(store.Id.Value))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Select(p => p.ProductCategoryId)
            .ToListAsync(cancellationToken);

        var countsByCategory = vitrineProducts
            .Where(cid => cid.HasValue)
            .GroupBy(cid => cid!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return categories
            .Select(c => new VitrineCategoryDto(
                Id: c.Id.Value,
                Title: c.Title,
                PrefixCode: c.PrefixCode,
                ProductCount: countsByCategory.GetValueOrDefault(c.Id, 0)))
            .Where(c => c.ProductCount > 0)
            .ToList();
    }

    public async Task<IReadOnlyList<VitrineProductSummaryDto>> GetVitrineProductsAsync(
        string storeSlug,
        Guid? categoryId = null,
        bool? isFeatured = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return [];

        var normalizedSlug = storeSlug.ToLowerInvariant().Trim();

        var store = await storeRepository.Get(new StoreBySlugSpecification(normalizedSlug))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (store == null)
            return [];

        var spec = new ProductsForVitrineSpecification(store.Id.Value);
        var query = productRepository.Get(spec)
            .AsNoTracking()
            .IgnoreQueryFilters();

        if (categoryId.HasValue)
        {
            var pCatId = new ProductCategoryId(categoryId.Value);
            query = query.Where(p => p.ProductCategoryId == pCatId);
        }

        if (isFeatured.HasValue && isFeatured.Value)
        {
            query = query.Where(p => p.IsFeatured);
        }

        var products = await query.ToListAsync(cancellationToken);
        var gramPrice750 = await GetLive18KGoldPriceAsync(cancellationToken);

        var productIds = products.Select(p => p.Id).ToList();
        var stockQuantities = await inventoryStockRepository.Get(new InventoryStocksDefaultSpecification())
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(s => s.StoreId == store.Id && s.ProductId != null && productIds.Contains(s.ProductId.Value))
            .GroupBy(s => s.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);

        return products.Select(p =>
        {
            var mainImage = p.Images?.OrderByDescending(x => x.IsMain).ThenBy(x => x.DisplayOrder).FirstOrDefault()?.Url;
            var isAvailable = stockQuantities.TryGetValue(p.Id, out var qty) && qty > 0.0001m;
            var effectiveWeight = p.Weight > 0 ? p.Weight : (stockQuantities.TryGetValue(p.Id, out var sq) && sq > 0 ? sq : 0m);
            var priceBreakdown = CalculateVitrinePrice(p, gramPrice750, effectiveWeight);

            return new VitrineProductSummaryDto(
                Id: p.Id.Value,
                Barcode: p.Barcode,
                Name: p.Name,
                Weight: effectiveWeight,
                Fineness: p.Fineness,
                ProductType: p.ProductType,
                CategoryId: p.ProductCategoryId?.Value,
                CategoryTitle: p.ProductCategory?.Title,
                MainImageUrl: mainImage,
                EstimatedPrice: priceBreakdown.EstimatedPrice,
                IsFeatured: p.IsFeatured,
                IsAvailable: isAvailable);
        }).ToList();
    }

    public async Task<VitrineProductDetailDto?> GetProductDetailAsync(
        string storeSlug,
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug) || string.IsNullOrWhiteSpace(barcode))
            return null;

        var normalizedSlug = storeSlug.ToLowerInvariant().Trim();

        var store = await storeRepository.Get(new StoreBySlugSpecification(normalizedSlug))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (store == null)
            return null;

        var spec = new ProductForVitrineByBarcodeSpecification(barcode.Trim(), store.Id.Value);
        var product = await productRepository.Get(spec)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return null;

        var gramPrice750 = await GetLive18KGoldPriceAsync(cancellationToken);

        var quantity = await inventoryStockRepository.Get(new InventoryStocksDefaultSpecification())
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(s => s.StoreId == store.Id && s.ProductId == product.Id)
            .SumAsync(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount, cancellationToken);

        var isAvailable = quantity > 0.0001m;
        var effectiveWeight = product.Weight > 0 ? product.Weight : (quantity > 0 ? quantity : 0m);
        var priceBreakdown = CalculateVitrinePrice(product, gramPrice750, effectiveWeight);

        var imageUrls = product.Images?
            .OrderByDescending(x => x.IsMain)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => x.Url)
            .ToList() ?? [];

        var gemstones = product.GemStones?
            .Select(s => new VitrineGemStoneDto(
                Type: s.Type,
                Color: s.Color,
                Carat: s.Carat,
                Cost: s.Cost))
            .ToList() ?? [];

        return new VitrineProductDetailDto(
            Id: product.Id.Value,
            Barcode: product.Barcode,
            Name: product.Name,
            Weight: effectiveWeight,
            Wage: product.Wage,
            WageType: product.WageType,
            Fineness: product.Fineness,
            ProductType: product.ProductType,
            CategoryId: product.ProductCategoryId?.Value,
            CategoryTitle: product.ProductCategory?.Title,
            Description: product.VitrineDescription,
            ImageUrls: imageUrls,
            GemStones: gemstones,
            EstimatedPrice: priceBreakdown.EstimatedPrice,
            RawGoldPrice: priceBreakdown.RawGoldPrice,
            WageAmount: priceBreakdown.WageAmount,
            ProfitAmount: priceBreakdown.ProfitAmount,
            TaxAmount: priceBreakdown.TaxAmount,
            GramPrice750: gramPrice750,
            UpdatedAt: DateTime.Now,
            IsAvailable: isAvailable);
    }

    public async Task UpdateProductVitrineAsync(
        Guid productId,
        UpdateProductVitrineRequest request,
        CancellationToken cancellationToken = default)
    {
        var spec = new ProductsByIdSpecification(new ProductId(productId));
        var product = await productRepository.Get(spec)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("محصول مورد نظر یافت نشد.");

        product.SetVitrineOptions(request.ShowInVitrine, request.IsFeatured, request.VitrineDescription);

        if (request.Images != null)
        {
            var images = request.Images.Select(x => ProductImage.Create(x.Url, x.IsMain, x.DisplayOrder)).ToList();
            product.SetImages(images);
        }

        await productRepository.UpdateAsync(product, cancellationToken);
    }

    #region Helper Methods

    private async Task<decimal> GetLive18KGoldPriceAsync(CancellationToken cancellationToken)
    {
        var spec = new PricesByPriceCatalogSpecification(PriceCatalog.Geram18);
        var gold18KPrice = await priceRepository.Get(spec)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Select(p => p.PriceHistory != null ? p.PriceHistory.CurrentValue : 0m)
            .FirstOrDefaultAsync(cancellationToken);

        if (gold18KPrice <= 0)
        {
            var defaultSpec = new PricesDefaultSpecification();
            var fallbackPrice = await priceRepository.Get(defaultSpec)
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(p => p.PriceHistory != null && p.PriceHistory.CurrentValue > 0)
                .Select(p => p.PriceHistory!.CurrentValue)
                .FirstOrDefaultAsync(cancellationToken);

            gold18KPrice = fallbackPrice;
        }

        var priceInRials = gold18KPrice > 0 ? gold18KPrice : 35_000_000m;
        // Database prices from TGJU are stored in Rials (IRR). Convert to Tomans (1 Toman = 10 Rials).
        return Math.Round(priceInRials / 10m, 0);
    }

    private static (decimal EstimatedPrice, decimal RawGoldPrice, decimal WageAmount, decimal ProfitAmount, decimal TaxAmount)
        CalculateVitrinePrice(Product product, decimal gramPrice750, decimal? overrideWeight = null)
    {
        var weight = (overrideWeight.HasValue && overrideWeight.Value > 0) ? overrideWeight.Value : product.Weight;
        var fineness = product.Fineness > 0 ? product.Fineness : 750m;
        var adjustedGramPrice = gramPrice750 * (fineness / 750m);
        var rawGoldPrice = Math.Round(weight * adjustedGramPrice, 0);

        decimal wageAmount = 0;
        if (product.Wage > 0)
        {
            if (product.WageType == WageType.Percent)
            {
                wageAmount = Math.Round(rawGoldPrice * (product.Wage / 100m), 0);
            }
            else
            {
                wageAmount = Math.Round(product.Wage * weight, 0);
            }
        }

        decimal stoneCost = 0;
        if (product.GemStones != null && product.GemStones.Count > 0)
        {
            stoneCost = product.GemStones.Sum(s => s.Cost);
        }

        var basePrice = rawGoldPrice + wageAmount + stoneCost;
        var profitAmount = Math.Round(basePrice * 0.07m, 0); // 7% standard retail profit
        var taxAmount = Math.Round((wageAmount + profitAmount) * 0.09m, 0); // 9% tax on wage+profit
        var estimatedPrice = basePrice + profitAmount + taxAmount;

        return (estimatedPrice, rawGoldPrice, wageAmount, profitAmount, taxAmount);
    }

    #endregion
}
