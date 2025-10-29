using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryStocks;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryStockService(
    IInventoryStockRepository repository,
    IServerProductService productService,
    IMapper mapper,
    MeltUsedProductsValidator usedProductsValidator) 
    : IServerInventoryStockService, IInventoryStockService
{
    #region Server Service

    private static DateTime ComposePostingDate(Invoice invoice, long tickOffset = 0)
        => invoice.InvoiceDate.ToDateTime(TimeOnly.FromTimeSpan(invoice.CreatedAt.TimeOfDay)).AddTicks(tickOffset);

    // Active = نه رکورد برگشتی و نه اصلی که قبلاً برگشت خورده
    private async Task<List<InventoryStock>> GetActiveStocksForInvoiceAsync(Invoice invoice, CancellationToken ct)
    {
        var all = await repository
            .Get(new InventoryStocksByInvoiceIdSpecification(invoice.Id, null))
            .AsNoTracking()
            .ToListAsync(ct);

        var reversedOriginalIds = all
            .Where(x => x.ReverseInventoryStockId != null)
            .Select(x => x.ReverseInventoryStockId!.Value)
            .ToHashSet();

        return all
            .Where(x => x.ReverseInventoryStockId == null && !reversedOriginalIds.Contains(x.Id))
            .ToList();
    }

    private static WarehouseActionType Invert(WarehouseActionType t) =>
        t == WarehouseActionType.In ? WarehouseActionType.Out : WarehouseActionType.In;

    private static (ItemType type, Guid id) KeyOf(InventoryStock s)
    {
        if (s.ProductId is { } p) return (ItemType.Product, p.Value);
        if (s.CoinId is { } c) return (ItemType.Coin, c.Value);
        if (s.CurrencyId is { } u) return (ItemType.Currency, u.Value);
        throw new InvalidOperationException("Unknown inventory stock item type.");
    }

    private readonly record struct StockSignature(
        ItemType ItemType,
        Guid ItemId,
        decimal Amount,
        WarehouseActionType ActionType,
        InvoiceId? InvoiceId
    );

    private static StockSignature Sig(InventoryStock s)
    {
        var (t, id) = KeyOf(s);
        return new StockSignature(t, id, s.ChangeAmount, s.ActionType, s.InvoiceId);
    }

    // Builder: رکوردهای مطلوب جدید را می‌سازد (Persist نمی‌کند)
    private static List<InventoryStock> BuildStocksForInvoice(Invoice invoice, long tickOffset)
    {
        var postingDate = ComposePostingDate(invoice, tickOffset); ;

        var action = invoice.InvoiceType == InvoiceType.Purchase ? WarehouseActionType.In : WarehouseActionType.Out;

        // Product items
        var list = invoice.ProductItems.Select(pi => InventoryStock.CreateProduct(pi.ProductId,
                pi.TotalWeight,
                action,
                invoice.Id,
                postingDate))
            .ToList();

        // Instant products → ورود
        list.AddRange(invoice.ProductItems.Where(x => x.IsInstantProduct)
            .Select(pi => InventoryStock.CreateProduct(pi.ProductId,
                pi.TotalWeight,
                WarehouseActionType.In,
                invoice.Id,
                postingDate)));

        // Coins
        list.AddRange(invoice.CoinItems.Select(ci => InventoryStock.CreateCoin(ci.CoinId,
            ci.Quantity,
            action,
            invoice.Id,
            postingDate)));

        // Currencies
        list.AddRange(invoice.CurrencyItems.Select(cu => InventoryStock.CreateCurrency(cu.CurrencyId,
            cu.Amount,
            action,
            invoice.Id,
            postingDate)));

        // Used products → ورود
        list.AddRange(invoice.UsedProducts.Where(x => x.ProductId != null)
            .Select(up => InventoryStock.CreateProduct(up.ProductId!.Value,
                up.Weight,
                WarehouseActionType.In,
                invoice.Id,
                postingDate)));

        return list;
    }

    // Build reversals: PostingDate = original.PostingDate + 1 tick
    private static List<InventoryStock> BuildReversalStocks(IEnumerable<InventoryStock> originals)
    {
        var res = new List<InventoryStock>();
        foreach (var s in originals)
        {
            var revDate = s.PostingDate.AddTicks(1);
            InventoryStock rev;

            if (s.ProductId is { } p)
                rev = InventoryStock.CreateProduct(p, s.ChangeAmount, Invert(s.ActionType), s.InvoiceId, revDate);
            else if (s.CoinId is { } c)
                rev = InventoryStock.CreateCoin(c, (int)s.ChangeAmount, Invert(s.ActionType), s.InvoiceId, revDate);
            else if (s.CurrencyId is { } u)
                rev = InventoryStock.CreateCurrency(u, s.ChangeAmount, Invert(s.ActionType), s.InvoiceId, revDate);
            else
                continue;

            rev.MarkAsReversalOf(s.Id);
            res.Add(rev);
        }
        return res;
    }

    public async Task CreateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var stocks = BuildStocksForInvoice(invoice, tickOffset: 0);
        if (stocks.Count > 0)
            await repository.CreateRangeAsync(stocks, cancellationToken);
    }

    // Delta-based Replace (برای Update)
    public async Task ReplaceInventoryForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var active = await GetActiveStocksForInvoiceAsync(invoice, cancellationToken);
        var preview0 = BuildStocksForInvoice(invoice, tickOffset: 0);

        var activeSet = active.Select(Sig).ToHashSet();
        var previewSet = preview0.Select(Sig).ToHashSet();

        var toReverse = active.Where(x => !previewSet.Contains(Sig(x))).ToList();
        var toPost = preview0.Where(x => !activeSet.Contains(Sig(x))).ToList();

        if (toReverse.Count == 0 && toPost.Count == 0) return;

        if (toReverse.Count > 0)
        {
            var reversals = BuildReversalStocks(toReverse);
            await repository.CreateRangeAsync(reversals, cancellationToken);
        }

        if (toPost.Count > 0)
        {
            // ثبت مجدد: +2 ticks برای نمایش بعد از برگشت‌ها
            var preview2 = BuildStocksForInvoice(invoice, tickOffset: 2);
            // فیلتر به اقلام واقعی جدید
            var preview2Map = preview2.GroupBy(Sig).ToDictionary(g => g.Key, g => g.ToList());
            var realToPost = new List<InventoryStock>();
            foreach (var s in toPost)
            {
                if (preview2Map.TryGetValue(Sig(s), out var list))
                    realToPost.AddRange(list);
            }

            if (realToPost.Count > 0)
                await repository.CreateRangeAsync(realToPost, cancellationToken);
        }
    }

    public async Task RemoveInventoryByInvoiceIdAsync(InvoiceId invoiceId, ItemType? itemType,
        CancellationToken cancellationToken = default)
    {
        var inventoryItems = await repository
            .Get(new InventoryStocksByInvoiceIdSpecification(invoiceId, itemType))
            .ToListAsync(cancellationToken);

        await repository.DeleteRangeAsync(inventoryItems, cancellationToken);
    }

    public async Task MeltProductsAsync(MeltingBatchId meltingBatchId, List<ProductId> productIds, CancellationToken cancellationToken = default)
    {
        await usedProductsValidator.ValidateAndThrowAsync(productIds, cancellationToken);

        List<InventoryStock> inventoryItems = [];

        foreach (var productId in productIds)
        {
            var inventoryStocks = InventoryStock.CreateMeltingBatchProduct(productId, 1, WarehouseActionType.Out, meltingBatchId);
            inventoryItems.Add(inventoryStocks);
        }

        if (inventoryItems.Any())
            await repository.CreateRangeAsync(inventoryItems, cancellationToken);
    }

    public async Task CreateMoltenGoldAsync(MeltingBatch meltingBatch, string assayNumber, decimal fineness, decimal weight,
        CancellationToken cancellationToken = default)
    {
        var moltenGoldDetail = MoltenGoldDetail.Create(weight, meltingBatch.WeightUnitType, assayNumber, fineness, meltingBatch.AssayerId!.Value);

        var product = await productService.CreateProductAsync(new ProductRequestDto(null,
            null,
            null,
            weight,
            0,
            null,
            ProductType.MoltenGold,
            fineness,
            meltingBatch.WeightUnitType,
            null,
            null,
            null,
            null,
            new MoltenGoldDto(assayNumber,
                moltenGoldDetail.AssayerId.Value,
                DateTime.Now)), 
            cancellationToken);

        var inventoryItem = InventoryStock.CreateMoltenGold(product.Id, meltingBatch.Id, moltenGoldDetail, weight, WarehouseActionType.In);

        await repository.CreateAsync(inventoryItem, cancellationToken);
    }

    #endregion

    #region Shared Service

    public async Task<PagedList<GetInventoryStockResponse>> GetListAsync(RequestFilter filter, InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default)
    {
        var summary = await repository.GetInventorySummaryAsync(filter, inventoryFilter, cancellationToken);

        return new PagedList<GetInventoryStockResponse>
        {
            Data = mapper.Map<List<GetInventoryStockResponse>>(summary.Data),
            Total = summary.Total,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 100
        };
    }

    public async Task<PagedList<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest calculatorFilter, RequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        var candidateProducts = await repository.GetAvailableInventorySummaryAsync(filter, calculatorFilter, cancellationToken);

        var results = new List<GetInventoryStockResponse>();

        foreach (var item in candidateProducts.Data)
        {
            if (item.Product is null)
                continue;

            var rawPrice = CalculatorHelper.Product.CalculateRawPrice(item.CurrentAmount, calculatorFilter.GramPrice, item.Product.Fineness,
                1, item.Product.ProductType);
            var wageAmount = CalculatorHelper.Product.CalculateWage(rawPrice, item.CurrentAmount, item.Product.Wage, item.Product.WageType, null);
            var profitAmount = CalculatorHelper.Product.CalculateProfit(rawPrice, wageAmount, item.Product.ProductType, calculatorFilter.ProfitPercent);
            var taxAmount = CalculatorHelper.Product.CalculateTax(wageAmount, profitAmount, calculatorFilter.TaxPercent, item.Product.ProductType, null);

            var finalPrice = rawPrice + wageAmount + profitAmount + taxAmount;

            if ((!calculatorFilter.MinPrice.HasValue || finalPrice >= calculatorFilter.MinPrice.Value) &&
                (!calculatorFilter.MaxPrice.HasValue || finalPrice <= calculatorFilter.MaxPrice.Value))
            {
                results.Add(mapper.Map<GetInventoryStockResponse>(item));
            }
        }

        return new PagedList<GetInventoryStockResponse>
        {
            Data = mapper.Map<List<GetInventoryStockResponse>>(results),
            Total = results.Count,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 100
        };
    }

    public async Task<List<GetInventoryWeightChartResponse>> GetInventoryWeightChartAsync(GoldUnitType targetUnit,
        CancellationToken cancellationToken = default)
    {
        var summary = await repository.GetInventoryWeightChartDataAsync(targetUnit, cancellationToken);

        return mapper.Map<List<GetInventoryWeightChartResponse>>(summary);
    }

    public async Task<PagedList<GetInventoryStockItemResponse>> GetInvoiceInventoryItemsAsync(Guid invoiceId,
        RequestFilter requestFilter, CancellationToken cancellationToken = default)
    {
        var spec = new InventoryStocksByInvoiceFilterSpecification(new InvoiceId(invoiceId), requestFilter);

        var items = await repository.Get(spec).ToListAsync(cancellationToken);

        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetInventoryStockItemResponse>
        {
            Data = mapper.Map<List<GetInventoryStockItemResponse>>(items),
            Total = total,
            Skip = requestFilter.Skip ?? 0,
            Take = requestFilter.Take ?? 100
        };
    }

    #endregion
}