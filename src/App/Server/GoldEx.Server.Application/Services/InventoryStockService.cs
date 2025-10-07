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
    MeltUsedProductsValidator usedProductsValidator,
    CreateMoltenGoldRequestValidator moltenGoldRequestValidator) 
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
                productItem.Quantity,
                warehouseActionType,
                invoice.Id))
            .ToList();

        if (invoice.ProductItems.Any(x => x.IsInstantProduct))
        {
            newStockItems.AddRange(invoice.ProductItems
                .Where(x => x.IsInstantProduct)
                .Select(productItem => InventoryStock.CreateProduct(productItem.ProductId,
                    productItem.Quantity,
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
            .Where(usedProduct => usedProduct is { IsSellable: true, ProductId: not null })
            .Select(usedProduct => InventoryStock.CreateProduct(usedProduct.ProductId!.Value,
                usedProduct.Quantity,
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
            var inventoryStocks = InventoryStock.CreateProduct(productId, 1, WarehouseActionType.Out);
            inventoryItems.Add(inventoryStocks);
        }

        if (inventoryItems.Any())
            await repository.CreateRangeAsync(inventoryItems, cancellationToken);
    }

    public async Task CreateMoltenGoldAsync(MeltingBatch meltingBatch, string assayNumber, decimal fineness, decimal weight,
        CancellationToken cancellationToken = default)
    {
        await moltenGoldRequestValidator.ValidateAndThrowAsync((meltingBatch, assayNumber, fineness, weight), cancellationToken);

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

    public async Task<List<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var candidateProducts = await repository.GetAvailableProductsForCalculatorAsync(filter, cancellationToken);

        var results = new List<GetInventoryStockResponse>();

        foreach (var item in candidateProducts)
        {
            var rawPrice = CalculatorHelper.Product.CalculateRawPrice(item.Weight, filter.GramPrice, item.Fineness, 1, item.ProductType);
            var wageAmount = CalculatorHelper.Product.CalculateWage(rawPrice, item.Weight, item.Wage, item.WageType, null);
            var profitAmount = CalculatorHelper.Product.CalculateProfit(rawPrice, wageAmount, item.ProductType, filter.ProfitPercent);
            var taxAmount = CalculatorHelper.Product.CalculateTax(wageAmount, profitAmount, filter.TaxPercent, item.ProductType, null);

            var finalPrice = rawPrice + wageAmount + profitAmount + taxAmount;

            if ((!filter.MinPrice.HasValue || finalPrice >= filter.MinPrice.Value) &&
                (!filter.MaxPrice.HasValue || finalPrice <= filter.MaxPrice.Value))
            {
                results.Add(new GetInventoryStockResponse(
                    CurrentAmount: 1,
                    SoldAmount: 0,
                    DateTime.Now,
                    null,
                    null,
                    null,
                    mapper.Map<GetProductResponse>(item),
                    null,
                    null
                ));
            }
        }

        return results;
    }

    public async Task<List<GetInventoryWeightChartResponse>> GetInventoryWeightChartAsync(GoldUnitType targetUnit,
        CancellationToken cancellationToken = default)
    {
        var summary = await repository.GetInventoryWeightChartDataAsync(targetUnit, cancellationToken);

        return mapper.Map<List<GetInventoryWeightChartResponse>>(summary);
    }

    #endregion
}