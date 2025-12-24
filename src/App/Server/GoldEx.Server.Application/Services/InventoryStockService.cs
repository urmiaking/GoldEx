using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryStocks;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.DTOs.InventoryExits;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryStockService(
    IInventoryStockRepository repository,
    IProductRepository productRepository,
    ITransactionRepository transactionRepository,
    IServiceProvider serviceProvider,
    IMapper mapper,
    MeltUsedProductsValidator usedProductsValidator,
    DeleteInventoryStockProductValidator deleteValidator,
    ILogger<InventoryStockService> logger) 
    : IServerInventoryStockService, IInventoryStockService
{
    #region Server Service

    private static DateTime ComposePostingDate(Invoice invoice, long tickOffset = 0)
        => invoice.InvoiceDate.ToDateTime(TimeOnly.FromTimeSpan(invoice.CreatedAt.TimeOfDay)).AddTicks(tickOffset);

    // Active = نه رکورد برگشتی و نه اصلی که قبلاً برگشت خورده
    private async Task<List<InventoryStock>> GetActiveStocksForInvoiceAsync(Invoice invoice, CancellationToken ct)
    {
        var all = await repository
            .Get(new InventoryStocksByInvoiceIdSpecification(invoice.Id))
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
        if (s.CoinInstanceId is { } c) return (ItemType.Coin, c.Value);
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
        var postingDate = ComposePostingDate(invoice, tickOffset);

        var action = invoice.InvoiceType == InvoiceType.Purchase ? WarehouseActionType.In : WarehouseActionType.Out;

        // Instant products → ورود
        var list = invoice.ProductItems.Where(x => x.IsInstantProduct)
            .Select(pi => InventoryStock.CreateProduct(pi.ProductId,
                pi.TotalWeight,
                WarehouseActionType.In,
                invoice.Id,
                postingDate)).ToList();

        // Instant coins → ورود
        list.AddRange(invoice.CoinItems.Where(x => x.IsInstant)
            .Select(ci => InventoryStock.CreateCoin(ci.CoinInstanceId,
                ci.Quantity,
                WarehouseActionType.In,
                invoice.Id,
                postingDate)));

        // Product items
        list.AddRange(invoice.ProductItems.Select(pi => InventoryStock.CreateProduct(pi.ProductId,
            pi.TotalWeight,
            action,
            invoice.Id,
            postingDate)));

        // Coins
        list.AddRange(invoice.CoinItems.Select(ci => InventoryStock.CreateCoin(ci.CoinInstanceId,
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
            else if (s.CoinInstanceId is { } c)
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

    public async Task<(InventoryStock? OutStock, InventoryStock? InStock)> UpdateStockAsync(ProductId id, decimal weight, CancellationToken cancellationToken = default)
    {
        // 1. Get current balance
        var currentQuantity = await repository.GetQuantityAsync(id, cancellationToken);

        // If no change needed, exit
        if (currentQuantity == weight) return (null, null);

        var stocksToCreate = new List<InventoryStock>();
        var now = DateTime.Now;

        InventoryStock? outStock = null;
        InventoryStock? inStock = null;

        // 2. Remove the OLD total
        // This brings the theoretical balance to 0.
        if (currentQuantity > 0)
        {
            outStock = InventoryStock.CreateProduct(
                id,
                currentQuantity,
                WarehouseActionType.Out,
                null,
                now
            );
            stocksToCreate.Add(outStock);
        }
        else if (currentQuantity < 0)
        {
            // If balance was somehow negative, add to make it 0
            stocksToCreate.Add(InventoryStock.CreateProduct(
                id,
                Math.Abs(currentQuantity),
                WarehouseActionType.In,
                null,
                now
            ));
        }

        // 3. Add the NEW total
        if (weight > 0)
        {
            var entryDate = stocksToCreate.Any() ? now.AddTicks(1) : now;
            inStock = InventoryStock.CreateProduct(
                id,
                weight,
                WarehouseActionType.In,
                null,
                entryDate
            );
            stocksToCreate.Add(inStock);
        }

        if (stocksToCreate.Count > 0) 
            await repository.CreateRangeAsync(stocksToCreate, cancellationToken);

        return (outStock, inStock);
    }

    public async Task<List<InventoryStock>> ExitInventoryAsync(InventoryExitId inventoryExitId,
        CreateInventoryExitRequest request, CancellationToken cancellationToken = default)
    {
        var inventoryStocks = request.Products.Select(productRequest => InventoryStock.CreateProduct(
                new ProductId(productRequest.ProductId),
                productRequest.Weight,
                WarehouseActionType.Out,
                postingDate: request.ExitDate,
                inventoryExitId: inventoryExitId))
            .ToList();

        inventoryStocks.AddRange(request.Coins.Select(coinRequest => InventoryStock.CreateCoin(
            new CoinInstanceId(coinRequest.Id),
            coinRequest.Quantity,
            WarehouseActionType.Out,
            postingDate: request.ExitDate,
            inventoryExitId: inventoryExitId)));

        if (inventoryStocks.Any())
        {
            await repository.CreateRangeAsync(inventoryStocks, cancellationToken);
            return inventoryStocks;
        }

        return [];
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
            var currentQuantity = await repository.GetQuantityAsync(productId, cancellationToken);

            var inventoryStocks = InventoryStock.CreateMeltingBatchProduct(productId, currentQuantity, WarehouseActionType.Out, meltingBatchId);
            inventoryItems.Add(inventoryStocks);
        }

        if (inventoryItems.Any())
            await repository.CreateRangeAsync(inventoryItems, cancellationToken);
    }

    public async Task CreateMoltenGoldAsync(MeltingBatch meltingBatch, string assayNumber, decimal fineness, decimal weight,
        CancellationToken cancellationToken = default)
    {
        var moltenGoldDetail = MoltenGoldDetail.Create(weight, meltingBatch.WeightUnitType, assayNumber, fineness, meltingBatch.AssayerId!.Value);

        var productService = serviceProvider.GetRequiredService<IServerProductService>();

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
            null, 
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
            Total = candidateProducts.Total,
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

        var items = await repository.Get(spec)
            .Include(x => x.Invoice!.CurrencyItems)
                .ThenInclude(x => x.FinancialAccount)
            .Include(x => x.InventoryEntry)
            .Include(x => x.MeltingBatch)
            .Include(x => x.CoinInstance!.Coin)
            .ToListAsync(cancellationToken);

        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetInventoryStockItemResponse>
        {
            Data = mapper.Map<List<GetInventoryStockItemResponse>>(items),
            Total = total,
            Skip = requestFilter.Skip ?? 0,
            Take = requestFilter.Take ?? 100
        };
    }

    public async Task<PagedList<GetInventoryStockTraceResponse>> GetInventoryStockTracesAsync(Guid itemId, ItemType itemType, RequestFilter requestFilter,
        CancellationToken cancellationToken = default)
    {
        var spec = new InventoryStocksForTraceSpecification(itemId, itemType, requestFilter);

        var items = await repository.Get(spec)
            .Include(x => x.Invoice!.CurrencyItems)
                .ThenInclude(x => x.FinancialAccount)
            .Include(x => x.Transactions)
            .Include(x => x.CoinInstance!.Coin)
            .ToListAsync(cancellationToken);

        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetInventoryStockTraceResponse>
        {
            Data = mapper.Map<List<GetInventoryStockTraceResponse>>(items),
            Total = total,
            Skip = requestFilter.Skip ?? 0,
            Take = requestFilter.Take ?? 100
        };
    }

    public async Task<GetInventoryStockAmountResponse> GetAvailableItemAmountAsync(Guid itemId, ItemType itemType, CancellationToken cancellationToken = default)
    {
        decimal amount;

        switch (itemType)
        {
            case ItemType.Coin:
                amount = await repository.GetQuantityAsync(new CoinInstanceId(itemId), cancellationToken);
                break;
            case ItemType.Currency:
                amount = await repository.GetQuantityAsync(new PriceUnitId(itemId), cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }

        return new GetInventoryStockAmountResponse(amount);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(productId, cancellationToken);

        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var inventoryStocks = await repository
                .Get(new InventoryStocksByProductIdSpecification(new ProductId(productId)))
                .ToListAsync(cancellationToken);

            // Collect transactions
            var transactions = inventoryStocks
                .SelectMany(x => x.Transactions!)
                .ToList();

            // 1. Delete transactions
            await transactionRepository.DeleteRangeAsync(transactions, cancellationToken);

            // 2. Delete inventory stocks
            await repository.DeleteRangeAsync(inventoryStocks, cancellationToken);

            // 3. Delete product
            var product = await productRepository.Get(new ProductsByIdSpecification(new ProductId(productId))).FirstOrDefaultAsync(cancellationToken);
            await productRepository.DeleteAsync(product!, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    #endregion
}