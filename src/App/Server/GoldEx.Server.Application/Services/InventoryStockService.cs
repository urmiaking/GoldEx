using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryStockService(IInventoryStockRepository repository, IMapper mapper) : IServerInventoryStockService, IInventoryStockService
{
    #region Server Service

    public async Task SetForProductAsync(ProductId productId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
       CancellationToken cancellationToken = default)
    {
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
            await repository.CreateAsync(InventoryStock.CreateProduct(productId, quantity, actionType, invoiceId), cancellationToken);
        }

    }

    public async Task SetForCoinAsync(CoinId coinId, int quantity, WarehouseActionType actionType, InvoiceId? invoiceId,
        CancellationToken cancellationToken = default)
    {
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

    #endregion
}