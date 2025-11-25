using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryEntries;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Services.Mappers;
using GoldEx.Server.Infrastructure.Specifications.InventoryEntries;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryEntryService(
    IInventoryEntryRepository inventoryEntryRepository,
    IInventoryStockRepository inventoryStockRepository,
    IServerProductService productService,
    IAccountingTransactionService transactionService,
    ISpreadsheetService spreadsheetService,
    IExcelProductProcessor excelProductProcessor,
    CreateInventoryEntryRequestValidator createValidator,
    RollbackInventoryEntryValidator rollbackValidator,
    IMapper mapper,
    ILogger<InventoryEntryService> logger) : IInventoryEntryService
{
    public async Task<PagedList<InventoryEntryResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var spec = new InventoryEntriesByFilterSpecification(filter);

        var list = await inventoryEntryRepository.Get(spec).ToListAsync(cancellationToken);
        var total = await inventoryEntryRepository.CountAsync(spec, cancellationToken);

        return new PagedList<InventoryEntryResponse>
        {
            Data = mapper.Map<List<InventoryEntryResponse>>(list),
            Total = total,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 100
        };
    }

    public async Task CreateAsync(CreateInventoryEntryRequest request)
    {
        await using var transaction = await inventoryEntryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        {
            try
            {
                logger.LogInformation(
                    $"Start creating inventory entry with {request.Products.Count} products, {request.Coins.Count} coins, {request.Currencies.Count} currencies.");

                // Step 1. Validate the request
                await createValidator.ValidateAndThrowAsync(request);

                // Step 2. Create the opening inventory
                var inventoryEntry = InventoryEntry.Create();
                await inventoryEntryRepository.CreateAsync(inventoryEntry);

                // Step 3. Create the inventory stock records
                var total = request.Products.Count;
                var index = 0;
                var sw = Stopwatch.StartNew();

                foreach (var productItem in request.Products)
                {
                    index++;

                    var product = await productService.CreateProductAsync(productItem.Product with { Id = null }, null);

                    var inventoryStock = InventoryStock.CreateProduct(
                        product.Id,
                        productItem.Product.Weight,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock);
                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry, inventoryStock, product, productItem);

                    // Log every 100 items
                    if (index % 100 == 0 || index == total)
                    {
                        logger.LogInformation(
                            "Processed {Index}/{Total} products in {Seconds:n2} seconds",
                            index,
                            total,
                            sw.Elapsed.TotalSeconds);
                    }
                }

                logger.LogInformation("Finished processing all {Total} products in {Seconds:n2} seconds", total, sw.Elapsed.TotalSeconds);

                foreach (var currencyItem in request.Currencies)
                {
                    var inventoryStock = InventoryStock.CreateCurrency(
                        new PriceUnitId(currencyItem.CurrencyId),
                        currencyItem.Amount,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock);

                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry,
                        inventoryStock,
                        currencyItem);
                }

                logger.LogInformation($"Processing {request.Currencies.Count} currencies done...");

                foreach (var coinItem in request.Coins)
                {
                    var inventoryStock = InventoryStock.CreateCoin(new CoinId(coinItem.CoinId),
                        coinItem.Quantity,
                        WarehouseActionType.In,
                        inventoryEntryId: inventoryEntry.Id);

                    await inventoryStockRepository.CreateAsync(inventoryStock);

                    await transactionService.CreateForInventoryEntryAsync(inventoryEntry, inventoryStock, coinItem);
                }

                logger.LogInformation($"Processing {request.Coins.Count} coins done...");

                await transaction.CommitAsync();

                logger.LogInformation($"Inventory entry {inventoryEntry.Id.Value} created successfully.");
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<ProcessExcelResponse> ProcessExcelAsync(ProcessExcelRequest request, CancellationToken cancellationToken = default)
    {
        var parsedResult = await spreadsheetService.ParseAsync(request.FileContent, new ExcelProductItemMapper(), cancellationToken: cancellationToken);

        var result = await excelProductProcessor.ProcessAsync(parsedResult, cancellationToken);

        return result;
    }

    public async Task RollbackAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await inventoryEntryRepository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await rollbackValidator.ValidateAndThrowAsync(new InventoryEntryId(id), cancellationToken);

                var inventoryEntry =
                    await inventoryEntryRepository
                        .Get(new InventoryEntriesByIdSpecification(new InventoryEntryId(id)))
                        .Include(x => x.InventoryStocks!)
                        .ThenInclude(x => x.Product)
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                var productList = inventoryEntry.InventoryStocks?
                    .Where(x => x.Product != null)
                    .Select(x => x.Product!)
                    .ToList() ?? [];

                if (productList.Any()) 
                    await productService.DeleteRangeAsync(productList, cancellationToken);

                await inventoryEntryRepository.DeleteAsync(inventoryEntry, cancellationToken);
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