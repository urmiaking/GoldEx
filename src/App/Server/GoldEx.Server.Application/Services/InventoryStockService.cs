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

    public async Task SetForProductAsync(ProductId productId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
       CancellationToken cancellationToken = default)
    {
        // TODO: add validation for product existence and availability

        if (invoiceId.HasValue)
        {
            var inventoryItems = await repository
                .Get(new InventoryStocksByInvoiceIdSpecification(invoiceId.Value, ItemType.Product))
                .ToListAsync(cancellationToken);

            var inventoryItem = inventoryItems.FirstOrDefault(x => x.ProductId == productId);

            if (inventoryItem != null)
            {
                await repository.DeleteAsync(inventoryItem, cancellationToken);
                await repository.CreateAsync(InventoryStock.CreateProduct(productId, quantity, actionType, invoiceId), cancellationToken);
            }
        }
        else
        {
            // TODO: after implementing warehousing, this method should be changed
            await repository.CreateAsync(InventoryStock.CreateProduct(productId, quantity, actionType), cancellationToken);
        }

    }

    public async Task SetForCoinAsync(CoinId coinId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default)
    {
        // TODO: add validation for coin existence and availability

        if (invoiceId.HasValue)
        {
            var inventoryItems = await repository
                .Get(new InventoryStocksByInvoiceIdSpecification(invoiceId.Value, ItemType.Coin))
                .ToListAsync(cancellationToken);

            var inventoryItem = inventoryItems.FirstOrDefault(x => x.CoinId == coinId);

            if (inventoryItem != null)
            {
                await repository.DeleteAsync(inventoryItem, cancellationToken);
                await repository.CreateAsync(InventoryStock.CreateCoin(coinId, quantity, actionType, invoiceId), cancellationToken);
            }
        }
        else
        {
            await repository.CreateAsync(InventoryStock.CreateCoin(coinId, quantity, actionType, invoiceId), cancellationToken);
        }
    }

    public async Task SetForCurrencyAsync(PriceUnitId currencyId, decimal amount, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default)
    {
        // TODO: add validation for currency existence and availability

        if (invoiceId.HasValue)
        {
            var inventoryItems = await repository
                .Get(new InventoryStocksByInvoiceIdSpecification(invoiceId.Value, ItemType.Currency))
                .ToListAsync(cancellationToken);

            var inventoryItem = inventoryItems.FirstOrDefault(x => x.CurrencyId == currencyId);

            if (inventoryItem != null)
            {
                await repository.DeleteAsync(inventoryItem, cancellationToken);
                await repository.CreateAsync(InventoryStock.CreateCurrency(currencyId, amount, actionType, invoiceId), cancellationToken);
            }
        }
        else
        {
            await repository.CreateAsync(InventoryStock.CreateCurrency(currencyId, amount, actionType, invoiceId), cancellationToken);
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

    public async Task UpdateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var oldStockItems = await repository
            .Get(new InventoryStocksByInvoiceIdSpecification(invoice.Id))
            .ToListAsync(cancellationToken);

        if (oldStockItems.Any())
        {
            await repository.DeleteRangeAsync(oldStockItems, cancellationToken);
        }

        var warehouseActionType = invoice.InvoiceType == InvoiceType.Purchase
            ? WarehouseActionType.In
            : WarehouseActionType.Out;

        var newStockItems = invoice.ProductItems.Select(productItem => InventoryStock.CreateProduct(
                productItem.ProductId,
                1,
                warehouseActionType,
                invoice.Id))
            .ToList();

        newStockItems.AddRange(invoice.CoinItems.Select(coinItem => InventoryStock.CreateCoin(coinItem.CoinId,
            coinItem.Quantity,
            warehouseActionType,
            invoice.Id)));

        newStockItems.AddRange(invoice.CurrencyItems.Select(currencyItem => InventoryStock.CreateCurrency(
            currencyItem.CurrencyId,
            currencyItem.Amount,
            warehouseActionType,
            invoice.Id)));

        if (newStockItems.Any())
        {
            await repository.CreateRangeAsync(newStockItems, cancellationToken);
        }
    }

    public async Task CreateInvoiceInventoryAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var warehouseActionType = invoice.InvoiceType == InvoiceType.Purchase
            ? WarehouseActionType.In
            : WarehouseActionType.Out;

        var newStockItems = invoice.ProductItems.Select(productItem => InventoryStock.CreateProduct(
                productItem.ProductId,
                productItem.TotalWeight,
                warehouseActionType,
                invoice.Id))
            .ToList();

        if (invoice.ProductItems.Any(x => x.IsInstantProduct))
        {
            newStockItems.AddRange(invoice.ProductItems
                .Where(x => x.IsInstantProduct)
                .Select(productItem => InventoryStock.CreateProduct(productItem.ProductId,
                    productItem.TotalWeight,
                    WarehouseActionType.In,
                    invoice.Id)));
        }

        newStockItems.AddRange(invoice.CoinItems.Select(coinItem => InventoryStock.CreateCoin(coinItem.CoinId,
            coinItem.Quantity,
            warehouseActionType,
            invoice.Id)));

        newStockItems.AddRange(invoice.CurrencyItems.Select(currencyItem => InventoryStock.CreateCurrency(
            currencyItem.CurrencyId,
            currencyItem.Amount,
            warehouseActionType,
            invoice.Id)));

        newStockItems.AddRange(invoice.UsedProducts
            .Where(usedProduct => usedProduct is { ProductId: not null })
            .Select(usedProduct => InventoryStock.CreateProduct(usedProduct.ProductId!.Value,
                usedProduct.Weight,
                WarehouseActionType.In,
                invoice.Id)));

        if (newStockItems.Any())
        {
            await repository.CreateRangeAsync(newStockItems, cancellationToken);
        }
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

        var product = await productService.FindOrCreateMoltenGoldProductAsync(fineness, cancellationToken);

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

            var rawPrice = CalculatorHelper.Product.CalculateRawPrice(item.CurrentQuantity, calculatorFilter.GramPrice, item.Product.Fineness,
                1, item.Product.ProductType);
            var wageAmount = CalculatorHelper.Product.CalculateWage(rawPrice, item.CurrentQuantity, item.Product.Wage, item.Product.WageType, null);
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

    #endregion
}