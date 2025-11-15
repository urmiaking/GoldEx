using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryEntries;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryEntryService(
    IInventoryEntryRepository openingInventoryRepository,
    IInventoryStockRepository inventoryStockRepository,
    ICoinRepository coinRepository,
    IPriceUnitRepository priceUnitRepository,
    IProductRepository productRepository,
    IServerProductCategoryService productCategoryService,
    CreateInventoryEntryRequestValidator createValidator,
    ILogger<InventoryEntryService> logger) : IInventoryEntryService
{
    public async Task CreateAsync(CreateInventoryEntryRequest request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await openingInventoryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                // Step 1. Validate the request
                await createValidator.ValidateAndThrowAsync(request, cancellationToken);

                // Step 2. Create the opening inventory
                var inventoryEntry = InventoryEntry.Create();

                // Step 3. Create the products
                foreach (var stockRequest in request.Stocks)
                {
                    if (stockRequest.CoinItem is not null)
                    {
                        var inventoryStock = InventoryStock.CreateCoin(new CoinId(stockRequest.CoinItem.CoinId),
                            (int)stockRequest.Amount,
                            WarehouseActionType.In,
                            inventoryEntryId: inventoryEntry.Id);

                        await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);
                    }
                    else if (stockRequest.CurrencyItem is not null)
                    {
                        var inventoryStock = InventoryStock.CreateCurrency(
                            new PriceUnitId(stockRequest.CurrencyItem.CurrencyId),
                            stockRequest.Amount,
                            WarehouseActionType.In,
                            inventoryEntryId: inventoryEntry.Id);

                        await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);
                    }
                    else if (stockRequest.ProductItem is not null)
                    {
                        if (stockRequest.ProductItem.Type is ProductType.Gold or ProductType.Jewelry)
                        {
                            if (string.IsNullOrEmpty(stockRequest.ProductItem.Name))
                                throw new ArgumentException("Product name is required for gold and jewelry types.");

                            PriceUnitId? wagePriceUnitId = null;
                            if (!string.IsNullOrEmpty(stockRequest.ProductItem.WagePriceUnitTitle))
                            {
                                var priceUnit = await priceUnitRepository
                                    .Get(new PriceUnitsByTitleSpecification(stockRequest.ProductItem.WagePriceUnitTitle))
                                    .FirstOrDefaultAsync(cancellationToken) 
                                                ?? throw new NotFoundException($"Price unit not found for title: {stockRequest.ProductItem.WagePriceUnitTitle}");

                                wagePriceUnitId = priceUnit.Id;
                            }

                            var productCategory = await productCategoryService.GetOrCreateAsync(stockRequest.ProductItem.Category, cancellationToken);

                            var product = Product.Create(stockRequest.ProductItem.Name,
                                stockRequest.Amount,
                                stockRequest.ProductItem.Wage,
                                stockRequest.ProductItem.Type,
                                stockRequest.ProductItem.Fineness,
                                GoldUnitType.Gram,
                                stockRequest.ProductItem.WageType,
                                wagePriceUnitId,
                                null,
                                productCategory.Id);

                            // TODO: for product category we need to dynamically get or create categories by product name using AI
                        }


                        // await inventoryStockRepository.CreateAsync(inventoryStock, cancellationToken);
                    }
                }
                // Step 4. Create the inventory stocks

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