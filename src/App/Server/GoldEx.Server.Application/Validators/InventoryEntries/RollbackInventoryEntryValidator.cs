using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryEntries;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.InventoryEntries;

[ScopedService]
internal class RollbackInventoryEntryValidator : AbstractValidator<InventoryEntryId>
{
    private readonly IInventoryStockRepository _inventoryStockRepository;
    private readonly IInventoryEntryRepository _inventoryEntryRepository;

    public RollbackInventoryEntryValidator(IInventoryStockRepository inventoryStockRepository, IInventoryEntryRepository inventoryEntryRepository)
    {
        _inventoryStockRepository = inventoryStockRepository;
        _inventoryEntryRepository = inventoryEntryRepository;

        RuleFor(x => x)
            .MustAsync(NotResultInNegativeInventoryAsync)
            .WithMessage("بازگردانی این عملیات به دلیل منفی شدن موجودی امکان‌پذیر نیست.");
    }

    private async Task<bool> NotResultInNegativeInventoryAsync(InventoryEntryId inventoryEntryId, CancellationToken cancellationToken)
    {
        // 1. Load the Entry
        var inventoryEntry = await _inventoryEntryRepository
            .Get(new InventoryEntriesByIdSpecification(inventoryEntryId))
            .Include(x => x.InventoryStocks)
            .FirstOrDefaultAsync(cancellationToken);

        if (inventoryEntry == null)
            throw new NotFoundException("Inventory entry not found.");

        // If there are no stock movements, validation passes
        if (inventoryEntry.InventoryStocks == null || !inventoryEntry.InventoryStocks.Any())
            return true;

        // 2. GATHER: Collect IDs for batch fetching
        var productIds = inventoryEntry.InventoryStocks
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();

        var coinIds = inventoryEntry.InventoryStocks
            .Where(x => x.CoinId.HasValue)
            .Select(x => x.CoinId!.Value)
            .Distinct()
            .ToList();

        var currencyIds = inventoryEntry.InventoryStocks
            .Where(x => x.CurrencyId.HasValue)
            .Select(x => x.CurrencyId!.Value)
            .Distinct()
            .ToList();

        // 3. FETCH: Batch Query the database
        // We initialize empty dictionaries in case no IDs exist for a specific type
        var productStocks = productIds.Any()
            ? await _inventoryStockRepository.GetQuantitiesAsync(productIds, cancellationToken)
            : new Dictionary<ProductId, decimal>();

        var coinStocks = coinIds.Any()
            ? await _inventoryStockRepository.GetQuantitiesAsync(coinIds, cancellationToken)
            : new Dictionary<CoinId, decimal>();

        var currencyStocks = currencyIds.Any()
            ? await _inventoryStockRepository.GetQuantitiesAsync(currencyIds, cancellationToken)
            : new Dictionary<PriceUnitId, decimal>();

        // 4. VALIDATE: In-Memory Comparison
        foreach (var inventoryStock in inventoryEntry.InventoryStocks)
        {
            var entryAmount = inventoryStock.ChangeAmount;
            decimal currentStock = 0;

            // Determine which dictionary to look in
            if (inventoryStock.ProductId.HasValue)
            {
                productStocks.TryGetValue(inventoryStock.ProductId.Value, out currentStock);
            }
            else if (inventoryStock.CoinId.HasValue)
            {
                coinStocks.TryGetValue(inventoryStock.CoinId.Value, out currentStock);
            }
            else if (inventoryStock.CurrencyId.HasValue)
            {
                currencyStocks.TryGetValue(inventoryStock.CurrencyId.Value, out currentStock);
            }

            // If the stock record wasn't in the dictionary, it means the quantity is 0 (or record doesn't exist).
            // currentStock defaults to 0 via the 'out' parameter, so we are good.

            // LOGIC: If current stock is less than the amount we need to rollback
            if (currentStock < entryAmount)
            {
                return false;
            }
        }

        return true;
    }
}