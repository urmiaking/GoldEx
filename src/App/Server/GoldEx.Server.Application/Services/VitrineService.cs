using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Shared.DTOs.Vitrine;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class VitrineService(
    IProductRepository productRepository,
    IStoreRepository storeRepository,
    ISettingRepository settingRepository,
    IProductCategoryRepository categoryRepository,
    IPriceRepository priceRepository,
    GoldExDbContext dbContext) : IVitrineService
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task<VitrineStoreInfoDto?> GetStoreInfoAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return null;

        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            var store = await ResolveStoreAsync(storeSlug, cancellationToken);
            if (store == null)
                return null;

            var setting = await settingRepository.Get(new SettingsByStoreIdSpecification(store.Id))
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(cancellationToken);

            var liveGoldPrice18K = await GetLive18KGoldPriceAsync(cancellationToken);

            var themeDto = setting != null
                ? new VitrineThemeDto(
                    Preset: setting.VitrineThemePreset ?? "royal-emerald",
                    PrimaryColor: setting.VitrinePrimaryColor,
                    AccentColor: setting.VitrineAccentColor,
                    BackgroundColor: setting.VitrineBackgroundColor,
                    SurfaceColor: setting.VitrineSurfaceColor,
                    CardStyle: setting.VitrineCardStyle ?? "minimal",
                    RadiusStyle: setting.VitrineRadiusStyle ?? "rounded",
                    FontStyle: setting.VitrineFontStyle ?? "vazirmatn",
                    HeaderStyle: setting.VitrineHeaderStyle ?? "glass-sticky")
                : new VitrineThemeDto();

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
                LiveGoldPrice18K: liveGoldPrice18K,
                CustomDomain: store.CustomDomain,
                Theme: themeDto);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<IReadOnlyList<VitrineCategoryDto>> GetCategoriesAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return [];

        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            var store = await ResolveStoreAsync(storeSlug, cancellationToken);
            if (store == null)
                return [];

            var storeId = store.Id;

            var categories = await categoryRepository.Get(new ProductCategoriesByStoreIdSpecification(storeId))
                .AsNoTracking()
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            var countsByCategory = await dbContext.Set<Product>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(p => p.StoreId == storeId && p.ShowInVitrine && (p.ProductType == ProductType.Gold || p.ProductType == ProductType.Jewelry) && p.ProductCategoryId != null)
                .GroupBy(p => p.ProductCategoryId!.Value)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

            return categories
                .Select(c => new VitrineCategoryDto(
                    Id: c.Id.Value,
                    Title: c.Title,
                    PrefixCode: c.PrefixCode,
                    ProductCount: countsByCategory.GetValueOrDefault(c.Id, 0)))
                .Where(c => c.ProductCount > 0)
                .ToList();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<IReadOnlyList<VitrineProductSummaryDto>> GetVitrineProductsAsync(
        string storeSlug,
        Guid? categoryId = null,
        bool? isFeatured = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug))
            return [];

        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            var store = await ResolveStoreAsync(storeSlug, cancellationToken);
            if (store == null)
                return [];

            var storeId = store.Id;

            var baseQuery = dbContext.Set<Product>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AsSingleQuery()
                .Where(p => p.StoreId == storeId && p.ShowInVitrine && (p.ProductType == ProductType.Gold || p.ProductType == ProductType.Jewelry));

            if (categoryId.HasValue)
            {
                var pCatId = new ProductCategoryId(categoryId.Value);
                baseQuery = baseQuery.Where(p => p.ProductCategoryId == pCatId);
            }

            if (isFeatured.HasValue && isFeatured.Value)
            {
                baseQuery = baseQuery.Where(p => p.IsFeatured);
            }

            var rawProducts = await baseQuery
                .Select(p => new VitrineProductRawProjection
                {
                    Id = p.Id.Value,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    Weight = p.Weight,
                    Fineness = p.Fineness,
                    ProductType = p.ProductType,
                    Wage = p.Wage,
                    WageType = p.WageType,
                    CategoryId = p.ProductCategoryId != null ? (Guid?)p.ProductCategoryId.Value.Value : null,
                    CategoryTitle = p.ProductCategory != null ? p.ProductCategory.Title : null,
                    IsFeatured = p.IsFeatured,
                    MainImageUrl = p.Images
                        .OrderByDescending(img => img.IsMain)
                        .ThenBy(img => img.DisplayOrder)
                        .Select(img => img.Url)
                        .FirstOrDefault(),
                    GemStoneTotalCost = p.GemStones.Sum(s => (decimal?)s.Cost) ?? 0m,
                    Attributes = p.AttributeValues
                        .Where(v => v.Attribute != null)
                        .Select(v => new VitrineAttributeProjection
                        {
                            AttributeId = v.AttributeId.Value,
                            Title = v.Attribute!.Title,
                            Unit = v.Attribute.Unit,
                            Value = v.Value,
                            NumericValue = v.NumericValue,
                            DataType = v.Attribute.DataType
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            if (rawProducts.Count == 0)
                return [];

            var gramPrice750 = await GetLive18KGoldPriceAsync(cancellationToken);

            var productIds = rawProducts.Select(p => new ProductId(p.Id)).ToList();
            var stockQuantities = await dbContext.Set<InventoryStock>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(s => s.StoreId == storeId && s.ProductId != null && productIds.Contains(s.ProductId.Value))
                .GroupBy(s => s.ProductId!.Value)
                .Select(g => new
                {
                    ProductId = g.Key.Value,
                    TotalQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalQuantity, cancellationToken);

            return rawProducts.Select(p =>
            {
                var isAvailable = stockQuantities.TryGetValue(p.Id, out var qty) && qty > 0.0001m;
                var effectiveWeight = p.Weight > 0 ? p.Weight : (stockQuantities.TryGetValue(p.Id, out var sq) && sq > 0 ? sq : 0m);
                var priceBreakdown = CalculateVitrinePriceFromRaw(p.Weight, p.Fineness, p.Wage, p.WageType, p.GemStoneTotalCost, gramPrice750, effectiveWeight);

                var attributes = p.Attributes
                    .Select(v => new VitrineAttributeValueDto(
                        AttributeId: v.AttributeId,
                        Title: v.Title,
                        Unit: v.Unit,
                        Value: v.Value,
                        NumericValue: v.NumericValue,
                        DataType: v.DataType,
                        DisplayOrder: 999))
                    .OrderBy(x => x.DisplayOrder)
                    .ThenBy(x => x.Title)
                    .ToList();

                return new VitrineProductSummaryDto(
                    Id: p.Id,
                    Barcode: p.Barcode,
                    Name: p.Name,
                    Weight: effectiveWeight,
                    Fineness: p.Fineness,
                    ProductType: p.ProductType,
                    CategoryId: p.CategoryId,
                    CategoryTitle: p.CategoryTitle,
                    MainImageUrl: p.MainImageUrl,
                    EstimatedPrice: priceBreakdown.EstimatedPrice,
                    IsFeatured: p.IsFeatured,
                    IsAvailable: isAvailable,
                    Attributes: attributes,
                    Wage: p.Wage,
                    WageType: p.WageType,
                    WageAmount: priceBreakdown.WageAmount);
            }).ToList();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<VitrineProductDetailDto?> GetProductDetailAsync(
        string storeSlug,
        string barcode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storeSlug) || string.IsNullOrWhiteSpace(barcode))
            return null;

        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            var store = await ResolveStoreAsync(storeSlug, cancellationToken);
            if (store == null)
                return null;

            var storeId = store.Id;
            var cleanBarcode = barcode.Trim();

            var rawProduct = await dbContext.Set<Product>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AsSingleQuery()
                .Where(p => p.StoreId == storeId && p.Barcode == cleanBarcode && p.ShowInVitrine && (p.ProductType == ProductType.Gold || p.ProductType == ProductType.Jewelry))
                .Select(p => new VitrineProductDetailRawProjection
                {
                    Id = p.Id.Value,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    Weight = p.Weight,
                    Wage = p.Wage,
                    WageType = p.WageType,
                    Fineness = p.Fineness,
                    ProductType = p.ProductType,
                    CategoryId = p.ProductCategoryId != null ? (Guid?)p.ProductCategoryId.Value.Value : null,
                    CategoryTitle = p.ProductCategory != null ? p.ProductCategory.Title : null,
                    Description = p.VitrineDescription,
                    ImageUrls = p.Images
                        .OrderByDescending(x => x.IsMain)
                        .ThenBy(x => x.DisplayOrder)
                        .Select(x => x.Url)
                        .ToList(),
                    GemStones = p.GemStones
                        .Select(s => new VitrineGemStoneProjection
                        {
                            Type = s.Type,
                            Color = s.Color,
                            Carat = s.Carat,
                            Cost = s.Cost
                        })
                        .ToList(),
                    Attributes = p.AttributeValues
                        .Where(v => v.Attribute != null)
                        .Select(v => new VitrineAttributeProjection
                        {
                            AttributeId = v.AttributeId.Value,
                            Title = v.Attribute!.Title,
                            Unit = v.Attribute.Unit,
                            Value = v.Value,
                            NumericValue = v.NumericValue,
                            DataType = v.Attribute.DataType
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (rawProduct == null)
                return null;

            var gramPrice750 = await GetLive18KGoldPriceAsync(cancellationToken);

            var quantity = await dbContext.Set<InventoryStock>()
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(s => s.StoreId == storeId && s.ProductId == new ProductId(rawProduct.Id))
                .SumAsync(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount, cancellationToken);

            var isAvailable = quantity > 0.0001m;
            var effectiveWeight = rawProduct.Weight > 0 ? rawProduct.Weight : (quantity > 0 ? quantity : 0m);
            var gemStoneTotalCost = rawProduct.GemStones.Sum(s => s.Cost);
            var priceBreakdown = CalculateVitrinePriceFromRaw(rawProduct.Weight, rawProduct.Fineness, rawProduct.Wage, rawProduct.WageType, gemStoneTotalCost, gramPrice750, effectiveWeight);

            var gemstones = rawProduct.GemStones
                .Select(s => new VitrineGemStoneDto(
                    Type: s.Type,
                    Color: s.Color,
                    Carat: s.Carat,
                    Cost: s.Cost))
                .ToList();

            var attributes = rawProduct.Attributes
                .Select(v => new VitrineAttributeValueDto(
                    AttributeId: v.AttributeId,
                    Title: v.Title,
                    Unit: v.Unit,
                    Value: v.Value,
                    NumericValue: v.NumericValue,
                    DataType: v.DataType,
                    DisplayOrder: 999))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .ToList();

            return new VitrineProductDetailDto(
                Id: rawProduct.Id,
                Barcode: rawProduct.Barcode,
                Name: rawProduct.Name,
                Weight: effectiveWeight,
                Wage: rawProduct.Wage,
                WageType: rawProduct.WageType,
                Fineness: rawProduct.Fineness,
                ProductType: rawProduct.ProductType,
                CategoryId: rawProduct.CategoryId,
                CategoryTitle: rawProduct.CategoryTitle,
                Description: rawProduct.Description,
                ImageUrls: rawProduct.ImageUrls,
                GemStones: gemstones,
                EstimatedPrice: priceBreakdown.EstimatedPrice,
                RawGoldPrice: priceBreakdown.RawGoldPrice,
                WageAmount: priceBreakdown.WageAmount,
                ProfitAmount: priceBreakdown.ProfitAmount,
                TaxAmount: priceBreakdown.TaxAmount,
                GramPrice750: gramPrice750,
                UpdatedAt: DateTime.Now,
                IsAvailable: isAvailable,
                Attributes: attributes);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task UpdateProductVitrineAsync(
        Guid productId,
        UpdateProductVitrineRequest request,
        CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken);
        try
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

            if (request.AttributeValues != null && request.AttributeValues.Any())
            {
                var attrValues = request.AttributeValues.Select(v =>
                    ProductAttributeValue.Create(
                        new ProductAttributeId(v.AttributeId),
                        v.Value,
                        v.NumericValue ?? (decimal.TryParse(v.Value, out var n) ? n : null))).ToList();
                product.SetAttributeValues(attrValues);
            }
            else if (request.AttributeValues != null)
            {
                product.ClearAttributeValues();
            }

            await productRepository.UpdateAsync(product, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    #region Helper Methods

    private async Task<GoldEx.Server.Domain.StoreAggregate.Store?> ResolveStoreAsync(string? storeSlugOrDomain, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(storeSlugOrDomain))
            return null;

        var normalized = storeSlugOrDomain.ToLowerInvariant().Trim();

        // 1. Try resolving by store slug
        var store = await storeRepository.Get(new StoreBySlugSpecification(normalized))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (store != null)
            return store;

        // 2. Try resolving by store custom domain
        store = await storeRepository.Get(new StoreByCustomDomainSpecification(normalized))
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        return store;
    }

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
        CalculateVitrinePriceFromRaw(decimal weight, decimal fineness, decimal wage, WageType? wageType, decimal stoneCost, decimal gramPrice750, decimal? overrideWeight = null)
    {
        var effectiveWeight = (overrideWeight.HasValue && overrideWeight.Value > 0) ? overrideWeight.Value : weight;
        var effectiveFineness = fineness > 0 ? fineness : 750m;
        var adjustedGramPrice = gramPrice750 * (effectiveFineness / 750m);
        var rawGoldPrice = Math.Round(effectiveWeight * adjustedGramPrice, 0);

        decimal wageAmount = 0;
        if (wage > 0)
        {
            if (wageType == WageType.Percent)
            {
                wageAmount = Math.Round(rawGoldPrice * (wage / 100m), 0);
            }
            else
            {
                wageAmount = Math.Round(wage * effectiveWeight, 0);
            }
        }

        var basePrice = rawGoldPrice + wageAmount + stoneCost;
        var profitAmount = Math.Round(basePrice * 0.07m, 0); // 7% standard retail profit
        var taxAmount = Math.Round((wageAmount + profitAmount) * 0.09m, 0); // 9% tax on wage+profit
        var estimatedPrice = basePrice + profitAmount + taxAmount;

        return (estimatedPrice, rawGoldPrice, wageAmount, profitAmount, taxAmount);
    }

    private static (decimal EstimatedPrice, decimal RawGoldPrice, decimal WageAmount, decimal ProfitAmount, decimal TaxAmount)
        CalculateVitrinePrice(Product product, decimal gramPrice750, decimal? overrideWeight = null)
    {
        var stoneCost = product.GemStones?.Sum(s => s.Cost) ?? 0m;
        return CalculateVitrinePriceFromRaw(product.Weight, product.Fineness, product.Wage, product.WageType, stoneCost, gramPrice750, overrideWeight);
    }

    #endregion
}

internal sealed class VitrineProductRawProjection
{
    public Guid Id { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public decimal Fineness { get; init; }
    public ProductType ProductType { get; init; }
    public decimal Wage { get; init; }
    public WageType? WageType { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryTitle { get; init; }
    public bool IsFeatured { get; init; }
    public string? MainImageUrl { get; init; }
    public decimal GemStoneTotalCost { get; init; }
    public List<VitrineAttributeProjection> Attributes { get; init; } = [];
}

internal sealed class VitrineProductDetailRawProjection
{
    public Guid Id { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public decimal Wage { get; init; }
    public WageType? WageType { get; init; }
    public decimal Fineness { get; init; }
    public ProductType ProductType { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryTitle { get; init; }
    public string? Description { get; init; }
    public List<string> ImageUrls { get; init; } = [];
    public List<VitrineGemStoneProjection> GemStones { get; init; } = [];
    public List<VitrineAttributeProjection> Attributes { get; init; } = [];
}

internal sealed class VitrineGemStoneProjection
{
    public string Type { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public decimal Carat { get; init; }
    public decimal Cost { get; init; }
}

internal sealed class VitrineAttributeProjection
{
    public Guid AttributeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string Value { get; init; } = string.Empty;
    public decimal? NumericValue { get; init; }
    public ProductAttributeDataType DataType { get; init; }
}
