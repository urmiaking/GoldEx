using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryEntries;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryEntryService(
    IInventoryEntryRepository inventoryEntryRepository,
    IInventoryStockRepository inventoryStockRepository,
    IServerProductService productService,
    IAccountingTransactionService transactionService,
    CreateInventoryEntryRequestValidator createValidator,
    ILogger<InventoryEntryService> logger) : IInventoryEntryService
{
    public async Task CreateAsync(CreateInventoryEntryRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await inventoryEntryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                // Step 1. Validate the request
                await createValidator.ValidateAndThrowAsync(request, cancellationToken);

                // Step 2. Create the opening inventory
                var inventoryEntry = InventoryEntry.Create();
                await inventoryEntryRepository.CreateAsync(inventoryEntry, cancellationToken);

                // Step 3. Create the inventory stock records
                foreach (var productItem in request.Products)
                {
                    var product = await productService.CreateProductAsync(productItem.Product, null, cancellationToken);

                    var inventoryStock = InventoryStock.CreateProduct(product.Id,
                        productItem.Quantity,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);
                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry, inventoryStock, product, productItem, cancellationToken);
                }

                foreach (var currencyItem in request.Currencies)
                {
                    var inventoryStock = InventoryStock.CreateCurrency(
                        new PriceUnitId(currencyItem.CurrencyId),
                        currencyItem.Amount,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);

                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry,
                        inventoryStock,
                        currencyItem,
                        cancellationToken);
                }

                foreach (var coinItem in request.Coins)
                {
                    var inventoryStock = InventoryStock.CreateCoin(new CoinId(coinItem.CoinId),
                        coinItem.Quantity,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);

                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry, inventoryStock, coinItem, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}